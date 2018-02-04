import { EventAggregator, Subscription } from 'aurelia-event-aggregator';
import { autoinject, LogManager, observable, TaskQueue } from 'aurelia-framework';
import { SignalRService } from 'common/signalr-service';
import { SongService } from 'common/song-service';

const logger = LogManager.getLogger('control');

@autoinject()
export class Control {
  autocompleteHasFocus: boolean;
  subscriptions: Subscription[];
  currentSong: any;
  songAttributes: ['hymnNumber', 'title'];
  songLabel = song => `${song.title} (${song.hymnNumber})`;
  @observable songName: string;

  constructor(private signalRService: SignalRService, private songService: SongService, private eventAggregator: EventAggregator, private taskQueue: TaskQueue) {
    this.subscriptions = [
      eventAggregator.subscribe('song:select', song => { this.currentSong = song; })
    ];
  }

  deactivate() {
    for (const subscription of this.subscriptions) {
      subscription.dispose();
    }
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
    if (newValue == null) return;

    this.songService.getSong(newValue)
      .then(song => {
        this.eventAggregator.publish('song:enqueue', song);
        this.songName = null;
        this.autocompleteHasFocus = false;
        this.taskQueue.queueMicroTask(() => {
          this.autocompleteHasFocus = true;
        });
      });
  }

  getSongs(filter: string, limit: number) {
    if (filter == null || filter.length === 0) return Promise.resolve([]);
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
