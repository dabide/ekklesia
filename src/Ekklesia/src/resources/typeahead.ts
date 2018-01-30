import { element } from 'aurelia-protractor-plugin/protractor';
import { autoinject, LogManager } from 'aurelia-framework';
import $ from 'jquery';

const logger = LogManager.getLogger('typeahead');

@autoinject()
export class TypeaheadCustomAttribute {
  element: Element;

  constructor(element: Element) {
    this.element = element;
  }

  attached() {
    $(this.element).typeahead({
      hint: true,
      highlight: true,
      minLength: 1
    }, {
      name: 'states',
      source: this.substringMatcher(this.states)
    });
  }

  states = ['Alabama', 'Alaska', 'Arizona', 'Arkansas', 'California',
    'Colorado', 'Connecticut', 'Delaware', 'Florida', 'Georgia', 'Hawaii',
    'Idaho', 'Illinois', 'Indiana', 'Iowa', 'Kansas', 'Kentucky', 'Louisiana',
    'Maine', 'Maryland', 'Massachusetts', 'Michigan', 'Minnesota',
    'Mississippi', 'Missouri', 'Montana', 'Nebraska', 'Nevada', 'New Hampshire',
    'New Jersey', 'New Mexico', 'New York', 'North Carolina', 'North Dakota',
    'Ohio', 'Oklahoma', 'Oregon', 'Pennsylvania', 'Rhode Island',
    'South Carolina', 'South Dakota', 'Tennessee', 'Texas', 'Utah', 'Vermont',
    'Virginia', 'Washington', 'West Virginia', 'Wisconsin', 'Wyoming'
  ];

  substringMatcher(strs) {
    return (q, cb) => {
      var matches, substringRegex;

      // an array that will be populated with substring matches
      matches = [];

      // regex used to determine if a string contains the substring `q`
      let substrRegex = new RegExp(q, 'i');

      // iterate through the pool of strings and for any string that
      // contains the substring `q`, add it to the `matches` array
      $.each(strs, function (i, str) {
        if (substrRegex.test(str)) {
          matches.push(str);
        }
      });

      cb(matches);
    };
  }
}
