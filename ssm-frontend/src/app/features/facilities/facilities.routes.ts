import { Routes } from '@angular/router';
import { roleGuard } from '../../core/guards/role.guard';

export const FACILITIES_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./facility-list/facility-list.component').then(m => m.FacilityListComponent) },
  { path: 'new', loadComponent: () => import('./facility-form/facility-form.component').then(m => m.FacilityFormComponent), canActivate: [roleGuard], data: { roles: ['Admin'] } },
  { path: ':id/edit', loadComponent: () => import('./facility-form/facility-form.component').then(m => m.FacilityFormComponent), canActivate: [roleGuard], data: { roles: ['Admin'] } },
];
