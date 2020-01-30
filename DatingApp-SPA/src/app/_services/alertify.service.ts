import { Injectable } from '@angular/core';
import * as alertify from 'alertifyjs';

@Injectable({
  providedIn: 'root'
})
export class AlertifyService {

constructor() { }

  // we ant to add a confirm message, we take in the string of teh confirm and a callback of type any function
  // pass in any function into the confirm, if a function exists call back the function (this will be defined in components)
  confirm(message: string, okCallback: () => any) {
    alertify.confirm(message, (e: any) => {
      if (e) {
        okCallback();
      } else {}
    });
  }

  // THIS IS JAVASCRIPT, NO TYPESCRIPT HELP HERE, ERRORS BY MISSPELLING WILL NOT BE CAUGHT 
  // this is due to how we import this file in the typings.d.ts
  success(message: string) {
    alertify.success(message);
  }

  error(message: string) {
    alertify.error(message);
  }

  warning(message: string) {
    alertify.warning(message);
  }

  message(message: string) {
    alertify.message(message);
  }


}
