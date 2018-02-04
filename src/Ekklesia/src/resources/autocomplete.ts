import { inject, LogManager, bindable, bindingMode, observable, TaskQueue, Optional, BindingEngine, Disposable } from 'aurelia-framework';
import { Focus } from 'aurelia-templating-resources';
import $ from 'jquery';

const logger = LogManager.getLogger('autocomplete');

@inject(TaskQueue, Optional.of(Focus), BindingEngine)
export class AutocompleteCustomElement {
  focusSubscription: Disposable;
  settingValue: boolean;
  notFound: boolean;
  isOpen: boolean;
  dropdownElement: Element;
  toggleElement: HTMLInputElement;
  @bindable limit: number = 10;
  @bindable({ defaultBindingMode: bindingMode.twoWay }) value: any;
  @observable searchText: any;
  @bindable data;
  @bindable placeholder;

  items = [];

  constructor(private taskQueue: TaskQueue, private focusCustomAttribute: Focus, private bindingEngine: BindingEngine) {
  }

  bind() {
    if (this.focusCustomAttribute != null) {
      this.focusSubscription = this.bindingEngine.propertyObserver(this.focusCustomAttribute, 'value')
        .subscribe((newValue, oldValue) => {
          logger.debug('focus changes', newValue, oldValue);
          if (newValue) {
            logger.debug('Focusing on input');
            this.taskQueue.queueMicroTask(() => this.toggleElement.focus());
          }
        });
    }

    $(this.dropdownElement)
      .on('show.bs.dropdown', () => {
        return this.searchText != null && this.searchText.length > 0;
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
        })
        .then(() => {
          this.setNotFound(newValue);
        });
    } else {
      this.handleStatic(this.data);
      this.setNotFound(newValue);
    }

    if (!this.isOpen) {
      logger.debug('toggleElement', this.toggleElement);
      $(this.toggleElement).dropdown('toggle');
    }
  }

  setNotFound(searchText) {
    this.notFound = searchText != null && this.items.length === 0;
  }

  setValue(item) {
    this.settingValue = true;
    this.value = item.value;
    this.searchText = typeof item.value === 'string' ? item.value : item.value.name;

    this.taskQueue.queueMicroTask(() => {
      this.settingValue = false;
    })
  }

  focus() {
    logger.debug('focus');
    if (this.searchText != null) {
      this.toggleElement.setSelectionRange(0, this.searchText.length);
    }
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

  valueChanged(newValue) {
    if (newValue == null) {
      this.searchText = null;
    }
  }

  detached() {
    if (this.focusSubscription != null) {
      this.focusSubscription.dispose();
    }
  }
}
