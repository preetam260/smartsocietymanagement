import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { VisitorService } from '../visitor.service';
import { VisitorEntryResponse } from '../visitor.model';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-visitor-entries',
  standalone: true,
  imports: [RouterLink, LoadingSpinnerComponent, EmptyStateComponent, DatePipe],
  templateUrl: './visitor-entries.component.html'
})
export class VisitorEntriesComponent implements OnInit {
  private svc = inject(VisitorService);
  private route = inject(ActivatedRoute);
  loading = signal(true);
  entries = signal<VisitorEntryResponse[]>([]);
  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.svc.getEntries(id).subscribe(e => { this.entries.set(e); this.loading.set(false); });
  }
}
