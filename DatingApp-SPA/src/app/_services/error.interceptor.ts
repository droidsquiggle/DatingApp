import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpErrorResponse, HTTP_INTERCEPTORS } from '@angular/common/http';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

@Injectable()
export class ErrorInteceptor implements HttpInterceptor {
  intercept(
    req: import('@angular/common/http').HttpRequest<any>,
    next: import('@angular/common/http').HttpHandler
  ): import('rxjs').Observable<import('@angular/common/http').HttpEvent<any>> {
    return next.handle(req).pipe(
        catchError(error => {
            if (error.status === 401) {
                return throwError(error.statusText);
            }
            if (error instanceof HttpErrorResponse) {
                // this Application-Error must match what we called it in the c# code
                // this error text is stored in the header
                const applicationError = error.headers.get('Application-Error');
                if( applicationError) {
                    return throwError(applicationError);
                }
                // handle model state errors by finding the array
                // if array exists (in this case it would be an array of errors for login)
                // store it in a modal state errors object
                const serverError = error.error;
                let modalStateErrors = '';
                if(serverError.errors && typeof serverError.errors === 'object') {
                    for (const key in serverError.errors) {
                        if (serverError.errors[key]) {
                            modalStateErrors += serverError.errors[key] + '\n';
                        }
                    }
                }
                // if modalStateErrors is not an empty string throw that first
                // server state will throw "username already exists" kind of error
                // if none of those exists then we have a server error not captured
                return throwError(modalStateErrors || serverError || 'Server Error');
            }
        })
    );
  }
}

export const ErrorInteceptorProvider = {
    provide: HTTP_INTERCEPTORS,
    useClass: ErrorInteceptor,
    multi: true
};