import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, tap, catchError, shareReplay, map, finalize } from 'rxjs';
import { LoginRequest, LoginResponse, User, UserRole } from '../models/user.model';
import { API_ENDPOINTS } from '../constants/api-endpoints.const';
import { appConfig } from '../constants/app-config';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly TOKEN_KEY = 'auth_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';
  private readonly USER_KEY = 'user_data';

  currentUser = signal<User | null>(null);

  private readonly apiUrl = appConfig.apiUrl;
  private refreshInProgress: Observable<LoginResponse | null> | null = null;

  constructor(
    private http: HttpClient
  ) {
    this.loadUserFromStorage();
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    const url = `${this.apiUrl}${API_ENDPOINTS.AUTH.LOGIN}`;
    console.log('Login request to:', url);
    console.log('Login credentials:', { email: credentials.email, password: '***' });
    
    return this.http.post<LoginResponse>(url, credentials).pipe(
      tap({
        next: (response) => {
          console.log('Login successful:', response);
          this.setAuthData(response);
        },
        error: (error) => {
          console.error('Login error in service:', error);
        }
      })
    );
  }

  /** Try to refresh the access token. Returns new LoginResponse or null if failed. Shared so concurrent 401s trigger one refresh. */
  refreshToken(): Observable<LoginResponse | null> {
    const refresh = this.getRefreshToken();
    if (!refresh) {
      return of(null);
    }
    if (this.refreshInProgress) {
      return this.refreshInProgress;
    }
    const url = `${this.apiUrl}${API_ENDPOINTS.AUTH.REFRESH}`;
    this.refreshInProgress = this.http.post<LoginResponse>(url, { refreshToken: refresh }).pipe(
      tap((res) => this.setAuthData(res)),
      map((res) => res),
      catchError(() => of(null)),
      shareReplay(1),
      finalize(() => { this.refreshInProgress = null; })
    );
    return this.refreshInProgress;
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  logout(): void {
    const token = this.getToken();
    if (token) {
      const url = `${this.apiUrl}${API_ENDPOINTS.AUTH.LOGOUT}`;
      this.http.post(url, {}).subscribe({ error: () => {} });
    }
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.currentUser.set(null);
  }

  changePassword(currentPassword: string, newPassword: string): Observable<{ success: boolean; message: string }> {
    const url = `${this.apiUrl}${API_ENDPOINTS.AUTH.CHANGE_PASSWORD}`;
    return this.http.post<{ success: boolean; message: string }>(url, {
      currentPassword,
      newPassword
    });
  }

  /** Call after successful change-password so user is not sent back to change-password. */
  setMustChangePasswordFalse(): void {
    const user = this.currentUser();
    if (user) {
      const updated = { ...user, mustChangePassword: false };
      this.currentUser.set(updated);
      localStorage.setItem(this.USER_KEY, JSON.stringify(updated));
    }
  }

  isAuthenticated(): boolean {
    return !!this.getToken() && !this.isTokenExpired();
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  hasRole(roles: UserRole[]): boolean {
    const user = this.currentUser();
    if (!user) return false;
    
    const userRole = typeof user.role === 'string' 
      ? UserRole[user.role as keyof typeof UserRole] 
      : user.role;
    
    return roles.includes(userRole);
  }

  private setAuthData(response: LoginResponse): void {
    localStorage.setItem(this.TOKEN_KEY, response.token);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, response.refreshToken);
    localStorage.setItem(this.USER_KEY, JSON.stringify(response.user));
    this.currentUser.set(response.user);
  }

  private loadUserFromStorage(): void {
    const userData = localStorage.getItem(this.USER_KEY);
    if (userData) {
      this.currentUser.set(JSON.parse(userData));
    }
  }

  private isTokenExpired(): boolean {
    const token = this.getToken();
    if (!token) return true;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.exp * 1000 < Date.now();
    } catch {
      return true;
    }
  }
}
