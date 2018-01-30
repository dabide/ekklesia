import { inject, LogManager } from 'aurelia-framework';
import { Router } from 'aurelia-router';

const logger = LogManager.getLogger('home');

@inject(Router)
export class Home {
  router: Router;
  
  constructor(router: Router) {
    this.router = router;
  }
}
