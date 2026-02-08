import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError, switchMap } from 'rxjs';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const authService = inject(AuthService);
  const isRefreshRequest = () => {
    const url = req.url ?? '';
    return url.includes('/auth/refresh');
  };

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401) {
        return throwError(() => error);
      }
      if (isRefreshRequest()) {
        authService.logout();
        router.navigate(['/auth/login']);
        return throwError(() => error);
      }
      return authService.refreshToken().pipe(
        switchMap((response) => {
          if (response?.token) {
            const cloned = req.clone({
              setHeaders: { Authorization: `Bearer ${response.token}` }
            });
            return next(cloned);
          }
          authService.logout();
          router.navigate(['/auth/login']);
          return throwError(() => error);
        })
      );
    })
  );
};
