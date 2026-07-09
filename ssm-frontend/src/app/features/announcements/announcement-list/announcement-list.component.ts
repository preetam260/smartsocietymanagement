import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged, switchMap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AnnouncementService } from '../announcement.service';
import { AnnouncementResponse } from '../announcement.model';
import { PagedResult } from '../../../core/models/paged-result.model';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { ToastService } from '../../../core/services/toast.service';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-announcement-list',
  standalone: true,
  imports: [RouterLink, PaginationComponent, ConfirmDialogComponent, LoadingSpinnerComponent, EmptyStateComponent, DatePipe],
  templateUrl: './announcement-list.component.html'
})
export class AnnouncementListComponent implements OnInit {
  private svc = inject(AnnouncementService);
  private toast = inject(ToastService);
  loading = signal(true);
  firstLoadDone = signal(false);
  pagedResult = signal<PagedResult<AnnouncementResponse> | null>(null);
  showDelete = signal(false);
  toDelete = signal<AnnouncementResponse | null>(null);
  searchTerm = new Subject<string>();

  constructor() {
    this.searchTerm.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(t => {
        this.loading.set(true);
        return this.svc.getAllPaged({ pageNumber: 1, pageSize: 5, search: t });
      }),
      takeUntilDestroyed()
    ).subscribe(r => {
      this.pagedResult.set(r);
      this.loading.set(false);
      this.firstLoadDone.set(true);
    });
  }

  ngOnInit() { this.load(1); }

  onSearch(e: Event) { 
    this.searchTerm.next((e.target as HTMLInputElement).value); 
  }

  load(page: number) {
    this.loading.set(true);
    this.svc.getAllPaged({ pageNumber: page, pageSize: 10 }).subscribe(r => {
      this.pagedResult.set(r);
      this.loading.set(false);
      this.firstLoadDone.set(true);
    });
  }

  onDelete() {
    const a = this.toDelete();
    if (!a) return;
    this.svc.delete(a.id).subscribe(() => {
      this.toast.success('Deleted');
      this.showDelete.set(false);
      this.load(1);
    });
  }
}