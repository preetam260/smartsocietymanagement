import { Component, inject, output, OnInit, OnDestroy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { NotificationService } from '../../../../features/notifications/notification.service';
import { NotificationStateService } from '../../../../features/notifications/notification-state.service';
import { SignalRService } from '../../../../core/services/signalr.service';
import { NotificationResponse } from '../../../../features/notifications/notification.model';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css']
})
export class NavbarComponent implements OnInit, OnDestroy {
  auth = inject(AuthService);
  private notificationService = inject(NotificationService);
  private notificationState = inject(NotificationStateService);
  private signalR = inject(SignalRService);

  unreadCount = this.notificationState.unreadCount;

  sidebarToggle = output<void>();

  private hubTeardown?: () => void;

  ngOnInit() {
    this.notificationService.getUnread().subscribe(
      list => this.notificationState.setCount(list.length)
    );

    this.signalR.startConnection();

    this.hubTeardown = this.signalR.on<NotificationResponse>(
      'ReceiveNotification',
      () => this.notificationState.increment()
    );
  }

  ngOnDestroy() {
    this.hubTeardown?.();
    this.signalR.stopConnection();
  }

  toggleSidebar() {
    this.sidebarToggle.emit();
  }
}
