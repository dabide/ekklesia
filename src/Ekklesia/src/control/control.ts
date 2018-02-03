import { autoinject, LogManager, observable } from 'aurelia-framework';
import { SignalRService } from 'common/signalr-service';
import { SongService } from 'common/song-service';

const logger = LogManager.getLogger('control');

@autoinject()
export class Control {
  currentSong: any;
  songAttributes: ['hymnNumber', 'title'];
  songLabel = song => `${song.title} (${song.hymnNumber})`;
  @observable songName: string;

  constructor(private signalRService: SignalRService, private songService: SongService) {
  }

  display(songPart: any) {
    for (let identifier of this.currentSong.presentation) {
      this.currentSong.lyrics[identifier].active = false;
    }
    songPart.active = true;
    this.signalRService.hubConnection.invoke('changeSongPart', { id: this.currentSong.id, partIdentifier: songPart.identifier });
  }

  songNameChanged(newValue: string) {
    logger.debug('songNameChanged', newValue);
    this.songService.getSong(newValue)
      .then(song => this.currentSong = song);
  }

  getSongs(filter: string, limit: number) {
    return this.songService.getSongs(filter, limit);
  }

  navigate(event: KeyboardEvent, index: number) {
    switch (event.code) {
      case "ArrowUp":
        if (index > 0) {
          this.display(this.currentSong.lyrics[this.currentSong.presentation[index - 1]]);
          (<HTMLElement>event.srcElement.previousElementSibling).focus();
        }
        event.stopPropagation();
        break;
      case "ArrowDown":
        if (index < this.currentSong.presentation.length - 1) {
          this.display(this.currentSong.lyrics[this.currentSong.presentation[index + 1]]);
          (<HTMLElement>event.srcElement.nextElementSibling).focus();
        }
        event.stopPropagation();
        break;
    }
  }
}
