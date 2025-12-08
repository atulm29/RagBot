export interface ApiResponse<T> {
  data?: T;
  error?: string;
  statusCode: number;
}

export interface ApiError {
  error: string;
  statusCode: number;
  timestamp: string;
}
