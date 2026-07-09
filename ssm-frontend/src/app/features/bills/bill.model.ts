import { BillingStatus } from '../../core/models/enums';

export interface BillResponse {
  id: string;
  apartmentId: string;
  apartmentBlock: string;
  apartmentNumber: string;
  billedToUserId: string;
  billedToUserName: string;
  period: string;
  baseAmount: number;
  penalty: number;
  total: number;
  dueDate: string;
  paidAt: string | null;
  status: BillingStatus;
  transactionRef: string | null;
  isVacantRate: boolean;
}

export interface CreateBillDto {
  apartmentId: string;
  period: string;
  baseAmount: number;
  dueDate: string;
}

export interface CreatePaymentOrderResponse {
  orderId: string;
  amount: number;
  currency: string;
}

export interface VerifyPaymentDto {
  orderId: string;
  paymentId: string;
  signature: string;
}
