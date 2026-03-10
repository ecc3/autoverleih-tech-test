import apiClient from "./axiosClient";
import type {
  CreateCarRequest,
  UpdateCarRequest,
  CarResponse,
} from "../types/car";

export const carService = {
  getAll: () => apiClient.get<CarResponse[]>("/cars").then((res) => res.data),

  getById: (id: string) =>
    apiClient.get<CarResponse>(`/cars/${id}`).then((res) => res.data),

  create: (data: CreateCarRequest) =>
    apiClient.post<CarResponse>("/cars", data).then((res) => res.data),

  update: (id: string, data: UpdateCarRequest) =>
    apiClient.put<CarResponse>(`/cars/${id}`, data).then((res) => res.data),

  delete: (id: string) => apiClient.delete(`/cars/${id}`),
};
