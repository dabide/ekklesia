import { autoinject, LogManager, bindable, bindingMode, observable, TaskQueue } from 'aurelia-framework';
import $ from 'jquery';

const logger = LogManager.getLogger('autocomplete');

@autoinject()
export class AutocompleteCustomElement {
  settingValue: boolean;
  taskQueue: TaskQueue;
  isOpen: boolean;
  dropdownElement: Element;
  toggleElement: Element;
  @bindable limit: number = 10;
  @bindable({ defaultBindingMode: bindingMode.twoWay }) value: any;
  @observable searchText: any;
  @bindable data;
  @bindable placeholder;

  items = [];

  constructor(taskQueue: TaskQueue) {
    this.taskQueue = taskQueue;
  }

  bind() {
    $(this.dropdownElement)
      .on('show.bs.dropdown', () => {
        return this.items.length > 0;
      })
      .on('shown.bs.dropdown', () => {
        this.isOpen = true;
      })
      .on('hide.bs.dropdown', () => {
        return true;
      })
      .on('hidden.bs.dropdown', () => {
        this.isOpen = false;
      });
  }

  searchTextChanged(newValue) {
    if (this.settingValue) return;

    if (this.data instanceof Function) {
      logger.debug('data is a function');

      this.data({ filter: newValue, limit: this.limit })
        .then(result => {
          this.handleData(result);
        });
    } else {
      this.handleStatic(this.data);
    }

    if (!this.isOpen) {
      logger.debug('toggleElement', this.toggleElement);
      $(this.toggleElement).dropdown('toggle');
    }
  }

  setValue(item) {
    this.settingValue = true;
    this.value = item.value;
    this.searchText = typeof item.value === 'string' ? item.value : item.value.name;

    this.taskQueue.queueMicroTask(() => {
      this.settingValue = false;
    })
  }

  handleData(result) {
    let newItems = [];
    let regex = new RegExp(`(${this.searchText})`, "gi");
    for (let item of result) {
      logger.debug('item', item);
      if (typeof item === 'string') {
        newItems.push({ name: this.emphasize(regex, item), value: item });
      } else {
        newItems.push({ name: this.emphasize(regex, item.name), value: item });
      }
    }

    this.items = newItems;
  }

  handleStatic(data) {

  }

  emphasize(regex: RegExp, name: string) {
    return name.replace(regex, "<b>$1</b>");
  }

  lookup(event: any) {

    return true;
  }
}
