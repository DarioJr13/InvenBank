export interface TokenPayload {
  sub: string;
  email: string;
  role: string;
  firstName: string;
  lastName: string;
  exp: number;
  iat: number;
}
