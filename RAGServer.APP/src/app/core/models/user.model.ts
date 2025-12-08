export interface User {
  id: string;
  email: string;
  username: string;
  firstName?: string;
  lastName?: string;
  tenantId: string;
  tenantName: string;
  roleId: string;
  roleName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  user: User;
}

export interface RegisterRequest {
  email: string;
  username: string;
  password: string;
  firstName: string;
  lastName: string;
  tenantId: string;
  roleId: string;
}
