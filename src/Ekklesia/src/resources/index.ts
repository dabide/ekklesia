import {FrameworkConfiguration, PLATFORM} from 'aurelia-framework';

export function configure(config: FrameworkConfiguration) {
  config.globalResources([
    PLATFORM.moduleName('./autocomplete'),
    PLATFORM.moduleName('./media-element'),
    PLATFORM.moduleName('./playback-controls'),
  ]);
}
