import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DocumentService } from '../../core/services/document.service';
import { AuthService } from '../../core/services/auth.service';
import { ThemeService } from '../../core/services/theme.service';
import { Document } from '../../core/models/document.model';

@Component({
  selector: 'app-documents',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './documents.component.html',
  styleUrls: ['./documents.component.css'],
})
export class DocumentsComponent implements OnInit {
  private documentService = inject(DocumentService);
  private authService = inject(AuthService);
  private router = inject(Router);
  public themeService = inject(ThemeService);

  documents = signal<Document[]>([]);
  isLoading = signal(false);
  isUploading = signal(false);
  uploadProgress = signal(0);
  uploadError = signal('');
  uploadSuccess = signal(false);
  selectedFile: File | null = null;
  isPublic = false;

  ngOnInit(): void {
    this.loadDocuments();
  }

  loadDocuments(): void {
    this.isLoading.set(true);
    const user = this.authService.getCurrentUser();

    this.documentService.getDocuments(user?.tenantId).subscribe({
      next: (docs) => {
        this.documents.set(docs);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  onFileSelect(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
      this.uploadError.set('');
      this.uploadSuccess.set(false);
    }
  }

  uploadDocument(): void {
    if (!this.selectedFile) return;

    const user = this.authService.getCurrentUser();
    if (!user) return;

    this.isUploading.set(true);
    this.uploadProgress.set(0);
    this.uploadError.set('');
    this.uploadSuccess.set(false);

    // Simulate progress
    const progressInterval = setInterval(() => {
      this.uploadProgress.update(p => Math.min(p + 10, 90));
    }, 200);

    this.documentService.uploadDocument(
      this.selectedFile,
      user.tenantId,
      user.roleId,
      this.isPublic
    ).subscribe({
      next: () => {
        clearInterval(progressInterval);
        this.uploadProgress.set(100);
        this.uploadSuccess.set(true);
        this.isUploading.set(false);
        this.selectedFile = null;
        this.isPublic = false;

        setTimeout(() => {
          this.uploadSuccess.set(false);
          this.uploadProgress.set(0);
          this.loadDocuments();
        }, 2000);
      },
      error: (error) => {
        clearInterval(progressInterval);
        this.uploadError.set(error.message || 'Upload failed');
        this.isUploading.set(false);
        this.uploadProgress.set(0);
      }
    });
  }

  deleteDocument(id: string): void {
    if (confirm('Are you sure you want to delete this document?')) {
      this.documentService.deleteDocument(id).subscribe({
        next: () => {
          this.loadDocuments();
        }
      });
    }
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }

  navigateToChat(): void {
    this.router.navigate(['/chat']);
  }

  processDocument(id: string) {
    this.documentService.processDocument(id).subscribe({
     next: () => {
       this.loadDocuments();
      }
    });
  }
   getProcessButtonText(status: string): string {
    switch (status) {
      case 'processing':
        return 'Processing...';
      case 'indexed':
      case 'ready':
        return 'Processed';
      case 'error':
        return 'Retry Processing';
      default:
        return 'Process';
    }
  }
}
