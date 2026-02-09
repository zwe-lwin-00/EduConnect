import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../../core/services/notification.service';
import { Notification } from '../../../core/models/notification.model';
import { DisplayDatePipe } from '../../pipes/display-date.pipe';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [CommonModule, DisplayDatePipe],
  templateUrl: './notification-bell.component.html',
  styleUrl: './notification-bell.component.css'
})
export class NotificationBellComponent implements OnInit {
  notifications: Notification[] = [];
  unreadCount = 0;
  open = false;
  loading = false;
  markingAll = false;

  constructor(private notificationService: NotificationService) {}

  ngOnInit(): void {
    this.notificationService.getMyNotifications(true).subscribe({
      next: (list) => { this.unreadCount = list.length; }
    });
  }

  toggle(): void {
    this.open = !this.open;
    if (this.open && this.notifications.length === 0) {
      this.load();
    }
  }

  load(): void {
    this.loading = true;
    this.notificationService.getMyNotifications(false).subscribe({
      next: (list) => {
        this.notifications = list;
        this.unreadCount = list.filter(n => !n.isRead).length;
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  refreshUnreadCount(): void {
    this.notificationService.getMyNotifications(true).subscribe({
      next: (list) => { this.unreadCount = list.length; }
    });
  }

  markAsRead(n: Notification): void {
    if (n.isRead || n.id < 0) return;
    this.notificationService.markAsRead(n.id).subscribe({
      next: () => {
        n.isRead = true;
        this.unreadCount = Math.max(0, this.unreadCount - 1);
        this.refreshUnreadCount();
      }
    });
  }

  markAllAsRead(): void {
    if (this.markingAll || this.unreadCount === 0) return;
    this.markingAll = true;
    this.notificationService.markAllAsRead().subscribe({
      next: () => {
        this.notifications.forEach(n => { n.isRead = true; });
        this.unreadCount = 0;
        this.markingAll = false;
        this.refreshUnreadCount();
      },
      error: () => { this.markingAll = false; }
    });
  }

  close(): void {
    this.open = false;
  }
}
