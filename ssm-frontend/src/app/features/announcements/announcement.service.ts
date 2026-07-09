import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { AnnouncementResponse, AnnouncementDto } from './announcement.model';
import { PagedResult, PaginationQuery } from '../../core/models/paged-result.model';
import { UserRole } from '../../core/models/enums';

@Injectable({ providedIn: 'root' })
export class AnnouncementService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/announcement`;

  getAllPaged(query: PaginationQuery) {
    let params = new HttpParams()
      .set('pageNumber', query.pageNumber)
      .set('pageSize', query.pageSize);
    if (query.search) params = params.set('search', query.search);
    return this.http.get<PagedResult<AnnouncementResponse>>(this.base, { params });
  }

  getById(id: string) {
    return this.http.get<AnnouncementResponse>(`${this.base}/${id}`);
  }

  getByAudience(role: UserRole) {
    return this.http.get<AnnouncementResponse[]>(`${this.base}/audience/${role}`);
  }

  getPinned() {
    return this.http.get<AnnouncementResponse[]>(`${this.base}/pinned`);
  }

  getMine() {
    return this.http.get<AnnouncementResponse[]>(`${this.base}/mine`);
  }

  create(dto: AnnouncementDto) {
    return this.http.post<AnnouncementResponse>(this.base, dto);
  }

  update(id: string, dto: AnnouncementDto) {
    return this.http.put<AnnouncementResponse>(`${this.base}/${id}`, dto);
  }

  delete(id: string) {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
