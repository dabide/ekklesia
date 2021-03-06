import { SongService } from 'common/song-service';
import { ViewerContext } from './viewer-context';
import { autoinject, LogManager, PLATFORM, Container } from 'aurelia-framework';
import { EventAggregator, Subscription } from 'aurelia-event-aggregator';
import screenfull from 'screenfull';
import './view.scss';
import { RouterConfiguration, Router, RouteConfig, NavigationInstruction } from 'aurelia-router';

const logger = LogManager.getLogger('view');

@autoinject()
export class View {
  viewerContext: ViewerContext;
  router: Router;
  notFullscreen: boolean = true;
  songPart: any;
  cleared: boolean = false;
  blackened: boolean = false;

  private _subscriptions = [];

  constructor(private eventAggregator: EventAggregator) {
    this._subscriptions.push(eventAggregator.subscribe('blank', message => {
      logger.debug('Blanking out view', message);
      switch (message.mode) {
        case 'clear':
          this.cleared = message.on;
          break;

        case 'black':
          this.blackened = message.on;
          break;
      }
    }));

    this._subscriptions.push(eventAggregator.subscribe('song:change_part', message => {
      logger.debug('Changing song part', message);
      this.router.parent.navigate(`view/song/${message.id}/${message.partIdentifier}`);
    }));

    this._subscriptions.push(eventAggregator.subscribe('url:browse', message => {
      logger.debug('Browsing to URL', message);
      if (message.mime.startsWith('audio/') || message.mime.startsWith('video/')) {
        this.router.parent.navigate(`view/video?url=${encodeURIComponent(message.url)}&mime=${encodeURIComponent(message.mime)}`);
      } else if (message.mime.startsWith('image/')) {
        this.router.parent.navigate(`view/image?url=${encodeURIComponent(message.url)}&mime=${encodeURIComponent(message.mime)}`);
      } else {
        this.router.parent.navigate(`view/browse?url=${encodeURIComponent(message.url)}`);
      }
    }));
  }

  configureRouter(config: RouterConfiguration, router: Router) {
    logger.debug('configureRouter', config, router);
    config.title = 'Ekklesia';
    config.map([
      { route: ['', 'song', 'song/:song'], name: 'song-view', moduleId: PLATFORM.moduleName('./song-view'), nav: true, title: 'Part' },
      { route: ['browse'], name: 'web-view', moduleId: PLATFORM.moduleName('./browser'), nav: true, title: 'Browser' },
      { route: ['video'], name: 'video-player', moduleId: PLATFORM.moduleName('./video-player'), nav: true, title: 'Video Player' },
      { route: ['image'], name: 'image-viewer', moduleId: PLATFORM.moduleName('./image-viewer'), nav: true, title: 'Image Player' }
    ]);

    this.router = router;
  }

  canActivate(params: any, routeConfig: RouteConfig, navigationInstruction: NavigationInstruction) {
    logger.debug('canActivate', params, routeConfig, navigationInstruction);

    if (screenfull.enabled) {
      screenfull.on('change', () => {
        logger.debug('screenfull changed', screenfull.isFullscreen);
        this.notFullscreen = !screenfull.isFullscreen;
      });
    }
  }

  deactivate() {
    for (const subscription of this._subscriptions) {
      subscription.dispose();
    }
  }

  fullScreen() {
    if (screenfull.enabled) {
      screenfull.request();
    }
  }
}
