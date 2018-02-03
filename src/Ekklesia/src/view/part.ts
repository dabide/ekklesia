import { activationStrategy } from 'aurelia-router';
import { SongPart } from './song-part';
import { SongContext } from './song-context';
import { autoinject, LogManager, Container } from 'aurelia-framework';

const logger = LogManager.getLogger('part');

@autoinject
export class Part {
  songPart: any;

  constructor(private songContext: SongContext, private container: Container) {
    logger.debug('constructor');
  }

  determineActivationStrategy() {
    return activationStrategy.replace;
  }
  
  activate(params: any) {
    if (this.songContext.song !== undefined) {
      this.songPart = this.songContext.song.lyrics[params.part];
      logger.debug('set song part', this.songContext.song, this.songPart);
    }
  }
}
