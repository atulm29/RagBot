import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { DocumentService } from '../../core/services/document.service';
import { AuthService } from '../../core/services/auth.service';
import { ThemeService } from '../../core/services/theme.service';

@Component({
  selector: 'app-upload-document',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
      <!-- Header -->
      <div class="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div class="flex items-center justify-between">
            <div class="flex items-center space-x-4">
              <button (click)="navigateBack()" class="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700">
                <svg class="w-6 h-6 text-gray-600 dark:text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
                </svg>
              </button>
              <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Upload Document</h1>
            </div>
            <button (click)="themeService.toggleTheme()" class="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700">
              @if (themeService.isDarkMode()) {
              <svg class="w-5 h-5 text-gray-600 dark:text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z" />
              </svg>
              } @else {
              <svg class="w-5 h-5 text-gray-600 dark:text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
              </svg>
              }
            </button>
          </div>
        </div>
      </div>

      <div class="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <!-- Upload Card -->
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-lg p-8">
          <h2 class="text-xl font-semibold text-gray-900 dark:text-white mb-6">
            Upload Document to RAG System
          </h2>

          @if (uploadError()) {
          <div class="mb-6 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg flex items-start">
            <svg class="w-5 h-5 text-red-600 dark:text-red-400 mt-0.5 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <div class="flex-1">
              <p class="text-red-800 dark:text-red-200">{{ uploadError() }}</p>
            </div>
            <button (click)="uploadError.set('')" class="text-red-600 dark:text-red-400 hover:text-red-800">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
          }

          @if (uploadSuccess()) {
          <div class="mb-6 p-4 bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-lg flex items-start">
            <svg class="w-5 h-5 text-green-600 dark:text-green-400 mt-0.5 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <p class="text-green-800 dark:text-green-200">Document uploaded successfully!</p>
          </div>
          }

          <!-- File Upload Area -->
          <div class="mb-6">
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Select Document
            </label>
            <div class="mt-1 flex justify-center px-6 pt-5 pb-6 border-2 border-gray-300 dark:border-gray-600 border-dashed rounded-lg hover:border-primary-500 dark:hover:border-primary-400 transition-colors"
              [class.border-primary-500]="selectedFile"
              [class.bg-primary-50]="selectedFile"
              [class.dark:bg-primary-900/10]="selectedFile">
              <div class="space-y-1 text-center">
                @if (!selectedFile) {
                <svg class="mx-auto h-12 w-12 text-gray-400" stroke="currentColor" fill="none" viewBox="0 0 48 48">
                  <path d="M28 8H12a4 4 0 00-4 4v20m32-12v8m0 0v8a4 4 0 01-4 4H12a4 4 0 01-4-4v-4m32-4l-3.172-3.172a4 4 0 00-5.656 0L28 28M8 32l9.172-9.172a4 4 0 015.656 0L28 28m0 0l4 4m4-24h8m-4-4v8m-12 4h.02" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" />
                </svg>
                <div class="flex text-sm text-gray-600 dark:text-gray-400">
                  <label for="file-upload" class="relative cursor-pointer rounded-md font-medium text-primary-600 hover:text-primary-500">
                    <span>Upload a file</span>
                    <input id="file-upload" type="file" class="sr-only" (change)="onFileSelect($event)" accept=".pdf,.docx,.doc,.txt" />
                  </label>
                  <p class="pl-1">or drag and drop</p>
                </div>
                <p class="text-xs text-gray-500 dark:text-gray-400">
                  PDF, DOCX, DOC, TXT up to 20MB
                </p>
                } @else {
                <svg class="mx-auto h-12 w-12 text-primary-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
                <p class="text-sm font-medium text-gray-900 dark:text-white">{{ selectedFile.name }}</p>
                <p class="text-xs text-gray-500 dark:text-gray-400">{{ formatFileSize(selectedFile.size) }}</p>
                <button (click)="clearFile()" class="mt-2 text-sm text-red-600 hover:text-red-800 dark:text-red-400 dark:hover:text-red-300">
                  Remove file
                </button>
                }
              </div>
            </div>
          </div>

          <!-- Document Options -->
          <div class="space-y-4 mb-6">
            <div class="flex items-center">
              <input type="checkbox" id="isPublic" [(ngModel)]="isPublic"
                class="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded" />
              <label for="isPublic" class="ml-2 block text-sm text-gray-700 dark:text-gray-300">
                Make document publicly accessible
              </label>
            </div>

            <div class="flex items-center">
              <input type="checkbox" id="autoProcess" [(ngModel)]="autoProcess"
                class="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded" />
              <label for="autoProcess" class="ml-2 block text-sm text-gray-700 dark:text-gray-300">
                Automatically process document after upload
              </label>
            </div>
          </div>

          <!-- Upload Progress -->
          @if (uploadProgress() > 0 && uploadProgress() < 100) {
          <div class="mb-6">
            <div class="flex justify-between text-sm text-gray-700 dark:text-gray-300 mb-2">
              <span>Uploading...</span>
              <span>{{ uploadProgress() }}%</span>
            </div>
            <div class="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
              <div class="bg-primary-600 h-2 rounded-full transition-all duration-300"
                [style.width.%]="uploadProgress()"></div>
            </div>
          </div>
          }

          <!-- Actions -->
          <div class="flex items-center justify-end space-x-4">
            <button (click)="navigateBack()"
              [disabled]="isUploading()"
              class="px-6 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 disabled:opacity-50 disabled:cursor-not-allowed">
              Cancel
            </button>
            <button (click)="uploadDocument()"
              [disabled]="!selectedFile || isUploading()"
              class="px-6 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 disabled:opacity-50 disabled:cursor-not-allowed flex items-center">
              @if (isUploading()) {
              <svg class="animate-spin -ml-1 mr-3 h-5 w-5 text-white" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              <span>Uploading...</span>
              } @else {
              <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
              </svg>
              <span>Upload Document</span>
              }
            </button>
          </div>
        </div>

        <!-- Info Card -->
        <div class="mt-6 bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg p-6">
          <h3 class="flex items-center text-sm font-medium text-blue-900 dark:text-blue-200 mb-2">
            <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            Document Processing Information
          </h3>
          <ul class="text-sm text-blue-800 dark:text-blue-300 space-y-1">
            <li>• Documents will be chunked based on your configuration settings</li>
            <li>• Text will be embedded using the selected embedding model</li>
            <li>• Processed documents can be searched using semantic search</li>
            <li>• Maximum file size: 20MB</li>
          </ul>
        </div>
      </div>
    </div>
  `
})
export class UploadDocumentComponent {
  private documentService = inject(DocumentService);
  private authService = inject(AuthService);
  private router = inject(Router);
  public themeService = inject(ThemeService);

  selectedFile: File | null = null;
  isPublic = false;
  autoProcess = true;
  isUploading = signal(false);
  uploadProgress = signal(0);
  uploadError = signal('');
  uploadSuccess = signal(false);

  onFileSelect(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];

      // Validate file size (20MB)
      if (file.size > 20 * 1024 * 1024) {
        this.uploadError.set('File size exceeds 20MB limit');
        return;
      }

      // Validate file type
      const allowedTypes = ['application/pdf', 'application/vnd.openxmlformats-officedocument.wordprocessingml.document', 'application/msword', 'text/plain'];
      if (!allowedTypes.includes(file.type)) {
        this.uploadError.set('Invalid file type. Please upload PDF, DOCX, DOC, or TXT files');
        return;
      }

      this.selectedFile = file;
      this.uploadError.set('');
      this.uploadSuccess.set(false);
    }
  }

  clearFile(): void {
    this.selectedFile = null;
    const fileInput = document.getElementById('file-upload') as HTMLInputElement;
    if (fileInput) {
      fileInput.value = '';
    }
  }

  uploadDocument(): void {
    if (!this.selectedFile) return;

    const user = this.authService.getCurrentUser();
    if (!user) {
      this.uploadError.set('User not authenticated');
      return;
    }

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
      next: (response) => {
        clearInterval(progressInterval);
        this.uploadProgress.set(100);
        this.uploadSuccess.set(true);
        this.isUploading.set(false);

        // Auto-process if enabled
        if (this.autoProcess && response.documentId) {
          this.documentService.processDocument(response.documentId).subscribe({
            next: (response) => {
              window.location.reload();
            }
          });
        }

        // Reset form after 2 seconds and navigate back
        setTimeout(() => {
          this.selectedFile = null;
          this.isPublic = false;
          this.uploadProgress.set(0);
          this.uploadSuccess.set(false);
          this.router.navigate(['/documents']);
        }, 2000);
      },
      error: (error) => {
        clearInterval(progressInterval);
        this.uploadError.set(error.message || 'Upload failed. Please try again.');
        this.isUploading.set(false);
        this.uploadProgress.set(0);
      }
    });
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }

  navigateBack(): void {
    this.router.navigate(['/documents']);
  }
}
