import { Routes } from '@angular/router';
import { roleGuard } from '../../core/guards/role.guard';

export const BOOKINGS_ROUTES: Routes = [
  { path: '', loadComponent: () => import('./booking-calendar/booking-calendar.component').then(m => m.BookingCalendarComponent) },
  { path: 'my', loadComponent: () => import('./my-bookings/my-bookings.component').then(m => m.MyBookingsComponent) },
  { path: 'book', loadComponent: () => import('./book-facility/book-facility.component').then(m => m.BookFacilityComponent) },
  { path: 'facility/:facilityId', loadComponent: () => import('./booking-list-by-facility/booking-list-by-facility.component').then(m => m.BookingListByFacilityComponent), canActivate: [roleGuard], data: { roles: ['Admin'] } },
];
