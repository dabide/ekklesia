import { autoinject, LogManager } from 'aurelia-framework';
import { EventAggregator } from 'aurelia-event-aggregator';
import screenfull from 'screenfull';
import './view.scss';

const logger = LogManager.getLogger('view');

@autoinject()
export class View {
  notFullscreen: boolean = true;
  songPart: any;
  private _subscriptions = [];
  eventAggregator: EventAggregator;
  
  constructor(eventAggregator: EventAggregator) {
    this.eventAggregator = eventAggregator;

    this._subscriptions.push(eventAggregator.subscribe('song:change_part', message => {
      logger.debug('Changing song part', message);
      this.songPart = message.songPart;
    }));
  }

  activate() {
    if (screenfull.enabled) {
      screenfull.on('change', () => {
        logger.debug('screenfull changed', screenfull.isFullscreen);
        this.notFullscreen = !screenfull.isFullscreen; 
      });
    }
  }

  fullScreen() {
    if (screenfull.enabled) {
      screenfull.request();
    }
  }
}
