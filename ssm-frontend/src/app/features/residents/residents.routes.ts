import { Routes } from '@angular/router';

export const RESIDENTS_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./resident-list/resident-list.component').then(m => m.ResidentListComponent) },
  { path: 'new', loadComponent: () => import('./resident-form/resident-form.component').then(m => m.ResidentFormComponent) },
  { path: ':id/edit', loadComponent: () => import('./resident-form/resident-form.component').then(m => m.ResidentFormComponent) },
];
