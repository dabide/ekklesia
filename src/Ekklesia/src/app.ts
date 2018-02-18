import { inject, LogManager } from 'aurelia-framework';
import { Router, RouterConfiguration } from 'aurelia-router';
import { PLATFORM } from 'aurelia-pal';
import { HttpClient } from 'aurelia-fetch-client';
import { SignalRService } from 'common/signalr-service';
import screenfull from 'screenfull';
import './app.scss';

const logger = LogManager.getLogger('app');

@inject(SignalRService, HttpClient)
export class App {
  signalRService: SignalRService;
  router: Router;
  message = 'Hello World!';
  currentSong: any;

  constructor(signalRService: SignalRService, httpClient: HttpClient) {
    this.signalRService = signalRService;
    signalRService.startConnection();

    httpClient.configure(config => {
      config
        .withBaseUrl('api/')
        .withDefaults({
          credentials: 'same-origin',
          headers: {
            'Accept': 'application/json',
            'X-Requested-With': 'Fetch'
          }
        })
    });
  }

  configureRouter(config: RouterConfiguration, router: Router) {
    config.title = 'Ekklesia';
    config.map([
      { route: '', name: 'home', moduleId: PLATFORM.moduleName('home/home'), nav: true, title: 'Home' },
      { route: 'control', name: 'control', moduleId: PLATFORM.moduleName('control/control'), nav: true, title: 'Control' },
      { route: 'view', name: 'view', moduleId: PLATFORM.moduleName('view/view'), nav: true, title: 'View', settings: { target: '_blank' } }
    ]);

    this.router = router;
  }
}
