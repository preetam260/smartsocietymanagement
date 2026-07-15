import { Component, inject, signal, computed, OnInit } from '@angular/core';
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

const PAGE_SIZE = 10;

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
  searchTerm = signal('');
  currentPage = signal(1);

  filtered = computed(() => {
    const term = this.searchTerm().toLowerCase();
    if (!term) return this.bills();
    return this.bills().filter(b =>
      b.period.toLowerCase().includes(term) ||
      b.status.toLowerCase().includes(term) ||
      `${b.apartmentBlock}-${b.apartmentNumber}`.toLowerCase().includes(term)
    );
  });

  paginated = computed(() => {
    const page = this.currentPage();
    return this.filtered().slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);
  });

  totalPages = computed(() => Math.max(1, Math.ceil(this.filtered().length / PAGE_SIZE)));

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    this.svc.getMyBills().subscribe(b => { this.bills.set(b); this.loading.set(false); });
  }

  onSearch(e: Event) {
    this.searchTerm.set((e.target as HTMLInputElement).value);
    this.currentPage.set(1);
  }

  prevPage() { if (this.currentPage() > 1) this.currentPage.update(p => p - 1); }
  nextPage() { if (this.currentPage() < this.totalPages()) this.currentPage.update(p => p + 1); }

  payBill(bill: BillResponse) {
    this.paying.set(true);
    this.svc.createPaymentOrder(bill.id).pipe(
      switchMap(order => this.svc.completePayment({
        orderId: order.orderId,
      }))
    ).subscribe({
      next: () => { this.toast.success('Payment successful!'); this.paying.set(false); this.load(); },
      error: () => this.paying.set(false)
    });
  }
}