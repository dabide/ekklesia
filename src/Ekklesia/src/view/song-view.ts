import { autoinject, LogManager, Container } from 'aurelia-framework';
import { RouterConfiguration, Router } from "aurelia-router";
import { PLATFORM } from 'aurelia-pal';
import { SongService } from 'common/song-service';
import { ViewerContext } from './viewer-context';

const logger = LogManager.getLogger('song-view');

@autoinject()
export class SongView {
  viewerContext: any;
  router: Router;
  constructor(private container: Container, private songService: SongService) {
    this.viewerContext = new ViewerContext();
    this.container.registerInstance(ViewerContext, this.viewerContext);
  }

  configureRouter(config: RouterConfiguration, router: Router) {
    logger.debug('configureRouter', config, router);
    config.title = 'Ekklesia';
    config.map([
      { route: ['', ':part'], name: 'song-part', moduleId: PLATFORM.moduleName('./part'), nav: true, title: 'Part' }
    ]);

    this.router = router;
  }

  canActivate(params: any) {
    if (params.song !== undefined) {
      return this.songService.getSong(params.song)
        .then(song => {
          this.viewerContext.song = song;
        });
    }
  }
}
