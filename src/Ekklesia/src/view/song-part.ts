import { bindable, autoinject, LogManager, TaskQueue, Animator } from 'aurelia-framework';
import textFit from 'textfit';
import './song-part.scss'

const logger = LogManager.getLogger('song-part');

@autoinject()
export class SongPart {
  lineContainer: Element;
  show: boolean;
  @bindable lines: any;

  constructor(private taskQueue: TaskQueue, private animator: Animator) {
  }

  attached() {
    textFit(this.lineContainer, { alignHoriz: true, alignVert: true });
    this.taskQueue.queueTask(() => {
      this.animator.enter(this.lineContainer);
    });
  }
}
