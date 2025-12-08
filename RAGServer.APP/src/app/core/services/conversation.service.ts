import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Conversation, CreateConversationRequest } from '../models/conversation.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ConversationService {
  private http = inject(HttpClient);

  createConversation(request: CreateConversationRequest): Observable<Conversation> {
    return this.http.post<Conversation>(`${environment.apiUrl}/conversation`, request);
  }

  getConversations(): Observable<Conversation[]> {
    return this.http.get<Conversation[]>(`${environment.apiUrl}/conversation`);
  }

  getConversation(id: string): Observable<Conversation> {
    return this.http.get<Conversation>(`${environment.apiUrl}/conversation/${id}`);
  }

  deleteConversation(id: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/conversation/${id}`);
  }
}
