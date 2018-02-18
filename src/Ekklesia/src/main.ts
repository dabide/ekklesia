import 'jquery';
// <reference types="aurelia-loader-webpack/src/webpack-hot-interface"/>
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

import * as faFile from '@fortawesome/fontawesome-free-solid/faFile';
import * as faFileAudio from '@fortawesome/fontawesome-free-solid/faFileAudio';
import * as faFileImage from '@fortawesome/fontawesome-free-solid/faFileImage';
import * as faFileVideo from '@fortawesome/fontawesome-free-solid/faFileVideo';
import * as faGlobe from '@fortawesome/fontawesome-free-solid/faGlobe';
import * as faMusic from '@fortawesome/fontawesome-free-solid/faMusic';
import * as faPlay from '@fortawesome/fontawesome-free-solid/faPlay';
import * as faPause from '@fortawesome/fontawesome-free-solid/faPause';
import * as faStop from '@fortawesome/fontawesome-free-solid/faStop';
import * as faTrash from '@fortawesome/fontawesome-free-solid/faTrash';
import * as faFacebook from '@fortawesome/fontawesome-free-brands/faFacebook';
import * as faYoutube from '@fortawesome/fontawesome-free-brands/faYoutube';

fontawesome.library.add(faFile);
fontawesome.library.add(faFileAudio);
fontawesome.library.add(faFileImage);
fontawesome.library.add(faFileVideo);
fontawesome.library.add(faGlobe);
fontawesome.library.add(faMusic);
fontawesome.library.add(faPlay);
fontawesome.library.add(faPause);
fontawesome.library.add(faStop);
fontawesome.library.add(faTrash);
fontawesome.library.add(faFacebook);
fontawesome.library.add(faYoutube);
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
      config.registerEndpoint('noembed', 'https://noembed.com/');
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
