import { VisitorStatus } from '../../core/models/enums';

export interface VisitorResponse {
  id: string;
  name: string;
  email: string;
  purpose: string;
  apartmentId: string;
  apartmentBlock: string;
  apartmentNumber: string;
  qrToken: string;
  eta: string;
  expiresAt: string;
  status: VisitorStatus;
}

export interface RegisterVisitorDto {
  name: string;
  email: string;
  purpose: string;
  eta: string;
}

export interface VisitorEntryResponse {
  id: string;
  visitorId: string;
  visitorName: string;
  checkinTime: string;
  checkoutTime: string | null;
  staffId: string;
  staffName: string;
}
