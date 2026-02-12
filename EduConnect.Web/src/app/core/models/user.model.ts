export interface User {
  id: string;
  email: string;
  fullName: string;
  role: UserRole | string;
  mustChangePassword: boolean;
}

export enum UserRole {
  Admin = 1,
  Teacher = 2,
  Parent = 3
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
}
