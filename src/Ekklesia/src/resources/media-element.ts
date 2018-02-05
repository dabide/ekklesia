import { bindable, LogManager, } from 'aurelia-framework';
import { Howl } from 'howler';

const logger = LogManager.getLogger('media-element');

export class MediaElementCustomElement {
  progress: number;
  duration: number;
  playing: boolean;
  howl: Howl;
  audio: HTMLAudioElement;
  player: any;
  audioElement: HTMLAudioElement;
  audioSourceElement; HTMLSourceElement;

  @bindable url;

  bind() {
    this.createPlayer();
  }

  createPlayer() {
    this.howl = new Howl({
      src: [this.url],
      format: ['mp3'],
      onplay: id => {
        this.duration = this.howl.duration();
        this.playing = true;
        requestAnimationFrame(() => this.setProgress());
      },
      onpause: id => this.playing = false,
      onend: id => this.playing = false,
      onstop: id => {
        this.playing = false;
        this.progress = 0;
      }
    });
  }

  urlChanged(newValue) {
    if (this.howl != null) {
      this.howl.unload();
    }

    this.createPlayer();
  }

  play() {
    logger.debug('play', this.howl);
    this.howl.play();
  }

  pause() {
    this.howl.pause();
  }

  stop() {
    this.howl.stop();
  }

  setProgress() {
    if (!this.playing || this.duration == 0) return;

    this.progress = <number>this.howl.seek() * 100 / this.duration;
    requestAnimationFrame(() => this.setProgress());
  }
}
