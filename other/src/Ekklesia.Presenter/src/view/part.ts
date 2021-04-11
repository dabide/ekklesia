import { activationStrategy } from 'aurelia-router';
import { SongPart } from './song-part';
import { ViewerContext } from './viewer-context';
import { autoinject, LogManager, Container } from 'aurelia-framework';

const logger = LogManager.getLogger('part');

@autoinject
export class Part {
  songPart: any;

  constructor(private viewerContext: ViewerContext) {
  }

  determineActivationStrategy() {
    return activationStrategy.replace;
  }
  
  activate(params: any) {
    if (this.viewerContext.song !== undefined) {
      this.songPart = this.viewerContext.song.lyrics[params.part];
      logger.debug('set song part', this.viewerContext.song, this.songPart);
    }
  }
}
