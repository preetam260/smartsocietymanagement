import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged, switchMap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { UserService } from '../user.service';
import { UserResponse } from '../user.model';
import { PagedResult } from '../../../core/models/paged-result.model';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [RouterLink, StatusBadgeComponent, PaginationComponent, ConfirmDialogComponent, LoadingSpinnerComponent, EmptyStateComponent],
  templateUrl: './user-list.component.html'
})
export class UserListComponent implements OnInit {
  private userService = inject(UserService);
  private toast = inject(ToastService);

  loading = signal(true);
  pagedResult = signal<PagedResult<UserResponse> | null>(null);
  showDeleteDialog = signal(false);
  userToDelete = signal<UserResponse | null>(null);
  currentPage = signal(1);

  searchTerm = new Subject<string>();

  constructor() {
    this.searchTerm.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(term => {
        this.loading.set(true);
        this.currentPage.set(1);
        return this.userService.getAllPaged({ pageNumber: 1, pageSize: 10, search: term });
      }),
      takeUntilDestroyed()
    ).subscribe(result => {
      this.pagedResult.set(result);
      this.loading.set(false);
    });
  }

  ngOnInit() {
    this.loadPage(1);
  }

  onSearch(event: Event) {
    this.searchTerm.next((event.target as HTMLInputElement).value);
  }

  goToPage(page: number) {
    this.currentPage.set(page);
    this.loadPage(page);
  }

  toggleActive(user: UserResponse) {
    const action$ = user.isActive
      ? this.userService.deactivate(user.id)
      : this.userService.activate(user.id);
    action$.subscribe(() => {
      this.toast.success(`User ${user.isActive ? 'deactivated' : 'activated'} successfully`);
      this.loadPage(this.currentPage());
    });
  }

  confirmDelete(user: UserResponse) {
    this.userToDelete.set(user);
    this.showDeleteDialog.set(true);
  }

  onDeleteConfirmed() {
    const user = this.userToDelete();
    if (!user) return;
    this.userService.delete(user.id).subscribe(() => {
      this.toast.success('User deleted successfully');
      this.showDeleteDialog.set(false);
      this.loadPage(this.currentPage());
    });
  }

  private loadPage(page: number) {
    this.loading.set(true);
    this.userService.getAllPaged({ pageNumber: page, pageSize: 10 }).subscribe(result => {
      this.pagedResult.set(result);
      this.loading.set(false);
    });
  }
}
