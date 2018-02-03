import { autoinject, LogManager, observable } from 'aurelia-framework';
import { HttpClient, json } from 'aurelia-fetch-client';
import { Rest, Config } from 'aurelia-api';
import { SignalRService } from 'common/signalr-service';

const logger = LogManager.getLogger('control');

@autoinject()
export class Control {
  apiEndpoint: Rest;
  currentSong: any;
  songAttributes: ['hymnNumber', 'title'];
  songLabel = song => `${song.title} (${song.hymnNumber})`;
  @observable songName: string;
  signalRService: SignalRService;
  httpClient: HttpClient;

  constructor(signalRService: SignalRService, httpClient: HttpClient, config: Config) {
    this.signalRService = signalRService;
    this.httpClient = httpClient;
    this.apiEndpoint = config.getEndpoint('api');
  }

  display(songPart: any) {
    this.signalRService.hubConnection.invoke('changeSongPart', { songPart: songPart });
  }

  getSongs(filter: string, limit: number) {
    if (limit == null) limit = 10;
    logger.debug('getSongs', filter);
    return this.httpClient.fetch(`song-search/${filter}?limit=${limit}`)
      .then(data => {
        logger.debug('data', data); 
        return data.json();
      })
      .then(data => {
        logger.debug('data', data); 
        return data.songNames;
      });
  }

  songNameChanged(newValue: string) {
    logger.debug('songNameChanged', newValue);
    this.httpClient.fetch(`song/${newValue}`)
      .then(data => {
        logger.debug('data', data); 
        return data.json();
      })
      .then(data => {
        logger.debug('data', data);
        this.currentSong = data.song;
      });
  }
}
