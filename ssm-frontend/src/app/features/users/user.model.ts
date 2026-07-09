import { UserRole } from '../../core/models/enums';

export interface UserResponse {
  id: string;
  name: string;
  email: string;
  phoneNumber: string;
  role: UserRole;
  isActive: boolean;
}

export interface CreateUserDto {
  name: string;
  email: string;
  phoneNumber: string;
  role: UserRole;
}

export interface UpdateUserDto {
  name: string;
  phoneNumber: string;
}
