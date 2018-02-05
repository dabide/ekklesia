import { SongService } from 'common/song-service';
import { SongContext } from './song-context';
import { autoinject, LogManager, PLATFORM, Container } from 'aurelia-framework';
import { EventAggregator } from 'aurelia-event-aggregator';
import screenfull from 'screenfull';
import './view.scss';
import { RouterConfiguration, Router, RouteConfig, NavigationInstruction } from 'aurelia-router';

const logger = LogManager.getLogger('view');

@autoinject()
export class View {
  songContext: SongContext;
  router: Router;
  notFullscreen: boolean = true;
  songPart: any;
  private _subscriptions = [];

  constructor(private eventAggregator: EventAggregator, private container: Container, private songService: SongService) {
    logger.debug('constructor');
    this.songContext = new SongContext();
    this.container.registerInstance(SongContext, this.songContext);
    this._subscriptions.push(eventAggregator.subscribe('song:change_part', message => {
      logger.debug('Changing song part', message);
      this.router.parent.navigate(`view/${message.id}/${message.partIdentifier}`);
    }));
  }

  configureRouter(config: RouterConfiguration, router: Router) {
    logger.debug('configureRouter', config, router);
    config.title = 'Ekklesia';
    config.map([
      { route: ['', ':part'], name: 'song-part', moduleId: PLATFORM.moduleName('./part'), nav: true, title: 'Part' }
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

    if (params.song !== undefined) {
      return this.songService.getSong(params.song)
        .then(song => {
          this.songContext.song = song;
        });
    }
  }

  fullScreen() {
    if (screenfull.enabled) {
      screenfull.request();
    }
  }
}
