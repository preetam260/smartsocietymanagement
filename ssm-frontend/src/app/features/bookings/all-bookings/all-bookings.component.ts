import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BookingService } from '../booking.service';
import { BookingResponse } from '../booking.model';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { ToastService } from '../../../core/services/toast.service';
import { DatePipe, CurrencyPipe } from '@angular/common';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { PagedResult } from '../../../core/models/paged-result.model';

const PAGE_SIZE = 15;

@Component({
  selector: 'app-all-bookings',
  standalone: true,
  imports: [RouterLink, StatusBadgeComponent, LoadingSpinnerComponent, EmptyStateComponent, DatePipe, CurrencyPipe, ConfirmDialogComponent],
  templateUrl: './all-bookings.component.html'
})
export class AllBookingsComponent implements OnInit {
  private svc = inject(BookingService);
  private toast = inject(ToastService);
  loading = signal(true);
  allBookings = signal<BookingResponse[]>([]);
  searchTerm = signal('');
  selectedStatus = signal('');
  currentPage = signal(1);
  showExpireHolds = signal(false);

  filtered = computed(() => {
    let list = this.allBookings();
    const term = this.searchTerm().toLowerCase();
    const status = this.selectedStatus();
    if (term) list = list.filter(b =>
      b.userName.toLowerCase().includes(term) ||
      b.facilityName.toLowerCase().includes(term)
    );
    if (status) list = list.filter(b => b.status === status);
    return list;
  });

  paginated = computed(() => {
    const page = this.currentPage();
    return this.filtered().slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);
  });

  totalPages = computed(() => Math.max(1, Math.ceil(this.filtered().length / PAGE_SIZE)));

  ngOnInit() { this.load(); }

  onSearch(e: Event) {
    this.searchTerm.set((e.target as HTMLInputElement).value);
    this.currentPage.set(1);
  }

  onStatusFilter(e: Event) {
    this.selectedStatus.set((e.target as HTMLSelectElement).value);
    this.currentPage.set(1);
  }

  load() {
    this.loading.set(true);
    this.svc.getAll({ pageNumber: 1, pageSize: 999 }).subscribe({
      next: r => { this.allBookings.set((r as PagedResult<BookingResponse>).items); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  markComplete(b: BookingResponse) {
    this.svc.complete(b.id).subscribe({
      next: () => { this.toast.success('Booking marked complete'); this.load(); },
      error: () => {}
    });
  }

  expireHolds() {
    this.svc.expireHolds().subscribe({
      next: () => { this.toast.success('Expired holds processed'); this.showExpireHolds.set(false); this.load(); },
      error: () => {}
    });
  }

  prevPage() { if (this.currentPage() > 1) this.currentPage.update(p => p - 1); }
  nextPage() { if (this.currentPage() < this.totalPages()) this.currentPage.update(p => p + 1); }
}
