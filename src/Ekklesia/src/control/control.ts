import { UrlHelper } from 'common/url-helper';
import { EventAggregator, Subscription } from 'aurelia-event-aggregator';
import { autoinject, LogManager, observable, TaskQueue } from 'aurelia-framework';
import * as webcast from 'webcaster/js/client';
import { SignalRService } from 'common/signalr-service';
import { SongService } from 'common/song-service';
import './control.scss';

const logger = LogManager.getLogger('control');

@autoinject()
export class Control {
  playing: boolean;
  progress: string;
  autocompleteHasFocus: boolean;
  singbackUrl: string;
  subscriptions: Subscription[];
  currentItem: any;
  fileInput: HTMLInputElement;
  @observable item: any;
  @observable files: File[];

  constructor(
    private signalRService: SignalRService,
    private songService: SongService,
    private eventAggregator: EventAggregator,
    private taskQueue: TaskQueue,
    private urlHelper: UrlHelper
  ) {
    this.subscriptions = [
      eventAggregator.subscribe('item:select', item => {
        logger.debug('Item selected', item);
        this.currentItem = item;
        if (item.url == null) {
          this.singbackUrl = `/api/singback/${item.hymnNumber}`;
        }
      }),
      eventAggregator.subscribe('video:control', message => {
        switch (message.action) {
          case 'event:ready':
            logger.debug('ready');
            this.currentItem.loaded = true;
            break;

          case 'event:play':
            this.playing = true;
            break;

          case 'event:pause':
          case 'event:ended':
            this.playing = false;
            break;

          case 'event:stop':
            this.playing = false;
            this.progress = '0';
            break;

          case 'event:progress':
            logger.debug('progress', message.value);
            this.progress = message.value;
            break;
        }
      })
    ];
  }

  canActivate() {
    navigator.mediaDevices.getUserMedia({ audio: true, video: false })
      .then(() => navigator.mediaDevices.enumerateDevices())
      .then(devices => {
        logger.debug('devices', devices.filter(d => d.kind === 'audioinput'));
      });
  }

  deactivate() {
    for (const subscription of this.subscriptions) {
      subscription.dispose();
    }
  }

  display(songPart: any) {
    for (let identifier of this.currentItem.presentation) {
      this.currentItem.lyrics[identifier].active = false;
    }
    songPart.active = true;
    this.signalRService.hubConnection.invoke('changeSongPart', { id: this.currentItem.id, partIdentifier: songPart.identifier });
  }

  browse(item: any) {
    this.signalRService.hubConnection.invoke('browse', { id: this.currentItem.url, url: this.currentItem.url, mime: this.currentItem.type });
  }

  play() {
    logger.debug('press play on tape');
    this.signalRService.hubConnection.invoke('controlVideo', { action: 'play' });
  }

  pause() {
    logger.debug('press pause on tape');
    this.signalRService.hubConnection.invoke('controlVideo', { action: 'pause' });
  }

  stop() {
    logger.debug('press stop on tape');
    this.signalRService.hubConnection.invoke('controlVideo', { action: 'stop' });
  }

  itemChanged(newValue: any) {
    logger.debug('itemChanged', newValue);
    if (newValue == null) return;

    if (typeof newValue === 'string') {
      this.songService.getSong(newValue)
        .then(song => this.enqueue(song));
    } else {
      this.urlHelper.enhance(newValue)
        .then(item => this.enqueue(item));
    }
  }

  enqueue(item: any) {
    this.eventAggregator.publish('item:enqueue', item);
    this.item = null;
    this.autocompleteHasFocus = false;
    this.taskQueue.queueMicroTask(() => {
      this.autocompleteHasFocus = true;
    });
  }

  getItems(filter: string, limit: number) {
    if (filter == null || filter.length === 0) return Promise.resolve([]);
    if (filter.startsWith('http')) return Promise.resolve([{ id: filter, name: filter, url: filter, icon: 'fas fa-globe' }]);
    return this.songService.getSongs(filter, limit);
  }

  navigate(event: KeyboardEvent, index: number) {
    switch (event.code) {
      case "ArrowUp":
        if (index > 0) {
          this.display(this.currentItem.lyrics[this.currentItem.presentation[index - 1]]);
          (<HTMLElement>event.srcElement.previousElementSibling).focus();
        }
        event.stopPropagation();
        break;
      case "ArrowDown":
        if (index < this.currentItem.presentation.length - 1) {
          this.display(this.currentItem.lyrics[this.currentItem.presentation[index + 1]]);
          (<HTMLElement>event.srcElement.nextElementSibling).focus();
        }
        event.stopPropagation();
        break;
    }
  }

  browseFiles() {
    this.fileInput.click();
  }

  filesChanged(newValue: File[]) {
    for (const file of newValue) {
      let url =  URL.createObjectURL(file);
      this.enqueue({
        id: url,
        url: url,
        type: file.type,
        title: file.name,
        icon: this.getIcon(file.type)
      });
    }

    this.files = [];
  }

  getIcon(type: string) {
    logger.debug('getIcon', type);
    let unknown: string = 'fas fa-file';
    if (type == null) return unknown;

    if (type.startsWith('image/')) return 'fas fa-file-image';
    if (type.startsWith('audio/')) return 'fas fa-file-audio';
    if (type.startsWith('video/')) return 'fas fa-file-video';

    return unknown;
  }
}
