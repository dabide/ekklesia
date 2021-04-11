import { Config, Rest } from 'aurelia-api';
import { autoinject, LogManager } from 'aurelia-framework';

const logger = LogManager.getLogger('youtube-service');

@autoinject()
export class YouTubeService {
  youTubeEndpoint: Rest;

  constructor(config: Config) {
    this.youTubeEndpoint = config.getEndpoint('noembed');
  }

  getInfo(url: string) {
    let encodedUrl = encodeURIComponent(url);
    return this.youTubeEndpoint.request('get', `embed?url=${url}&format=json`, null, { mode: 'cors' });
  }
}
