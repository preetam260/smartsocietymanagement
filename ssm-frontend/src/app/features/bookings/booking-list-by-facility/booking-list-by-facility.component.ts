import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { BookingService } from '../booking.service';
import { BookingResponse } from '../booking.model';
import { PagedResult } from '../../../core/models/paged-result.model';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { ToastService } from '../../../core/services/toast.service';
import { DatePipe, CurrencyPipe } from '@angular/common';

@Component({
  selector: 'app-booking-list-by-facility',
  standalone: true,
  imports: [RouterLink, StatusBadgeComponent, PaginationComponent, ConfirmDialogComponent, LoadingSpinnerComponent, EmptyStateComponent, DatePipe, CurrencyPipe],
  templateUrl: './booking-list-by-facility.component.html'
})
export class BookingListByFacilityComponent implements OnInit {
  private svc = inject(BookingService);
  private route = inject(ActivatedRoute);
  private toast = inject(ToastService);
  loading = signal(true);
  pagedResult = signal<PagedResult<BookingResponse> | null>(null);
  showExpireHolds = signal(false);
  facilityId = '';

  ngOnInit() {
    this.facilityId = this.route.snapshot.paramMap.get('facilityId')!;
    this.load(1);
  }

  load(page: number) {
    this.loading.set(true);
    this.svc.getByFacility(this.facilityId, { pageNumber: page, pageSize: 10 }).subscribe(r => { this.pagedResult.set(r); this.loading.set(false); });
  }

  markComplete(booking: BookingResponse) {
    this.svc.complete(booking.id).subscribe(() => { this.toast.success('Booking marked complete'); this.load(1); });
  }

  expireHolds() {
    this.svc.expireHolds().subscribe(() => { this.toast.success('Expired holds processed'); this.showExpireHolds.set(false); this.load(1); });
  }
}
