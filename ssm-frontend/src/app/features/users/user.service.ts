import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { UserResponse, CreateUserDto, UpdateUserDto } from './user.model';
import { PagedResult, PaginationQuery } from '../../core/models/paged-result.model';
import { UserRole } from '../../core/models/enums';

@Injectable({ providedIn: 'root' })
export class UserService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/user`;

  getAllPaged(query: PaginationQuery) {
    let params = new HttpParams()
      .set('pageNumber', query.pageNumber)
      .set('pageSize', query.pageSize);
    if (query.search) params = params.set('search', query.search);
    return this.http.get<PagedResult<UserResponse>>(this.base, { params });
  }

  getById(id: string) {
    return this.http.get<UserResponse>(`${this.base}/${id}`);
  }

  getByRole(role: UserRole) {
    return this.http.get<UserResponse[]>(`${this.base}/role/${role}`);
  }

  create(dto: CreateUserDto) {
    return this.http.post<UserResponse>(this.base, dto);
  }

  update(id: string, dto: UpdateUserDto) {
    return this.http.put<UserResponse>(`${this.base}/${id}`, dto);
  }

  activate(id: string) {
    return this.http.patch<void>(`${this.base}/${id}/activate`, {});
  }

  deactivate(id: string) {
    return this.http.patch<void>(`${this.base}/${id}/deactivate`, {});
  }

  delete(id: string) {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
