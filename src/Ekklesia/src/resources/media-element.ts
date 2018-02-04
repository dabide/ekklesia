import { bindable, LogManager, } from 'aurelia-framework';
import 'mediaelement/full';
import './media-element.scss';
import 'mediaelement/build/mediaelementplayer.css';
import 'mediaelement/build/mediaelement-flash-video.swf';

declare const MediaElementPlayer: any;

export class MediaElementCustomElement {
  player: any;
  audioElement: HTMLAudioElement;
  audioSourceElement; HTMLSourceElement;

  @bindable url;

  bind() {
    this.player = new MediaElementPlayer(this.audioElement);
  }

  urlChanged(newValue) {
    this.player.setSrc(newValue);
  }
}
