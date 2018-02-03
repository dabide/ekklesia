import { autoinject } from 'aurelia-framework';
import { EventAggregator } from 'aurelia-event-aggregator';

@autoinject()
export class Queue {
  currentItem: any;
  items = [];
  subscriptions;

  constructor(private eventAggregator: EventAggregator) {
    this.subscriptions = [
      eventAggregator.subscribe('song:enqueue', song => { this.items.push(song); })
    ];
  }

  select(item) {
    this.eventAggregator.publish('song:select', item);
  }

  deactivate() {
    for (const subscription of this.subscriptions) {
      subscription.dispose();
    }
  }
}
