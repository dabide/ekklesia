import { activationStrategy } from "aurelia-router";

export class ImageViewer {
  src: string;
  
  determineActivationStrategy() {
    return activationStrategy.replace;
  }

  canActivate(params) {
    this.src = params.url;
  }
}
