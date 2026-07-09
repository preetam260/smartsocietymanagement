import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { LoginRequest, LoginResponse, ForgotPasswordRequest, ResetPasswordRequest } from '../models/auth.model';
import { UserRole } from '../models/enums';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly STORAGE_KEY = 'ss_session';
  private http = inject(HttpClient);
  private router = inject(Router);

  currentUser = signal<LoginResponse | null>(this.restore());
  isLoggedIn = computed(() => !!this.currentUser());
  role = computed<UserRole | null>(() => this.currentUser()?.role ?? null);

  private loggingOut = false;

  login(dto: LoginRequest) {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/login`, dto).pipe(
      tap(res => {
        this.persist(res);
        this.currentUser.set(res);
      })
    );
  }

  forgotPassword(dto: ForgotPasswordRequest) {
    return this.http.post<{ message: string }>(`${environment.apiUrl}/auth/forgot-password`, dto);
  }

  resetPassword(dto: ResetPasswordRequest) {
    return this.http.post<{ message: string }>(`${environment.apiUrl}/auth/reset-password`, dto);
  }

  logout() {
    if (this.loggingOut) return;
    this.loggingOut = true;

    localStorage.removeItem(this.STORAGE_KEY);
    this.currentUser.set(null);
    this.router.navigate(['/login']).then(() => {
      this.loggingOut = false;
    });
  }

  private restore(): LoginResponse | null {
    try {
      const raw = localStorage.getItem(this.STORAGE_KEY);
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  }

  private persist(u: LoginResponse) {
    localStorage.setItem(this.STORAGE_KEY, JSON.stringify(u));
  }
}
