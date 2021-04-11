import { bindable } from 'aurelia-framework';
export class PlaybackControlsCustomElement {
  @bindable play;
  @bindable pause;
  @bindable stop;
  @bindable progress;
  @bindable playing: boolean;
  @bindable disabled: boolean;
}
