import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ResidentResponse, CreateResidentDto, UpdateResidentDto, MoveOutResidentDto } from './resident.model';

@Injectable({ providedIn: 'root' })
export class ResidentService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/resident`;

  getAll() {
    return this.http.get<ResidentResponse[]>(this.base);
  }

  getAllCurrent() {
    return this.http.get<ResidentResponse[]>(`${this.base}/current`);
  }

  getById(id: string) {
    return this.http.get<ResidentResponse>(`${this.base}/${id}`);
  }

  getByApartment(apartmentId: string) {
    return this.http.get<ResidentResponse[]>(`${this.base}/apartment/${apartmentId}`);
  }

  getByUser(userId: string) {
    return this.http.get<ResidentResponse>(`${this.base}/user/${userId}`);
  }

  create(dto: CreateResidentDto) {
    return this.http.post<ResidentResponse>(this.base, dto);
  }

  update(id: string, dto: UpdateResidentDto) {
    return this.http.put<ResidentResponse>(`${this.base}/${id}`, dto);
  }

  moveOut(id: string, dto: MoveOutResidentDto) {
    return this.http.patch<void>(`${this.base}/${id}/moveout`, dto);
  }
}
