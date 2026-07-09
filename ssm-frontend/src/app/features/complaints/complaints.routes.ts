import { Routes } from '@angular/router';

export const COMPLAINTS_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./complaint-list/complaint-list.component').then(m => m.ComplaintListComponent) },
  { path: 'new', loadComponent: () => import('./complaint-form/complaint-form.component').then(m => m.ComplaintFormComponent) },
  { path: 'my', loadComponent: () => import('./complaint-list/complaint-list.component').then(m => m.ComplaintListComponent) },
];
