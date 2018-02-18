import { inject, LogManager } from 'aurelia-framework';
import { EventAggregator } from 'aurelia-event-aggregator';
import { HubConnection } from '@aspnet/signalr-client';

const logger = LogManager.getLogger('signal-r-service');

@inject(EventAggregator)
export class SignalRService {
  connected: boolean;
  private eventAggregator: EventAggregator;
  hubConnection: HubConnection;

  constructor(eventAggregator: EventAggregator) {
    this.eventAggregator = eventAggregator;
    this.hubConnection = new HubConnection('/song');

    this.registerOnServerEvents();
  }

  startConnection(): void {
    this.hubConnection.start()
      .then(() => {
        logger.debug('Hub connection started');
        this.connected = true;
      })
      .catch(err => {
        logger.debug('Error while establishing connection');
        this.connected = false;
      });
  }

  send(methodName: string, message: any) : Promise<any> {
    if (this.connected) {
      return this.hubConnection.invoke(methodName, message);
    } else {
      logger.warn('Couldn\'t send message, not connected.');
      // return Promise.reject('Not connected');
    }
  }

  private registerOnServerEvents(): void {
    this.hubConnection.on('send', (message: any) => {
      logger.debug('Message received', message);
      this.eventAggregator.publish(message.type, message);
    });
  }
}
