import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ComplaintService } from '../complaint.service';
import { ComplaintResponse, PRIORITY_ORDER, ComplaintCategory, ComplaintPriority } from '../complaint.model';
import { ComplaintStatus } from '../../../core/models/enums';
import { AuthService } from '../../../core/services/auth.service';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { ToastService } from '../../../core/services/toast.service';
import { DatePipe, SlicePipe } from '@angular/common';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';

const PAGE_SIZE = 10;

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
  allComplaints = signal<ComplaintResponse[]>([]);
  activeComplaint = signal<ComplaintResponse | null>(null);

  searchTerm = signal('');
  selectedStatus = signal('');
  priorityFilter = signal<ComplaintPriority | ''>('');
  categoryFilter = signal<ComplaintCategory | ''>('');
  currentPage = signal(1);

  statusForm = new FormGroup({
    status: new FormControl<ComplaintStatus>('InProgress', [Validators.required]),
    adminResponse: new FormControl(''),
  });

  // single computed pipeline: search -> status -> priority -> category -> sort by priority
  filtered = computed(() => {
    let list = this.allComplaints();
    const term = this.searchTerm().toLowerCase();
    const status = this.selectedStatus();
    const priority = this.priorityFilter();
    const category = this.categoryFilter();

    if (term) list = list.filter(c =>
      c.title.toLowerCase().includes(term) ||
      c.userName.toLowerCase().includes(term)
    );
    if (status) list = list.filter(c => c.status === status);
    if (priority) list = list.filter(c => c.priority === priority);
    if (category) list = list.filter(c => c.category === category);

    return [...list].sort((a, b) => {
      const pa = a.priority ? PRIORITY_ORDER[a.priority] : 99;
      const pb = b.priority ? PRIORITY_ORDER[b.priority] : 99;
      return pa - pb;
    });
  });

  paginated = computed(() => {
    const page = this.currentPage();
    return this.filtered().slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);
  });

  totalPages = computed(() => Math.max(1, Math.ceil(this.filtered().length / PAGE_SIZE)));

  get requiresResponse(): boolean {
    const s = this.statusForm.value.status;
    return s === 'Resolved' || s === 'Rejected';
  }

  ngOnInit(): void {
    this.load();
  }

  hasDuplicates(c: ComplaintResponse): boolean {
    return !!c.possibleDuplicateIdsCsv && c.possibleDuplicateIdsCsv.length > 0;
  }

  onSearch(e: Event) {
    this.searchTerm.set((e.target as HTMLInputElement).value);
    this.currentPage.set(1);
  }

  onStatusFilter(e: Event) {
    this.selectedStatus.set((e.target as HTMLSelectElement).value);
    this.currentPage.set(1);
  }

  onPriorityFilter(e: Event) {
    this.priorityFilter.set((e.target as HTMLSelectElement).value as ComplaintPriority | '');
    this.currentPage.set(1);
  }

  onCategoryFilter(e: Event) {
    this.categoryFilter.set((e.target as HTMLSelectElement).value as ComplaintCategory | '');
    this.currentPage.set(1);
  }

  load() {
    this.loading.set(true);
    const obs = this.auth.role() === 'Admin' ? this.svc.getAll() : this.svc.getMyComplaints();
    obs.subscribe({
      next: c => { this.allComplaints.set(c); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  openStatusModal(c: ComplaintResponse) {
    this.activeComplaint.set(c);
    // Pre-fill with the AI draft if one exists and the likely action is resolving.
    const prefill = c.draftAdminResponse ?? '';
    this.statusForm.reset({ status: 'InProgress', adminResponse: prefill });
  }

  useAiDraft() {
    const c = this.activeComplaint();
    if (!c?.draftAdminResponse) return;
    this.statusForm.controls.adminResponse.setValue(c.draftAdminResponse);
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

  prevPage() { if (this.currentPage() > 1) this.currentPage.update(p => p - 1); }
  nextPage() { if (this.currentPage() < this.totalPages()) this.currentPage.update(p => p + 1); }
}