import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { ToastService } from '../services/toast.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const toast = inject(ToastService);

  if (req.url.includes('/auth/login')) {
    return next(req);
  }

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401) {
        auth.logout();
        toast.error('Your session has expired. Please log in again.');
      } else if (err.status === 403) {
        toast.error("You don't have permission to perform this action.");
      } else if (err.status === 0) {
        toast.error('Unable to reach the server. Please check your connection.');
      } else {
        toast.error(err.error?.message ?? 'Something went wrong. Please try again.');
      }
      return throwError(() => err);
    })
  );
};

