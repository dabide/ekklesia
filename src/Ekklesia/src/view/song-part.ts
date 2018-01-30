import { bindable, autoinject, TaskQueue, LogManager, noView, ViewFactory, ViewCompiler, ViewSlot, Container, ViewResources } from 'aurelia-framework';
import textFit from 'textfit';
import './song-part.scss'

const logger = LogManager.getLogger('song-part');

@noView()
@autoinject()
export class SongPart {
  viewSlot: ViewSlot;
  container: Container;
  viewFactory: ViewFactory;
  lineContainer: Element;
  taskQueue: TaskQueue;
  @bindable lines: any;

  constructor(taskQueue: TaskQueue, viewCompiler: ViewCompiler, viewSlot: ViewSlot, container: Container, viewResources: ViewResources) {
    this.taskQueue = taskQueue;
    this.viewSlot = viewSlot;
    this.container = container;
    this.viewFactory = viewCompiler.compile(
      '<template>'
      + '  <div ref="lineContainer" class="line-container">'
      + '    <span repeat.for="line of lines">${line}<br if.bind="!$last"></span>'
      + '  </div>'
      + '</template>', viewResources);
  }

  linesChanged(newValue: any) {
    logger.debug('linesChanged', newValue);
    let view = this.viewFactory.create(this.container);
    view.bind(this);
    this.viewSlot.removeAll();
    this.viewSlot.add(view);
    this.viewSlot.attached();

    this.taskQueue.queueMicroTask(() => {
      textFit(this.lineContainer, { alignHoriz: true, alignVert: true });
    });
  }
}
