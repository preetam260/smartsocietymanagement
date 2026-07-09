import { ComplaintStatus } from '../../core/models/enums';

export interface ComplaintResponse {
  id: string;
  userId: string;
  userName: string;
  apartmentId: string;
  apartmentBlock: string;
  apartmentNumber: string;
  title: string;
  description: string;
  status: ComplaintStatus;
  adminResponse: string | null;
  createdAt: string;
}

export interface CreateComplaintDto {
  apartmentId: string;
  title: string;
  description: string;
}

export interface ResolveComplaintDto {
  adminResponse: string;
}

export interface UpdateComplaintStatusDto {
  status: ComplaintStatus;
  adminResponse?: string;
}
