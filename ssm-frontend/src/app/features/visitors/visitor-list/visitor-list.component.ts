import { Component, inject, signal, computed, OnInit, OnDestroy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { VisitorService } from '../visitor.service';
import { VisitorResponse } from '../visitor.model';
import { AuthService } from '../../../core/services/auth.service';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { ToastService } from '../../../core/services/toast.service';
import { DatePipe } from '@angular/common';
import { VisitorStatus } from '../../../core/models/enums';
import { interval, Subscription } from 'rxjs';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-visitor-list',
  standalone: true,
  imports: [RouterLink, StatusBadgeComponent, LoadingSpinnerComponent, EmptyStateComponent, DatePipe],
  templateUrl: './visitor-list.component.html'
})
export class VisitorListComponent implements OnInit {
  private svc = inject(VisitorService);
  private toast = inject(ToastService);
  private pollSub?: Subscription;
  auth = inject(AuthService);
  loading = signal(true);
  allVisitors = signal<VisitorResponse[]>([]);
  searchTerm = signal('');
  selectedStatus = signal('');
  currentPage = signal(1);

  filtered = computed(() => {
    let list = this.allVisitors();
    const term = this.searchTerm().toLowerCase();
    const status = this.selectedStatus();
    if (term) list = list.filter(v =>
      v.name.toLowerCase().includes(term) ||
      v.email.toLowerCase().includes(term) ||
      v.purpose.toLowerCase().includes(term)
    );
    if (status) list = list.filter(v => v.status === status);
    return list;
  });

  paginated = computed(() => {
    const page = this.currentPage();
    return this.filtered().slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);
  });

  totalPages = computed(() => Math.max(1, Math.ceil(this.filtered().length / PAGE_SIZE)));

  ngOnInit() { 
    this.load(); 
    this.pollSub = interval(15000).subscribe(() => this.load());
  }

  ngOnDestroy() {
    this.pollSub?.unsubscribe();
  }

  onSearch(e: Event) {
    this.searchTerm.set((e.target as HTMLInputElement).value);
    this.currentPage.set(1);
  }

  onFilter(e: Event) {
    this.selectedStatus.set((e.target as HTMLSelectElement).value);
    this.currentPage.set(1);
  }

  load(silent = false) {
      if (!silent) this.loading.set(true);
      const role = this.auth.role();
      let obs$;
      if (role === 'Admin') obs$ = this.svc.getAll();
      else if (role === 'SecurityStaff') obs$ = this.svc.getAll();
      else obs$ = this.svc.getMyVisitors();

      obs$.subscribe({
        next: v => { this.allVisitors.set(v); this.loading.set(false); },
        error: () => { this.loading.set(false); }
      });
  }

  deny(v: VisitorResponse) {
    this.svc.deny(v.id).subscribe({
      next: () => { this.toast.success('Visitor denied'); this.load(); },
      error: () => {}
    });
  }

  checkout(v: VisitorResponse) {
    this.svc.checkOut(v.id).subscribe({
      next: () => { this.toast.success('Visitor checked out'); this.load(); },
      error: () => { this.load(); }
    });
  }

  prevPage() { if (this.currentPage() > 1) this.currentPage.update(p => p - 1); }
  nextPage() { if (this.currentPage() < this.totalPages()) this.currentPage.update(p => p + 1); }
}
