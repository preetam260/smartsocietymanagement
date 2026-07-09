import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { VisitorService } from '../visitor.service';
import { VisitorResponse } from '../visitor.model';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { DatePipe, SlicePipe } from '@angular/common';

@Component({
  selector: 'app-my-visitors',
  standalone: true,
  imports: [RouterLink, StatusBadgeComponent, LoadingSpinnerComponent, EmptyStateComponent, DatePipe, SlicePipe],
  templateUrl: './my-visitors.component.html'
})
export class MyVisitorsComponent implements OnInit {
  private svc = inject(VisitorService);
  loading = signal(true);
  visitors = signal<VisitorResponse[]>([]);
  ngOnInit() { this.svc.getMyVisitors().subscribe(v => { this.visitors.set(v); this.loading.set(false); }); }
}
