/// <reference types="aurelia-loader-webpack/src/webpack-hot-interface"/>
// we want font-awesome to load as soon as possible to show the fa-spinner
import {Aurelia} from 'aurelia-framework';
import environment from './environment';
import {PLATFORM} from 'aurelia-pal';
import * as Bluebird from 'bluebird';
import 'bootstrap';
import 'whatwg-fetch';

//--------------
// Font Awesome
//--------------
import fontawesome from '@fortawesome/fontawesome';
import faTrash from '@fortawesome/fontawesome-free-solid/faTrash';
fontawesome.library.add(faTrash);
//--------------

// remove out if you don't want a Promise polyfill (remove also from webpack.config.js)
Bluebird.config({ warnings: { wForgottenReturn: false } });

export function configure(aurelia: Aurelia) {
  aurelia.use
    .standardConfiguration()
    .feature(PLATFORM.moduleName('resources/index'))
    .plugin(PLATFORM.moduleName('aurelia-animator-css'))
    .plugin(PLATFORM.moduleName('aurelia-api'), config => {
      config.registerEndpoint('api', 'api/');
    })
    .plugin(PLATFORM.moduleName('aurelia-dragula'), options => {
      //options.revertOnSpill = false;
    })
    .globalResources([
    ]);

  // Uncomment the line below to enable animation.
  // aurelia.use.plugin(PLATFORM.moduleName('aurelia-animator-css'));
  // if the css animator is enabled, add swap-order="after" to all router-view elements

  // Anyone wanting to use HTMLImports to load views, will need to install the following plugin.
  // aurelia.use.plugin(PLATFORM.moduleName('aurelia-html-import-template-loader'));

  if (environment.debug) {
    aurelia.use.developmentLogging();
  }

  if (environment.testing) {
    aurelia.use.plugin(PLATFORM.moduleName('aurelia-testing'));
  }

  aurelia.start().then(() => aurelia.setRoot(PLATFORM.moduleName('app')));
}
