export type UserRole = 'Admin' | 'Technician';

export interface User {
  id: string;
  email: string;
  fullName: string;
  role: UserRole | string;
}

export interface AuthResponse {
  accessToken: string;
  expiresAtUtc: string;
  user: User;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  fullName: string;
  password: string;
}
