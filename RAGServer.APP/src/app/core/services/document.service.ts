import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Document, DocumentChunk, DocumentUploadResponse, SearchDocumentsRequest, SearchResult } from '../models/document.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class DocumentService {
  private http = inject(HttpClient);

  uploadDocument(file: File, tenantId: string, roleId: string, isPublic: boolean = false): Observable<DocumentUploadResponse> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('tenantId', tenantId);
    formData.append('roleId', roleId);
    formData.append('isPublic', isPublic.toString());

    return this.http.post<DocumentUploadResponse>(`${environment.apiUrl}/document/upload`, formData);
  }

  getDocuments(tenantId?: string): Observable<Document[]> {
    let params = new HttpParams();

    if (tenantId) {
      params = params.set('tenantId', tenantId);
    }

    return this.http.get<Document[]>(`${environment.apiUrl}/document`, { params });
  }


  getDocument(id: string): Observable<Document> {
    return this.http.get<Document>(`${environment.apiUrl}/document/${id}`);
  }

  deleteDocument(id: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/document/${id}`);
  }

  searchDocuments(request: SearchDocumentsRequest): Observable<SearchResult[]> {
    return this.http.post<SearchResult[]>(`${environment.apiUrl}/search/documents`, request);
  }

  processDocument(id: string): Observable<any> {
    return this.http.get(`${environment.apiUrl}/document/process/${id}`);
  }

  getDocumentChunks(documentId: string): Observable<DocumentChunk[]> {
    return this.http.get<DocumentChunk[]>(`${environment.apiUrl}/document/${documentId}/chunks`);
  }
}
