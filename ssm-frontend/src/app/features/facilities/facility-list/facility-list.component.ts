import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FacilityService } from '../facility.service';
import { FacilityResponse } from '../facility.model';
import { AuthService } from '../../../core/services/auth.service';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { ToastService } from '../../../core/services/toast.service';
import { CurrencyPipe, SlicePipe } from '@angular/common';

@Component({
  selector: 'app-facility-list',
  standalone: true,
  imports: [RouterLink, ConfirmDialogComponent, LoadingSpinnerComponent, EmptyStateComponent, CurrencyPipe, SlicePipe],
  templateUrl: './facility-list.component.html',
  styleUrls: ['./facility-list.component.css']
})
export class FacilityListComponent implements OnInit {
  private svc = inject(FacilityService);
  private toast = inject(ToastService);
  auth = inject(AuthService);
  loading = signal(true);
  facilities = signal<FacilityResponse[]>([]);
  showDelete = signal(false);
  toDelete = signal<FacilityResponse | null>(null);

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    const obs = this.auth.role() === 'Admin' ? this.svc.getAll() : this.svc.getActive();
    obs.subscribe(f => { this.facilities.set(f); this.loading.set(false); });
  }

  onDelete() {
    const f = this.toDelete();
    if (!f) return;
    this.svc.delete(f.id).subscribe(() => { this.toast.success('Facility deleted'); this.showDelete.set(false); this.load(); });
  }
}
