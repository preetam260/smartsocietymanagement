import { Routes } from '@angular/router';

export const APARTMENTS_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./apartment-list/apartment-list.component').then(m => m.ApartmentListComponent) },
  { path: 'new', loadComponent: () => import('./apartment-form/apartment-form.component').then(m => m.ApartmentFormComponent) },
  { path: 'my', loadComponent: () => import('./my-apartments/my-apartments.component').then(m => m.MyApartmentsComponent) },
];
