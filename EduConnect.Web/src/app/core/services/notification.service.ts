import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../services/api.service';
import { API_ENDPOINTS } from '../constants/api-endpoints.const';
import { appConfig } from '../constants/app-config';
import { Notification } from '../models/notification.model';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private readonly apiUrl = appConfig.apiUrl;

  constructor(private api: ApiService) {}

  getMyNotifications(unreadOnly = false): Observable<Notification[]> {
    const url = unreadOnly ? `${API_ENDPOINTS.NOTIFICATIONS.LIST}?unreadOnly=true` : API_ENDPOINTS.NOTIFICATIONS.LIST;
    return this.api.get<Notification[]>(url);
  }

  markAsRead(id: number): Observable<{ success: boolean }> {
    return this.api.patch<{ success: boolean }>(API_ENDPOINTS.NOTIFICATIONS.MARK_READ(id), {});
  }

  markAllAsRead(): Observable<{ success: boolean; markedCount: number }> {
    return this.api.post<{ success: boolean; markedCount: number }>(API_ENDPOINTS.NOTIFICATIONS.MARK_ALL_READ, {});
  }
}
