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
  seatsBooked: number;
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
  seatsBooked: number;
  bookFullFacility?: boolean;
}

export type AvailabilityLevel = 'Available' | 'FillingFast' | 'Full';

export interface BookingAvailabilitySlot {
  startTime: string;
  endTime: string;
  confirmedSeats: number;
  heldSeats: number;
  reservedSeats: number;
  availableSeats: number;
  availabilityLevel: AvailabilityLevel;
}

export interface BookingCalendarResponse {
  facilityId: string;
  facilityName: string;
  capacity: number;
  from: string;
  to: string;
  slots: BookingAvailabilitySlot[];
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
