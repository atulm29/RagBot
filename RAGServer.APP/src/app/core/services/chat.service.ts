import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ChatRequest, ChatResponse } from '../models/conversation.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private http = inject(HttpClient);

  sendMessage(request: ChatRequest): Observable<ChatResponse> {
    return this.http.post<ChatResponse>(`${environment.apiUrl}/chat`, request);
  }

  streamMessage(request: ChatRequest): Observable<string> {
    return new Observable(observer => {
      const eventSource = new EventSource(
        `${environment.apiUrl}/chat/stream`,
        { withCredentials: true }
      );

      // Send request via POST (EventSource doesn't support POST, so we use fetch)
      fetch(`${environment.apiUrl}/chat/stream`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify(request)
      }).then(response => {
        const reader = response.body?.getReader();
        const decoder = new TextDecoder();

        const readChunk = (): void => {
          reader?.read().then(({ done, value }) => {
            if (done) {
              observer.complete();
              return;
            }

            const chunk = decoder.decode(value);
            const lines = chunk.split('\n');

            lines.forEach(line => {
              if (line.startsWith('data: ')) {
                const data = line.substring(6);
                if (data === '[DONE]') {
                  observer.complete();
                } else {
                  observer.next(data);
                }
              }
            });

            readChunk();
          });
        };

        readChunk();
      }).catch(error => observer.error(error));

      return () => eventSource.close();
    });
  }
}
