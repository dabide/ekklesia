import { containerless, bindable, LogManager, bindingMode, TaskQueue } from 'aurelia-framework';
import $ from 'jquery';

const logger = LogManager.getLogger('checkbox-button');

@containerless
export class CheckboxButton {
  checkboxElement: HTMLInputElement;
  checkboxButtonElement: HTMLButtonElement;
  settingFromHere: boolean;
  @bindable({ defaultBindingMode: bindingMode.twoWay }) checked;

  constructor(private taskQueue: TaskQueue) { }

  bind() {
    $(this.checkboxElement).change(() => {
      logger.debug('changed', this.checkboxElement.checked);
      this.taskQueue.queueMicroTask(() => {
        if (this.settingFromHere) {
          this.settingFromHere = false;
        } else {
          this.settingFromHere = true;
          this.checked = this.checkboxElement.checked;
        }
      });
    });
  }

  checkedChanged(newValue) {
    logger.debug('checkedChanged', newValue, this.settingFromHere);
    if (!this.settingFromHere && this.checkboxElement.checked != newValue) {
      logger.debug('toggling');
      this.settingFromHere = true;
      $(this.checkboxButtonElement).button('toggle');
    } else {
      this.settingFromHere = false;
    }
  }
}
