import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  /** Build full URL without double slashes (apiUrl may end with /, endpoint may start with /). */
  private url(endpoint: string): string {
    const base = this.apiUrl.replace(/\/$/, '');
    const path = endpoint.startsWith('/') ? endpoint.slice(1) : endpoint;
    return `${base}/${path}`;
  }

  get<T>(endpoint: string): Observable<T> {
    return this.http.get<T>(this.url(endpoint));
  }

  post<T>(endpoint: string, data: any): Observable<T> {
    return this.http.post<T>(this.url(endpoint), data);
  }

  put<T>(endpoint: string, data: any): Observable<T> {
    return this.http.put<T>(this.url(endpoint), data);
  }

  delete<T>(endpoint: string): Observable<T> {
    return this.http.delete<T>(this.url(endpoint));
  }
}
