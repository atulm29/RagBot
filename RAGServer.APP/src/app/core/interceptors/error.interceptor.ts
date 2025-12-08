import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const authService = inject(AuthService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        authService.logout();
        router.navigate(['/auth/login']);
      }

      const errorMessage = error.error?.error || error.message || 'An error occurred';
      
      console.error('HTTP Error:', {
        status: error.status,
        message: errorMessage,
        url: error.url
      });

      return throwError(() => new Error(errorMessage));
    })
  );
};
