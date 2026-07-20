import { ComplaintStatus } from '../../core/models/enums';

export interface ComplaintResponse {
  id: string;
  title: string;
  description: string;
  status: ComplaintStatus;
  adminResponse?: string;
  userName: string;
  apartmentBlock: string;
  apartmentNumber: string;
  createdAt: string;

  category?: ComplaintCategory;
  priority?: ComplaintPriority;
  draftAdminResponse?: string;
  possibleDuplicateIdsCsv?: string;
  triageProcessed: boolean;
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

export type ComplaintCategory = 'Plumbing' | 'Electrical' | 'Security' | 'Parking' | 'Noise' | 'CommonArea' | 'Other';
export type ComplaintPriority = 'Low' | 'Medium' | 'High' | 'Urgent';

export interface Complaint {
  id: string;
  title: string;
  description: string;
  status: 'Open' | 'InProgress' | 'Resolved';
  adminResponse?: string;
  category?: ComplaintCategory;
  priority?: ComplaintPriority;
  draftAdminResponse?: string;
  possibleDuplicateIdsCsv?: string;
  triageProcessed: boolean;
  createdAt: string;
}

export const PRIORITY_ORDER: Record<ComplaintPriority, number> = {
  Urgent: 0, High: 1, Medium: 2, Low: 3
};