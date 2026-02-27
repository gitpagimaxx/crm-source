export interface LoginResponse {
  token: string;
}

export interface CustomerDto {
  id: string;
  customerType: string;
  name: string;
  document: string;
  email: string;
  birthDate?: string;
  companyName?: string;
  stateRegistration?: string;
  address?: AddressDto;
  status: string;
  createdAt: string;
  updatedAt?: string;
}

export interface AddressDto {
  zipCode?: string;
  street?: string;
  number?: string;
  complement?: string;
  neighborhood?: string;
  city?: string;
  state?: string;
}

export interface EventDto {
  id: number;
  streamId: string;
  eventType: string;
  eventData: string;
  streamVersion: number;
  createdAt: string;
  actorUserId?: string;
  actorEmail?: string;
  actorName?: string;
  correlationId?: string;
}

export interface CreateCustomerRequest {
  customerType: string;
  name: string;
  document: string;
  email: string;
  birthDate?: string;
  companyName?: string;
  stateRegistration?: string;
  zipCode?: string;
  addressNumber?: string;
  addressComplement?: string;
}
