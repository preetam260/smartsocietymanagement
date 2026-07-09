import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ApartmentResponse, CreateApartmentDto } from './apartment.model';
import { PagedResult, PaginationQuery } from '../../core/models/paged-result.model';

@Injectable({ providedIn: 'root' })
export class ApartmentService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/apartment`;

  getAllPaged(query: PaginationQuery) {
    let params = new HttpParams()
      .set('pageNumber', query.pageNumber)
      .set('pageSize', query.pageSize);
    if (query.search) params = params.set('search', query.search);
    return this.http.get<PagedResult<ApartmentResponse>>(this.base, { params });
  }


  getAllForDropdown(search?: string) {
    let params = new HttpParams().set('pageNumber', 1).set('pageSize', 100);
    if (search) params = params.set('search', search);
    return this.http.get<PagedResult<ApartmentResponse>>(this.base, { params });
  }

  getById(id: string) {
    return this.http.get<ApartmentResponse>(`${this.base}/${id}`);
  }

  getMyApartments() {
    return this.http.get<ApartmentResponse[]>(`${this.base}/my`);
  }

  create(dto: CreateApartmentDto) {
    return this.http.post<ApartmentResponse>(this.base, dto);
  }

  delete(id: string) {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
