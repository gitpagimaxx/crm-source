import { api } from './api';
import type { CustomerDto, CreateCustomerRequest, EventDto } from '../types';

export interface GetCustomersParams {
  page?: number;
  pageSize?: number;
}

export async function getCustomers(params: GetCustomersParams = {}): Promise<CustomerDto[]> {
  const { page = 1, pageSize = 20 } = params;
  const qs = new URLSearchParams({
    page: String(page),
    pageSize: String(pageSize),
  });
  return api.get<CustomerDto[]>(`/api/customers?${qs}`);
}

export async function getCustomer(id: string): Promise<CustomerDto> {
  return api.get<CustomerDto>(`/api/customers/${id}`);
}

export async function createCustomer(data: CreateCustomerRequest): Promise<{ id: string }> {
  return api.post<{ id: string }>('/api/customers', data);
}

export async function getCustomerEvents(id: string): Promise<EventDto[]> {
  return api.get<EventDto[]>(`/api/customers/${id}/events`);
}
