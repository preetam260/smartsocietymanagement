import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ResidentService } from '../resident.service';
import { ResidentResponse } from '../resident.model';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { ToastService } from '../../../core/services/toast.service';
import { DatePipe } from '@angular/common';

const PAGE_SIZE = 10;

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
  allResidents = signal<ResidentResponse[]>([]);
  showCurrent = signal(true);
  moveOutResident = signal<ResidentResponse | null>(null);
  today = new Date().toISOString().split('T')[0];
  searchTerm = signal('');
  currentPage = signal(1);

  filtered = computed(() => {
    const term = this.searchTerm().toLowerCase();
    if (!term) return this.allResidents();
    return this.allResidents().filter(r =>
      r.userName.toLowerCase().includes(term) ||
      r.userEmail.toLowerCase().includes(term) ||
      `${r.apartmentBlock}-${r.apartmentNumber}`.toLowerCase().includes(term)
    );
  });

  paginated = computed(() => {
    const page = this.currentPage();
    return this.filtered().slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);
  });

  totalPages = computed(() => Math.max(1, Math.ceil(this.filtered().length / PAGE_SIZE)));

  ngOnInit() { this.load(); }

  onSearch(e: Event) {
    this.searchTerm.set((e.target as HTMLInputElement).value);
    this.currentPage.set(1);
  }

  load() {
    this.loading.set(true);
    const obs = this.showCurrent() ? this.svc.getAllCurrent() : this.svc.getAll();
    obs.subscribe(r => { this.allResidents.set(r); this.loading.set(false); });
  }

  toggleView(current: boolean) {
    this.showCurrent.set(current);
    this.searchTerm.set('');
    this.currentPage.set(1);
    this.load();
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

  prevPage() { 
    if (this.currentPage() > 1) 
      this.currentPage.update(p => p - 1); 
  }
  nextPage() { 
    if (this.currentPage() < this.totalPages()) 
      this.currentPage.update(p => p + 1); 
  }
}
