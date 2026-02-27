import { api } from './api';
import type { LoginResponse } from '../types';

export async function login(username: string, password: string): Promise<LoginResponse> {
  return api.post<LoginResponse>('/api/auth/token', { username, password });
}
