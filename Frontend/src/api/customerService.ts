import apiClient from './axiosClient';
import type {
  CreateCustomerRequest,
  UpdateCustomerRequest,
  CustomerResponse,
} from '../types/customer';

export const customerService = {
  getAll: () =>
    apiClient.get<CustomerResponse[]>('/customers').then((res) => res.data),

  getById: (id: string) =>
    apiClient.get<CustomerResponse>(`/customers/${id}`).then((res) => res.data),

  create: (data: CreateCustomerRequest) =>
    apiClient.post<CustomerResponse>('/customers', data).then((res) => res.data),

  update: (id: string, data: UpdateCustomerRequest) =>
    apiClient.put<CustomerResponse>(`/customers/${id}`, data).then((res) => res.data),

  delete: (id: string) =>
    apiClient.delete(`/customers/${id}`),
};
