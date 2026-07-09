import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ComplaintService } from '../complaint.service';
import { ComplaintResponse } from '../complaint.model';
import { ComplaintStatus } from '../../../core/models/enums';
import { AuthService } from '../../../core/services/auth.service';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { ToastService } from '../../../core/services/toast.service';
import { DatePipe, SlicePipe } from '@angular/common';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-complaint-list',
  standalone: true,
  imports: [RouterLink, StatusBadgeComponent, LoadingSpinnerComponent, EmptyStateComponent, DatePipe, SlicePipe, ReactiveFormsModule],
  templateUrl: './complaint-list.component.html'
})
export class ComplaintListComponent implements OnInit {
  private svc = inject(ComplaintService);
  private toast = inject(ToastService);
  auth = inject(AuthService);
  loading = signal(true);
  complaints = signal<ComplaintResponse[]>([]);
  activeComplaint = signal<ComplaintResponse | null>(null);

  statusForm = new FormGroup({
    status: new FormControl<ComplaintStatus>('InProgress', [Validators.required]),
    adminResponse: new FormControl(''),
  });

  get requiresResponse(): boolean {
    const s = this.statusForm.value.status;
    return s === 'Resolved' || s === 'Rejected';
  }

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    const obs = this.auth.role() === 'Admin' ? this.svc.getAll() : this.svc.getMyComplaints();
    obs.subscribe({
      next: c => { this.complaints.set(c); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  openStatusModal(c: ComplaintResponse) {
    this.activeComplaint.set(c);
    this.statusForm.reset({ status: 'InProgress', adminResponse: '' });
  }

  closeModal() { this.activeComplaint.set(null); }

  submitStatus() {
    const c = this.activeComplaint();
    if (!c) return;

    const status = this.statusForm.value.status!;
    const adminResponse = this.statusForm.value.adminResponse ?? undefined;

    if (this.requiresResponse && !adminResponse?.trim()) {
      this.toast.error('A response is required when resolving or rejecting.');
      return;
    }

    this.svc.updateStatus(c.id, { status, adminResponse }).subscribe({
      next: () => {
        this.toast.success(`Complaint marked as ${status}`);
        this.closeModal();
        this.load();
      }
    });
  }

  availableTransitions(c: ComplaintResponse): ComplaintStatus[] {
    const map: Record<ComplaintStatus, ComplaintStatus[]> = {
      Open: ['InProgress', 'Resolved', 'Rejected', 'Closed'],
      InProgress: ['Resolved', 'Rejected', 'Closed'],
      Resolved: [],
      Rejected: [],
      Closed: [],
    };
    return map[c.status] ?? [];
  }

  statusLabel(s: ComplaintStatus): string {
    const labels: Record<ComplaintStatus, string> = {
      Open: 'Open',
      InProgress: 'In Progress',
      Resolved: 'Resolved',
      Rejected: 'Rejected',
      Closed: 'Closed',
    };
    return labels[s] ?? s;
  }
}
