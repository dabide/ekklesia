import { autoinject, LogManager } from 'aurelia-framework';
import { Rest, Config } from 'aurelia-api';

const logger = LogManager.getLogger('song-service');

@autoinject()
export class SongService {
  apiEndpoint: Rest;

  constructor(config: Config) {
    this.apiEndpoint = config.getEndpoint('api');
  }

  getSong(songName: string): Promise<any> {
    return this.apiEndpoint.findOne('song', songName)
      .then(data => {
        logger.debug('data', data);
        return data.song;
      });
  }

  getSongs(filter: string, limit: number): Promise<any> {
    if (limit == null) limit = 10;
    logger.debug('getSongs', filter);
    return this.apiEndpoint.find('song-search', filter, { limit: limit })
      .then(data => {
        logger.debug('data', data);
        return data.songNames;
      });
  }
}
