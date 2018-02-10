import { autoinject, TaskQueue, LogManager } from 'aurelia-framework';
import videojs from 'video.js';
import 'videojs-youtube/dist/Youtube';
import { EventAggregator, Subscription } from 'aurelia-event-aggregator';

const logger = LogManager.getLogger('video-player');

@autoinject()
export class VideoPlayer {
  video: any;
  videoElement: HTMLVideoElement;
  subscriptions: Subscription[] = [];
  type: string;
  src: string;

  constructor(private taskQueue: TaskQueue, private eventAggregator: EventAggregator) { 
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
          break;
      }
    }));
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
  }

  deactivate() {
    for (const subscription of this.subscriptions) {
      subscription.dispose();
    }

    this.video.dispose();
  }

  getVideoType(url: string) {
    return 'video/youtube';
  }
}
