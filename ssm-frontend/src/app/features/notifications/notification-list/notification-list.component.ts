import { Component, inject, signal, computed, OnInit, OnDestroy } from '@angular/core';
import { NotificationService } from '../notification.service';
import { NotificationStateService } from '../notification-state.service';
import { SignalRService } from '../../../core/services/signalr.service';
import { NotificationResponse } from '../notification.model';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { ToastService } from '../../../core/services/toast.service';
import { CommonModule, DatePipe } from '@angular/common';
import { PagedResult } from '../../../core/models/paged-result.model';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

const PAGE_SIZE = 5;

@Component({
  selector: 'app-notification-list',
  standalone: true,
  imports: [CommonModule, LoadingSpinnerComponent, EmptyStateComponent, DatePipe, PaginationComponent],
  templateUrl: './notification-list.component.html',
  styleUrls: ['./notification-list.component.css']
})
export class NotificationListComponent implements OnInit, OnDestroy {
  private service = inject(NotificationService);
  private notificationState = inject(NotificationStateService);
  private signalR = inject(SignalRService);
  private toast = inject(ToastService);

  loading = signal(true);
  firstLoadDone = signal(false);
  currentPage = signal(1);
  searchQuery = signal('');

  pagedResult = signal<PagedResult<NotificationResponse>>({
    items: [],
    pageNumber: 1,
    pageSize: PAGE_SIZE,
    totalCount: 0,
    totalPages: 0,
    hasPreviousPage: false,
    hasNextPage: false
  });

  items = computed(() => this.pagedResult().items ?? []);

  private searchInput$ = new Subject<string>();
  private hubTeardown?: () => void;

  constructor() {
    this.searchInput$.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntilDestroyed()
    ).subscribe(query => {
      this.searchQuery.set(query);
      this.loadPage(1);
    });
  }

  ngOnInit() {
    this.loadPage(1);

    this.hubTeardown = this.signalR.on<NotificationResponse>(
      'ReceiveNotification',
      (notification) => {
        if (this.currentPage() === 1) {
          this.pagedResult.update(result => {
            const newItems = [notification, ...result.items].slice(0, PAGE_SIZE);
            return {
              ...result,
              items: newItems,
              totalCount: result.totalCount + 1
            };
          });
        }
      }
    );
  }

  ngOnDestroy() {
    this.hubTeardown?.();
  }

  onSearch(event: Event) {
    this.searchInput$.next((event.target as HTMLInputElement).value);
  }

  private loadPage(page: number) {
    this.currentPage.set(page);
    this.loading.set(true);
    const search = this.searchQuery();
    this.service.getAll({
      pageNumber: page,
      pageSize: PAGE_SIZE,
      ...(search ? { search } : {})
    }).subscribe(data => {
      this.pagedResult.set(data);
      this.loading.set(false);
      this.firstLoadDone.set(true);
    });
  }

  markRead(n: NotificationResponse) {
    if (n.isRead) return;

    this.service.markAsRead(n.id).subscribe(() => {
      this.pagedResult.update(result => ({
        ...result,
        items: result.items.map(item =>
          item && item.id === n.id ? { ...item, isRead: true } : item
        )
      }));
      // Decrement the shared badge immediately — no refresh needed
      this.notificationState.decrement();
    });
  }

  markAllRead() {
    this.service.markAllAsRead().subscribe(() => {
      this.toast.success('All notifications marked as read');
      this.notificationState.reset();
      this.loadPage(this.currentPage());
    });
  }

  trackByNotification(index: number, notification: NotificationResponse | null) {
    return notification?.id ?? index;
  }

  gotoPage(page: number) {
    if (page < 1 || page > this.pagedResult().totalPages) return;
    this.loadPage(page);
  }
}