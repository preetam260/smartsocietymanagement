import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { VisitorService } from '../visitor.service';
import { VisitorResponse } from '../visitor.model';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { AuthService } from '../../../core/services/auth.service';
import { DatePipe, SlicePipe } from '@angular/common';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-my-visitors',
  standalone: true,
  imports: [RouterLink, StatusBadgeComponent, LoadingSpinnerComponent, EmptyStateComponent, DatePipe, SlicePipe],
  templateUrl: './my-visitors.component.html'
})
export class MyVisitorsComponent implements OnInit {
  private svc = inject(VisitorService);
  auth = inject(AuthService);
  loading = signal(true);
  allVisitors = signal<VisitorResponse[]>([]);
  searchTerm = signal('');
  currentPage = signal(1);

  filtered = computed(() => {
    const term = this.searchTerm().toLowerCase();
    if (!term) return this.allVisitors();
    return this.allVisitors().filter(v =>
      v.name.toLowerCase().includes(term) ||
      v.purpose.toLowerCase().includes(term) ||
      v.email.toLowerCase().includes(term)
    );
  });

  paginated = computed(() => {
    const page = this.currentPage();
    return this.filtered().slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);
  });

  totalPages = computed(() => Math.max(1, Math.ceil(this.filtered().length / PAGE_SIZE)));

  ngOnInit() {
    this.svc.getMyVisitors().subscribe({
      next: v => { this.allVisitors.set(v); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  onSearch(e: Event) {
    this.searchTerm.set((e.target as HTMLInputElement).value);
    this.currentPage.set(1);
  }

  prevPage() { if (this.currentPage() > 1) this.currentPage.update(p => p - 1); }
  nextPage() { if (this.currentPage() < this.totalPages()) this.currentPage.update(p => p + 1); }
}
