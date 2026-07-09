import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { NotificationResponse } from './notification.model';
import { PagedResult, PaginationQuery } from '../../core/models/paged-result.model';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/notification`;

  getAll(query?: PaginationQuery) {
    const pageNumber = query?.pageNumber ?? 1;
    const pageSize = query?.pageSize ?? 10;

    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (query?.search) {
      params = params.set('search', query.search);
    }

    return this.http.get<PagedResult<NotificationResponse>>(this.base, { params });
  }

  getUnread() {
    return this.http.get<NotificationResponse[]>(`${this.base}/unread`);
  }

  markAsRead(id: string) {
    return this.http.patch<void>(`${this.base}/${id}/read`, {});
  }

  markAllAsRead() {
    return this.http.patch<void>(`${this.base}/read-all`, {});
  }

  getByUser(userId: string) {
    return this.http.get<NotificationResponse[]>(`${this.base}/user/${userId}`);
  }
}
