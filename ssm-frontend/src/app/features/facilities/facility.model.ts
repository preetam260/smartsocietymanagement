export interface FacilityResponse {
  id: string;
  name: string;
  description: string;
  hourlyRate: number;
  capacity: number;
  isActive: boolean;
}

export interface FacilityDto {
  name: string;
  description: string;
  hourlyRate: number;
  capacity: number;
  isActive: boolean;
}
