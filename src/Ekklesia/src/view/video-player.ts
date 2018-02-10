import { autoinject, LogManager } from 'aurelia-framework';
import { activationStrategy } from 'aurelia-router';
import * as Rx from 'rxjs';
import * as videojs from 'video.js';
import { Player } from 'videojs';
import 'videojs-youtube/dist/Youtube';
import { EventAggregator, Subscription } from 'aurelia-event-aggregator';
import { SignalRService } from 'common/signalr-service';

const logger = LogManager.getLogger('video-player');

@autoinject()
export class VideoPlayer {
  timeupdateSubscription: Rx.Subscription;
  video: Player;
  videoElement: HTMLVideoElement;
  subscriptions: Subscription[] = [];
  type: string;
  src: string;

  constructor(private signalRService: SignalRService, private eventAggregator: EventAggregator) {
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
    this.src = params.url;
    this.type = this.getVideoType(params.url);
  }

  bind() {
    this.video = videojs(this.videoElement, {
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
