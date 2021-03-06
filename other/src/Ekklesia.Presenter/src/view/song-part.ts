import { bindable, autoinject, LogManager, TaskQueue, Animator } from 'aurelia-framework';
import textFit from 'textfit';
import './song-part.scss'

const logger = LogManager.getLogger('song-part');

@autoinject()
export class SongPart {
  lineContainer: HTMLElement;
  @bindable lines: any;

  constructor(private taskQueue: TaskQueue, private animator: Animator) {
  }

  attached() {
    textFit(this.lineContainer, { alignHoriz: true, alignVert: true, multiLine: true, widthOnly: true });
    this.taskQueue.queueTask(() => {
      return this.animator.enter(this.lineContainer);
    });
  }
}
