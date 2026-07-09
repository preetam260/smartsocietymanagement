import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged, switchMap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BillService } from '../bill.service';
import { BillResponse } from '../bill.model';
import { PagedResult } from '../../../core/models/paged-result.model';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { ToastService } from '../../../core/services/toast.service';
import { DatePipe, CurrencyPipe } from '@angular/common';

@Component({
  selector: 'app-bill-list',
  standalone: true,
  imports: [RouterLink, StatusBadgeComponent, PaginationComponent, ConfirmDialogComponent, LoadingSpinnerComponent, EmptyStateComponent, DatePipe, CurrencyPipe],
  templateUrl: './bill-list.component.html'
})
export class BillListComponent implements OnInit {
  private svc = inject(BillService);
  private toast = inject(ToastService);
  loading = signal(true);
  pagedResult = signal<PagedResult<BillResponse> | null>(null);
  showDelete = signal(false);
  toDelete = signal<BillResponse | null>(null);
  confirmPenalties = signal(false);
  searchTerm = new Subject<string>();

  constructor() {
    this.searchTerm.pipe(debounceTime(300), distinctUntilChanged(), switchMap(t => { 
      this.loading.set(true); 
      return this.svc.getAllPaged({ pageNumber: 1, pageSize: 10, search: t }); }), 
        takeUntilDestroyed()).subscribe(r => { this.pagedResult.set(r); this.loading.set(false); });
  }

  ngOnInit() { 
    this.load(1); 
  }
  onSearch(e: Event) { 
    this.searchTerm.next((e.target as HTMLInputElement).value); 
  }

  load(page: number) {
    this.loading.set(true);
    this.svc.getAllPaged({ pageNumber: page, pageSize: 10 }).subscribe(r => { this.pagedResult.set(r); this.loading.set(false); });
  }

  onDelete() {
    const b = this.toDelete();
    if (!b) return;
    this.svc.delete(b.id).subscribe(() => { this.toast.success('Bill deleted'); this.showDelete.set(false); this.load(1); });
  }

  applyPenalties() {
    this.svc.applyPenalties().subscribe(() => { this.toast.success('Penalties applied to overdue bills'); this.confirmPenalties.set(false); this.load(1); });
  }
}
