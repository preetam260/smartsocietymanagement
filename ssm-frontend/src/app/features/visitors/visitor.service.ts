import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { VisitorResponse, RegisterVisitorDto, VisitorEntryResponse } from './visitor.model';
import { PagedResult, PaginationQuery } from '../../core/models/paged-result.model';
import { VisitorStatus } from '../../core/models/enums';

@Injectable({ providedIn: 'root' })
export class VisitorService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/visitor`;

  register(dto: RegisterVisitorDto) {
    return this.http.post<VisitorResponse>(`${this.base}/register`, dto);
  }

  getAll() {
    return this.http.get<VisitorResponse[]>(`${this.base}`);
  }

  getMyVisitors() {
    return this.http.get<VisitorResponse[]>(`${this.base}/my`);
  }

  getById(id: string) {
    return this.http.get<VisitorResponse>(`${this.base}/${id}`);
  }

  getByApartment(apartmentId: string, query: PaginationQuery) {
    let params = new HttpParams()
      .set('pageNumber', query.pageNumber)
      .set('pageSize', query.pageSize);
    return this.http.get<PagedResult<VisitorResponse>>(`${this.base}/apartment/${apartmentId}`, { params });
  }

  getByStatus(status: VisitorStatus) {
    return this.http.get<VisitorResponse[]>(`${this.base}/status/${status}`);
  }

  getByUser(userId: string) {
    return this.http.get<VisitorResponse[]>(`${this.base}/user/${userId}`);
  }

  deny(id: string) {
    return this.http.patch<void>(`${this.base}/${id}/deny`, {});
  }

  checkIn(qrImageFile: File) {
    const formData = new FormData();
    formData.append('QrToken', qrImageFile);
    return this.http.post<VisitorEntryResponse>(`${this.base}/checkin`, formData);
  }

  checkOut(id: string) {
    return this.http.patch<VisitorEntryResponse>(`${this.base}/${id}/checkout`, {});
  }

  getEntries(visitorId: string) {
    return this.http.get<VisitorEntryResponse[]>(`${this.base}/${visitorId}/entries`);
  }
}
