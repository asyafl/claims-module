import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(private snackBar: MatSnackBar) {}

  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(req).pipe(
      catchError((err: HttpErrorResponse) => {
        let message = 'An unexpected error occurred.';
        if (err.error?.title) message = err.error.title;
        else if (err.error?.errors) {
          const first = Object.values(err.error.errors as Record<string, string[]>).flat()[0];
          if (first) message = first;
        } else if (err.status === 0) message = 'Cannot connect to server.';
        else if (err.status === 401) message = 'Session expired. Please log in again.';
        else if (err.status === 403) message = 'You do not have permission for this action.';
        else if (err.status === 404) message = 'Resource not found.';

        this.snackBar.open(message, 'Dismiss', {
          duration: 5000,
          panelClass: ['snack-error'],
          horizontalPosition: 'right',
          verticalPosition: 'top'
        });
        return throwError(() => err);
      })
    );
  }
}
