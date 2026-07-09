export interface ApartmentResponse {
  id: string;
  block: string;
  floor: number;
  number: string;
  ownerId: string;
  ownerName: string;
}

export interface CreateApartmentDto {
  ownerId: string;
  block: string;
  floor: number;
  number: string;
}
