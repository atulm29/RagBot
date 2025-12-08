export interface Conversation {
  id: string;
  title?: string;
  createdAt: string;
  updatedAt: string;
  messages: Message[];
}

export interface Message {
  id: string;
  role: 'user' | 'assistant' | 'system';
  content: string;
  createdAt: string;
}

export interface CreateConversationRequest {
  tenantId: string;
  roleId: string;
  title?: string;
}

export interface ChatRequest {
  conversationId: string;
  message: string;
  tenantId?: string;
  roleId?: string;
  useRag: boolean;
}

export interface ChatResponse {
  conversationId: string;
  messageId: string;
  content: string;
  references?: DocumentReference[];
  confidenceScore?: number;
}

export interface DocumentReference {
  documentId: string;
  fileName: string;
  chunkContent: string;
  similarityScore: number;
}
