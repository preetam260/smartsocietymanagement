import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { FacilityResponse, FacilityDto } from './facility.model';

@Injectable({ providedIn: 'root' })
export class FacilityService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/facility`;

  getAll() {
    return this.http.get<FacilityResponse[]>(this.base);
  }

  getActive() {
    return this.http.get<FacilityResponse[]>(`${this.base}/active`);
  }

  getById(id: string) {
    return this.http.get<FacilityResponse>(`${this.base}/${id}`);
  }

  create(dto: FacilityDto) {
    return this.http.post<FacilityResponse>(this.base, dto);
  }

  update(id: string, dto: FacilityDto) {
    return this.http.put<FacilityResponse>(`${this.base}/${id}`, dto);
  }

  delete(id: string) {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
