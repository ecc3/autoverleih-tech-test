export interface CreateRentalRequest {
  customerId: string;
  carId: string;
  startDate: string;
  endDate: string;
}

export interface RentalResponse {
  id: string;
  customerId: string;
  carId: string;
  startDate: string;
  endDate: string;
  returnedAt: string | null;
  status: string;
  createdAt: string;
}
