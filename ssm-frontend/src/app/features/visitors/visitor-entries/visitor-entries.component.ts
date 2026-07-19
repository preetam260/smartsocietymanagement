import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { VisitorService } from '../visitor.service';
import { VisitorHistoryResponse } from '../visitor.model';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-visitor-entries',
  standalone: true,
  imports: [RouterLink, LoadingSpinnerComponent, EmptyStateComponent, StatusBadgeComponent, DatePipe],
  templateUrl: './visitor-entries.component.html'
})
export class VisitorEntriesComponent implements OnInit {
  private svc = inject(VisitorService);
  private route = inject(ActivatedRoute);
  loading = signal(true);
  history = signal<VisitorHistoryResponse | null>(null);

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.svc.getHistory(id).subscribe({
      next: h => { this.history.set(h); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }
}