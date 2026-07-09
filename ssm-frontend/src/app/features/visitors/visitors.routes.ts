import { Routes } from '@angular/router';
import { roleGuard } from '../../core/guards/role.guard';

export const VISITORS_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./visitor-list/visitor-list.component').then(m => m.VisitorListComponent) },
  {
    path: 'register',
    canActivate: [roleGuard],
    data: { roles: ['Resident', 'Owner'] },
    loadComponent: () => import('./register-visitor/register-visitor.component').then(m => m.RegisterVisitorComponent)
  },
  { path: 'my', loadComponent: () => import('./my-visitors/my-visitors.component').then(m => m.MyVisitorsComponent) },
  { path: 'checkin', loadComponent: () => import('./visitor-checkin/visitor-checkin.component').then(m => m.VisitorCheckinComponent), canActivate: [roleGuard], data: { roles: ['SecurityStaff'] } },
  { path: ':id/entries', loadComponent: () => import('./visitor-entries/visitor-entries.component').then(m => m.VisitorEntriesComponent) },
];
