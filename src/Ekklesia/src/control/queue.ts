import { YouTubeService } from 'common/youtube-service';
import { autoinject, LogManager } from 'aurelia-framework';
import { EventAggregator } from 'aurelia-event-aggregator';
import { moveBefore } from 'aurelia-dragula';

const logger = LogManager.getLogger('queue');

@autoinject()
export class Queue {
  currentItem: any;
  items = [];
  subscriptions;

  constructor(private eventAggregator: EventAggregator, private youTubeService: YouTubeService) {
    this.subscriptions = [
      eventAggregator.subscribe('item:enqueue', item => { this.items.push(item); })
    ];
  }

  select(item) {
    this.currentItem = item;
    this.eventAggregator.publish('item:select', item);
  }

  itemDropped(item, target, source, sibling, itemVM, siblingVM) {
    logger.debug('dropped', item, target, source, sibling, itemVM, siblingVM);
    let itemId = item.dataset.id;
    let siblingId = sibling ? sibling.dataset.id : null;
    moveBefore(this.items, i => i.id == itemId, s => s.id == siblingId);
    logger.debug('items', this.items);
  }

  navigate(event: KeyboardEvent, index: number) {
    switch (event.code) {
      case "ArrowUp":
        if (index > 0) {
          this.select(this.items[index - 1]);
          (<HTMLElement>event.srcElement.previousElementSibling).focus();
        }
        event.stopPropagation();
        break;
      case "ArrowDown":
        if (index < this.items.length - 1) {
          this.select(this.items[index + 1]);
          (<HTMLElement>event.srcElement.nextElementSibling).focus();
        }
        event.stopPropagation();
        break;
    }
  }

  remove(item: any) {
    this.items = this.items.filter(i => i.id !== item.id);
    if (this.currentItem === item) {
      this.select(null);
    }
  }

  deactivate() {
    for (const subscription of this.subscriptions) {
      subscription.dispose();
    }
  }
}
