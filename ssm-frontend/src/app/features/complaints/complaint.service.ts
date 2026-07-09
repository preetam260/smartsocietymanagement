import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ComplaintResponse, CreateComplaintDto, ResolveComplaintDto, UpdateComplaintStatusDto } from './complaint.model';

@Injectable({ providedIn: 'root' })
export class ComplaintService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/complaint`;

  create(dto: CreateComplaintDto) {
    return this.http.post<ComplaintResponse>(this.base, dto);
  }

  getMyComplaints() {
    return this.http.get<ComplaintResponse[]>(`${this.base}/my`);
  }

  getAll() {
    return this.http.get<ComplaintResponse[]>(this.base);
  }

  resolve(id: string, dto: ResolveComplaintDto) {
    return this.http.patch<ComplaintResponse>(`${this.base}/${id}/resolve`, dto);
  }

  updateStatus(id: string, dto: UpdateComplaintStatusDto) {
    return this.http.patch<ComplaintResponse>(`${this.base}/${id}/status`, dto);
  }
}
