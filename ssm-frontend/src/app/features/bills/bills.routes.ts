import { Routes } from '@angular/router';
import { roleGuard } from '../../core/guards/role.guard';

export const BILLS_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./bill-list/bill-list.component').then(m => m.BillListComponent), canActivate: [roleGuard], data: { roles: ['Admin'] } },
  { path: 'new', loadComponent: () => import('./bill-form/bill-form.component').then(m => m.BillFormComponent), canActivate: [roleGuard], data: { roles: ['Admin'] } },
  { path: 'my', loadComponent: () => import('./my-bills/my-bills.component').then(m => m.MyBillsComponent) },
];