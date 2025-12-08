// export interface Document {
//   id: string;
//   fileName: string;
//   contentType: string;
//   fileSize: number;
//   status: 'processing' | 'indexed' | 'error';
//   createdAt: string;
// }

export interface Document {
  id: string;
  originalFileName: string,
  fileName: string;
  contentType: string;
  fileSize: number;
  tenantId: string;
  roleId: string;
  uploadedBy: string;
  isPublic: boolean;
  status: 'pending' | 'processing' | 'indexed' | 'error' | 'ready';
  createdAt: string;
  updatedAt: string;
  metadata?: {
    chunkCount?: number;
    processingTime?: number;
    errorMessage?: string;
  };
}

export interface UploadDocumentRequest {
  file: File;
  tenantId: string;
  roleId: string;
  isPublic: boolean;
}

export interface DocumentUploadResponse {
  documentId: string;
  fileName: string;
  gcsUri: string;
  status: string;
}

export interface SearchDocumentsRequest {
  query: string;
  tenantId?: string;
  roleId?: string;
  topK?: number;
  minSimilarity?: number;
}

export interface SearchResult {
  documentId: string;
  fileName: string;
  chunkContent: string;
  similarityScore: number;
  chunkIndex: number;
}

export interface DocumentChunk {
  id: string;
  documentId: string;
  chunkIndex: number;
  content: string;
  tokenCount: number;
  createdAt: string;
  metadata?: any;
}
