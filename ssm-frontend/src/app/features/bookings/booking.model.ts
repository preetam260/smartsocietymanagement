import { BookingStatus } from '../../core/models/enums';

export interface BookingResponse {
  id: string;
  facilityId: string;
  facilityName: string;
  userId: string;
  userName: string;
  date: string;
  startTime: string;
  endTime: string;
  totalCost: number;
  status: BookingStatus;
  holdExpiresAt: string | null;
  transactionRef: string | null;
}

export interface CreateBookingDto {
  facilityId: string;
  date: string;       
  startTime: string;  
  endTime: string;  
}

export interface CreatePaymentOrderResponse {
  orderId: string;
  amount: number;
  currency: string;
}

export interface VerifyBookingPaymentDto {
  orderId: string;
  paymentId: string;
  signature: string;
}
