import { Routes } from '@angular/router';
import { roleGuard } from '../../core/guards/role.guard';

export const ANNOUNCEMENTS_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./announcement-feed/announcement-feed.component').then(m => m.AnnouncementFeedComponent) },
  { path: 'manage', loadComponent: () => import('./announcement-list/announcement-list.component').then(m => m.AnnouncementListComponent), canActivate: [roleGuard], data: { roles: ['Admin'] } },
  { path: 'new', loadComponent: () => import('./announcement-form/announcement-form.component').then(m => m.AnnouncementFormComponent), canActivate: [roleGuard], data: { roles: ['Admin'] } },
  { path: ':id/edit', loadComponent: () => import('./announcement-form/announcement-form.component').then(m => m.AnnouncementFormComponent), canActivate: [roleGuard], data: { roles: ['Admin'] } },
];
