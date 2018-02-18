import { autoinject, LogManager, TaskQueue, Task } from 'aurelia-framework';
import { activationStrategy } from 'aurelia-router';
import * as Rx from 'rxjs/Rx';
import * as videojs from 'video.js';
import { Player } from 'videojs';
import 'videojs-youtube/dist/Youtube';
import { EventAggregator, Subscription } from 'aurelia-event-aggregator';
import { SignalRService } from 'common/signalr-service';
import './video-player.scss';

const logger = LogManager.getLogger('video-player');

@autoinject()
export class VideoPlayer {
  timeupdateSubscription: Rx.Subscription;
  video: Player;
  isVideo: boolean;
  videoElement: HTMLVideoElement;
  audioElement: HTMLAudioElement;
  subscriptions: Subscription[] = [];
  type: string;
  src: string;

  constructor(private signalRService: SignalRService, private eventAggregator: EventAggregator, private taskQueue: TaskQueue) {
    this.subscriptions.push(eventAggregator.subscribe('video:control', message => {
      switch (message.action) {
        case 'play':
          logger.debug('Playing video');
          this.video.play();
          break;
        case 'pause':
          logger.debug('Pausing video');
          this.video.pause();
          break;
        case 'stop':
          logger.debug('Stopping video');
          this.video.pause();
          this.video.currentTime(0);
          this.broadcastAction('event:stop');
          break;
      }
    }));
  }

  determineActivationStrategy() {
    return activationStrategy.replace;
  }

  canActivate(params: any) {
    logger.debug('canActivate', params);
    this.src = params.url;
    this.type = params.mime;
    this.isVideo = params.mime.startsWith('video/');
  }

  attached() {
    logger.debug('initializing videojs');
    let element = this.isVideo ? this.videoElement : this.audioElement;
    this.video = videojs(element, {
      controls: false,
      autoplay: false,
      preload: 'auto',
      textTrackSettings: false
    });

    this.timeupdateSubscription = Rx.Observable.fromEventPattern(handler => this.video.on('timeupdate', () => handler()), handler => this.video.off('timeupdate'))
      .throttleTime(500)
      .subscribe(event => {
        let progress = this.video.currentTime() * 100 / this.video.duration();
        this.broadcastAction('event:progress', progress.toString());
      });

    for (const event of ['loadstart', 'firstplay', 'waiting', 'canplay', 'canplaythrough', 'play', 'playing', 'seeking', 'seeked', 'pause', 'ended', 'fullscreenchange', 'error']) {
      this.video.on(event, () => {
        logger.debug(`Player event: ${event}`)
        this.broadcastAction(`event:${event}`);
      });
    }

    this.video.ready(() => {
      logger.debug('Player ready');
      this.broadcastAction('event:ready');
    });
  }

  broadcastAction(action: string, value?: string) {
    this.signalRService.hubConnection.invoke('controlVideo', { action: action, value: value });
  }

  deactivate() {
    for (const subscription of this.subscriptions) {
      subscription.dispose();
    }
    this.timeupdateSubscription.unsubscribe();
    this.video.off();
    this.video.dispose();
  }

  getVideoType(url: string) {
    return 'video/youtube';
  }
}
