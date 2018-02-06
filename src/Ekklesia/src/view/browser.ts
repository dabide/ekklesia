import { autoinject, LogManager } from 'aurelia-framework';
import { activationStrategy } from 'aurelia-router';
import { ViewerContext } from "./viewer-context";
import './browser.scss';

const logger = LogManager.getLogger('browser');

@autoinject()
export class Browser {
  url: string;

  constructor(private viewerContext: ViewerContext) {
  }

  determineActivationStrategy() {
    return activationStrategy.replace;
  }

  activate(params: any) {
    logger.debug('activate', params);    
    this.url = params.url;
  }
}
