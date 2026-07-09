import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { AppShellComponent } from './shared/components/layout/app-shell/app-shell.component';

export const routes: Routes = [
  {
    path: 'home',
    loadComponent: () => import('./features/landing/landing.component').then(m => m.LandingComponent)
  },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'forgot-password',
    loadComponent: () => import('./features/auth/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent)
  },
  {
    path: 'reset-password',
    loadComponent: () => import('./features/auth/reset-password/reset-password.component').then(m => m.ResetPasswordComponent)
  },

  {
    path: '',
    component: AppShellComponent,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'users',
        canActivate: [roleGuard],
        data: { roles: ['Admin'] },
        loadChildren: () => import('./features/users/users.routes').then(m => m.USERS_ROUTES)
      },
      {
        path: 'residents',
        canActivate: [roleGuard],
        data: { roles: ['Admin'] },
        loadChildren: () => import('./features/residents/residents.routes').then(m => m.RESIDENTS_ROUTES)
      },
      {
        path: 'apartments',
        canActivate: [roleGuard],
        data: { roles: ['Admin', 'Owner'] },
        loadChildren: () => import('./features/apartments/apartments.routes').then(m => m.APARTMENTS_ROUTES)
      },
      {
        path: 'complaints',
        canActivate: [roleGuard],
        data: { roles: ['Admin', 'Resident', 'Owner'] },
        loadChildren: () => import('./features/complaints/complaints.routes').then(m => m.COMPLAINTS_ROUTES)
      },
      {
        path: 'bills',
        canActivate: [roleGuard],
        data: { roles: ['Admin', 'Resident', 'Owner'] },
        loadChildren: () => import('./features/bills/bills.routes').then(m => m.BILLS_ROUTES)
      },
      {
        path: 'facilities',
        canActivate: [roleGuard],
        data: { roles: ['Admin', 'Resident', 'Owner'] },
        loadChildren: () => import('./features/facilities/facilities.routes').then(m => m.FACILITIES_ROUTES)
      },
      {
        path: 'bookings',
        canActivate: [roleGuard],
        data: { roles: ['Admin', 'Resident', 'Owner'] },
        loadChildren: () => import('./features/bookings/bookings.routes').then(m => m.BOOKINGS_ROUTES)
      },
      {
        path: 'visitors',
        canActivate: [roleGuard],
        data: { roles: ['Admin', 'Resident', 'Owner', 'SecurityStaff'] },
        loadChildren: () => import('./features/visitors/visitors.routes').then(m => m.VISITORS_ROUTES)
      },
      {
        path: 'announcements',
        loadChildren: () => import('./features/announcements/announcements.routes').then(m => m.ANNOUNCEMENTS_ROUTES)
      },
      {
        path: 'notifications',
        loadComponent: () => import('./features/notifications/notification-list/notification-list.component').then(m => m.NotificationListComponent)
      },
    ]
  },

  { path: '**', redirectTo: 'home' }
];
