export interface CreateCustomerRequest {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
}

export interface UpdateCustomerRequest {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
}

export interface CustomerResponse {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string | null;
  createdAt: string;
}
