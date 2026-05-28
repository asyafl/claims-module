export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  role: UserRole;
  userId: string;
  expiresAt: string;
}

export type UserRole = 'handler' | 'supervisor' | 'manager';

export interface AuthUser {
  token: string;
  email: string;
  role: UserRole;
  userId: string;
}

export const MOCK_USERS: { email: string; password: string; role: UserRole; label: string }[] = [
  { email: 'handler@claims.io', password: 'Handler123!', role: 'handler', label: 'Claims Handler' },
  { email: 'supervisor@claims.io', password: 'Supervisor123!', role: 'supervisor', label: 'Claims Supervisor' },
  { email: 'manager@claims.io', password: 'Manager123!', role: 'manager', label: 'Claims Manager' },
];
