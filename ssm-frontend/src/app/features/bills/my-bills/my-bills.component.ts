import { Component, inject, signal, OnInit } from '@angular/core';
import { BillService } from '../bill.service';
import { BillResponse } from '../bill.model';
import { AuthService } from '../../../core/services/auth.service';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { ToastService } from '../../../core/services/toast.service';
import { DatePipe, CurrencyPipe } from '@angular/common';
import { generateDummyPaymentId, DUMMY_SIGNATURE } from '../../../core/utils/payment-utils';
import { switchMap } from 'rxjs';

@Component({
  selector: 'app-my-bills',
  standalone: true,
  imports: [StatusBadgeComponent, LoadingSpinnerComponent, EmptyStateComponent, DatePipe, CurrencyPipe],
  templateUrl: './my-bills.component.html'
})
export class MyBillsComponent implements OnInit {
  private svc = inject(BillService);
  private toast = inject(ToastService);
  auth = inject(AuthService);
  loading = signal(true);
  bills = signal<BillResponse[]>([]);
  paying = signal(false);

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    this.svc.getMyBills().subscribe(b => { this.bills.set(b); this.loading.set(false); });
  }

  payBill(bill: BillResponse) {
    this.paying.set(true);
    this.svc.createPaymentOrder(bill.id).pipe(
      switchMap(order => this.svc.verifyPayment({
        orderId: order.orderId,
        paymentId: generateDummyPaymentId(),
        signature: DUMMY_SIGNATURE,
      }))
    ).subscribe({
      next: () => { this.toast.success('Payment successful!'); this.paying.set(false); this.load(); },
      error: () => this.paying.set(false)
    });
  }
}
