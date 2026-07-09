import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged, switchMap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ApartmentService } from '../apartment.service';
import { ApartmentResponse } from '../apartment.model';
import { PagedResult } from '../../../core/models/paged-result.model';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { ToastService } from '../../../core/services/toast.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-apartment-list',
  standalone: true,
  imports: [RouterLink, PaginationComponent, ConfirmDialogComponent, LoadingSpinnerComponent, EmptyStateComponent],
  templateUrl: './apartment-list.component.html'
})
export class ApartmentListComponent implements OnInit {
  private svc = inject(ApartmentService);
  private toast = inject(ToastService);
  auth = inject(AuthService);
  loading = signal(true);
  pagedResult = signal<PagedResult<ApartmentResponse> | null>(null);
  showDialog = signal(false);
  toDelete = signal<ApartmentResponse | null>(null);
  searchTerm = new Subject<string>();

  constructor() {
    this.searchTerm.pipe(debounceTime(300), distinctUntilChanged(), switchMap(term => { this.loading.set(true); return this.svc.getAllPaged({ pageNumber: 1, pageSize: 10, search: term }); }), takeUntilDestroyed()).subscribe(r => { this.pagedResult.set(r); this.loading.set(false); });
  }

  ngOnInit() { 
    this.load(1); 
  }
  onSearch(e: Event) { 
    this.searchTerm.next((e.target as HTMLInputElement).value); 
  }
  goToPage(p: number) { 
    this.load(p); 
  }
  confirmDelete(a: ApartmentResponse) { 
    this.toDelete.set(a); this.showDialog.set(true); 
  }
  onDeleteConfirmed() { 
    const a = this.toDelete(); 
    if (!a) return; this.svc.delete(a.id).subscribe(() => { 
      this.toast.success('Apartment deleted'); this.showDialog.set(false); this.load(1); 
    }); 
  }
  private load(page: number) { 
    this.loading.set(true); 
    this.svc.getAllPaged({ pageNumber: page, pageSize: 10 }).subscribe(r => 
      { this.pagedResult.set(r); this.loading.set(false); });
    }
}
