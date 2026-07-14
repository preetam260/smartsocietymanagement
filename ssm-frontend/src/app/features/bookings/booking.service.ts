import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { BookingResponse, CreateBookingDto, CreatePaymentOrderResponse, VerifyBookingPaymentDto } from './booking.model';
import { PagedResult, PaginationQuery } from '../../core/models/paged-result.model';

@Injectable({ providedIn: 'root' })
export class BookingService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/booking`;

  create(dto: CreateBookingDto) {
    return this.http.post<BookingResponse>(this.base, dto);
  }

  getAll(query?: PaginationQuery) {
    let params = new HttpParams()
      .set('pageNumber', query?.pageNumber ?? 1)
      .set('pageSize', query?.pageSize ?? 20);
    return this.http.get<PagedResult<BookingResponse>>(this.base, { params });
  }

  getMyBookings() {
    return this.http.get<BookingResponse[]>(`${this.base}/my`);
  }

  getById(id: string) {
    return this.http.get<BookingResponse>(`${this.base}/${id}`);
  }

  getByFacility(facilityId: string, query: PaginationQuery) {
    let params = new HttpParams()
      .set('pageNumber', query.pageNumber)
      .set('pageSize', query.pageSize);
    return this.http.get<PagedResult<BookingResponse>>(`${this.base}/facility/${facilityId}`, { params });
  }

  getByUser(userId: string) {
    return this.http.get<BookingResponse[]>(`${this.base}/user/${userId}`);
  }

  cancel(id: string) {
    return this.http.patch<void>(`${this.base}/${id}/cancel`, {});
  }

  complete(id: string) {
    return this.http.patch<void>(`${this.base}/${id}/complete`, {});
  }

  expireHolds() {
    return this.http.post<void>(`${this.base}/expire-holds`, {});
  }

  createPaymentOrder(bookingId: string) {
    return this.http.post<CreatePaymentOrderResponse>(`${this.base}/${bookingId}/create-order`, {});
  }

  verifyPayment(bookingId: string, dto: VerifyBookingPaymentDto) {
    return this.http.post<BookingResponse>(`${this.base}/${bookingId}/verify-payment`, dto);
  }
}
