import { UserRole } from '../../core/models/enums';

export interface AnnouncementResponse {
  id: string;
  title: string;
  content: string;
  audience: UserRole;
  isPinned: boolean;
  expiresAt: string;
  createdAt: string;
}

export interface AnnouncementDto {
  title: string;
  content: string;
  audience: UserRole;
  isPinned: boolean;
  expiresAt: string;
}
