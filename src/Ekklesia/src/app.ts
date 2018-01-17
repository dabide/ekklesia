import { inject, LogManager } from 'aurelia-framework';
import { SignalRService } from 'common/signalr-service';
import './app.scss';

const logger = LogManager.getLogger('app');

@inject(SignalRService)
export class App {
  signalRService: SignalRService;
  message = 'Hello World!';

  constructor(signalRService : SignalRService) {
    this.signalRService = signalRService;
    signalRService.startConnection();
  }

  send() {
    this.signalRService.hubConnection.invoke('send', { message: 'Hello'});
  }
}
;
