import { Component, inject, signal, OnInit } from '@angular/core';
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

@Component({
  selector: 'app-visitor-list',
  standalone: true,
  imports: [RouterLink, StatusBadgeComponent, LoadingSpinnerComponent, EmptyStateComponent, DatePipe],
  templateUrl: './visitor-list.component.html'
})
export class VisitorListComponent implements OnInit {
  private svc = inject(VisitorService);
  private toast = inject(ToastService);
  auth = inject(AuthService);
  loading = signal(true);
  visitors = signal<VisitorResponse[]>([]);

  ngOnInit() { this.load(); }

  onFilter(e: Event) {
    const status = (e.target as HTMLSelectElement).value;
    this.loading.set(true);
    if (status) {
      this.svc.getByStatus(status as VisitorStatus).subscribe({
        next: v => { this.visitors.set(v); this.loading.set(false); },
        error: () => this.loading.set(false)
      });
    } else {
      this.load();
    }
  }

  load() {
    this.loading.set(true);
    this.svc.getMyVisitors().subscribe({
      next: v => { this.visitors.set(v); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  deny(v: VisitorResponse) {
    this.svc.deny(v.id).subscribe(() => { this.toast.success('Visitor denied'); this.load(); });
  }

  checkout(v: VisitorResponse) {
    this.svc.checkOut(v.id).subscribe(() => { this.toast.success('Visitor checked out'); this.load(); });
  }
}
