export interface ResidentResponse {
  id: string;
  userId: string;
  userName: string;
  userEmail: string;
  apartmentId: string;
  apartmentBlock: string;
  apartmentNumber: string;
  moveInDate: string;
  moveOutDate: string | null;
  vehicleNumber: string | null;
}

export interface CreateResidentDto {
  userId: string;
  apartmentId: string;
  moveInDate: string;
  vehicleNumber?: string;
}

export interface UpdateResidentDto {
  vehicleNumber?: string;
}

export interface MoveOutResidentDto {
  moveOutDate: string;
}
