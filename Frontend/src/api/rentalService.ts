import apiClient from "./axiosClient";
import type { CreateRentalRequest, RentalResponse } from "../types/rental";

export const rentalService = {
  getAll: () =>
    apiClient.get<RentalResponse[]>("/rentals").then((res) => res.data),

  getById: (id: string) =>
    apiClient.get<RentalResponse>(`/rentals/${id}`).then((res) => res.data),

  create: (data: CreateRentalRequest) =>
    apiClient.post<RentalResponse>("/rentals", data).then((res) => res.data),

  returnRental: (id: string) =>
    apiClient
      .post<RentalResponse>(`/rentals/${id}/return`)
      .then((res) => res.data),

  cancelRental: (id: string) =>
    apiClient
      .post<RentalResponse>(`/rentals/${id}/cancel`)
      .then((res) => res.data),
};
