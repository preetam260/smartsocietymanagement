import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { UserRole } from '../models/enums';

export const roleGuard: CanActivateFn = (route) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const allowedRoles: UserRole[] = route.data?.['roles'] ?? [];
  const userRole = auth.role();

  if (userRole && allowedRoles.includes(userRole)) {
    return true;
  }
  return router.createUrlTree(['/dashboard']);
};
