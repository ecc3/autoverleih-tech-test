export interface CreateCarRequest {
  make: string;
  model: string;
  licensePlate: string;
  year: number;
}

export interface UpdateCarRequest {
  make: string;
  model: string;
  licensePlate: string;
  year: number;
}

export interface CarResponse {
  id: string;
  make: string;
  model: string;
  licensePlate: string;
  year: number;
  isAvailable: boolean;
  totalKilometers: number;
  createdAt: string;
}
