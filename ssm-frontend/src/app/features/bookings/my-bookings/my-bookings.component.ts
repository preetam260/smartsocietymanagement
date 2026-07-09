import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BookingService } from '../booking.service';
import { BookingResponse } from '../booking.model';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { ToastService } from '../../../core/services/toast.service';
import { DatePipe, CurrencyPipe } from '@angular/common';
import { generateDummyPaymentId, DUMMY_SIGNATURE } from '../../../core/utils/payment-utils';
import { switchMap } from 'rxjs';

@Component({
  selector: 'app-my-bookings',
  standalone: true,
  imports: [RouterLink, StatusBadgeComponent, LoadingSpinnerComponent, EmptyStateComponent, DatePipe, CurrencyPipe],
  templateUrl: './my-bookings.component.html'
})
export class MyBookingsComponent implements OnInit {
  private svc = inject(BookingService);
  private toast = inject(ToastService);
  loading = signal(true);
  bookings = signal<BookingResponse[]>([]);
  paying = signal(false);

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    this.svc.getMyBookings().subscribe(b => { this.bookings.set(b); this.loading.set(false); });
  }

  payBooking(booking: BookingResponse) {
    this.paying.set(true);
    this.svc.createPaymentOrder(booking.id).pipe(
      switchMap(order => this.svc.verifyPayment(booking.id, {
        orderId: order.orderId,
        paymentId: generateDummyPaymentId(),
        signature: DUMMY_SIGNATURE,
      }))
    ).subscribe({
      next: () => { this.toast.success('Payment successful!'); this.paying.set(false); this.load(); },
      error: () => this.paying.set(false)
    });
  }

  cancelBooking(booking: BookingResponse) {
    this.svc.cancel(booking.id).subscribe(() => { this.toast.success('Booking cancelled'); this.load(); });
  }
}
