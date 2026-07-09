import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ResidentService } from '../resident.service';
import { ResidentResponse } from '../resident.model';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { ToastService } from '../../../core/services/toast.service';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-resident-list',
  standalone: true,
  imports: [RouterLink, LoadingSpinnerComponent, EmptyStateComponent, DatePipe],
  templateUrl: './resident-list.component.html'
})
export class ResidentListComponent implements OnInit {
  private svc = inject(ResidentService);
  private toast = inject(ToastService);
  loading = signal(true);
  residents = signal<ResidentResponse[]>([]);
  showCurrent = signal(true);
  moveOutResident = signal<ResidentResponse | null>(null);
  today = new Date().toISOString().split('T')[0];

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    const obs = this.showCurrent() ? this.svc.getAllCurrent() : this.svc.getAll();
    obs.subscribe(r => { this.residents.set(r); this.loading.set(false); });
  }

  startMoveOut(r: ResidentResponse) { this.moveOutResident.set(r); }

  confirmMoveOut(dateStr: string) {
    const r = this.moveOutResident();
    if (!r || !dateStr) return;
    this.svc.moveOut(r.id, { moveOutDate: new Date(dateStr).toISOString() }).subscribe(() => {
      this.toast.success('Resident moved out');
      this.moveOutResident.set(null);
      this.load();
    });
  }
}
