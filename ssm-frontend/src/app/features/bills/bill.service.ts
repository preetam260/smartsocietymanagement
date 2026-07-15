import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { BillResponse, CreateBillDto, CreatePaymentOrderResponse, CompletePaymentDto } from './bill.model';
import { PagedResult, PaginationQuery } from '../../core/models/paged-result.model';

@Injectable({ providedIn: 'root' })
export class BillService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/bill`;
  private paymentBase = `${environment.apiUrl}/payment`;

  getAllPaged(query: PaginationQuery) {
    let params = new HttpParams()
      .set('pageNumber', query.pageNumber)
      .set('pageSize', query.pageSize);
    if (query.search) params = params.set('search', query.search);
    return this.http.get<PagedResult<BillResponse>>(this.base, { params });
  }

  getById(id: string) {
    return this.http.get<BillResponse>(`${this.base}/${id}`);
  }

  getPending() {
    return this.http.get<BillResponse[]>(`${this.base}/pending`);
  }

  getByApartment(apartmentId: string) {
    return this.http.get<BillResponse[]>(`${this.base}/apartment/${apartmentId}`);
  }

  getByPeriod(period: string) {
    return this.http.get<BillResponse[]>(`${this.base}/period/${period}`);
  }

  getByUser(userId: string) {
    return this.http.get<BillResponse[]>(`${this.base}/user/${userId}`);
  }

  getMyBills() {
    return this.http.get<BillResponse[]>(`${this.base}/my`);
  }

  create(dto: CreateBillDto) {
    return this.http.post<BillResponse>(this.base, dto);
  }

  applyPenalties() {
    return this.http.post<void>(`${this.base}/apply-penalties`, {});
  }

  delete(id: string) {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  createPaymentOrder(billId: string) {
    return this.http.post<CreatePaymentOrderResponse>(`${this.paymentBase}/create-order/${billId}`, {});
  }

  completePayment(dto: CompletePaymentDto) {
    return this.http.post<BillResponse>(`${this.paymentBase}/complete`, dto);
  }
}
