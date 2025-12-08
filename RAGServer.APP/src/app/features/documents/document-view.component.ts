import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { DocumentService } from '../../core/services/document.service';
import { ThemeService } from '../../core/services/theme.service';
import { Document, DocumentChunk } from '../../core/models/document.model';
import { LoaderComponent } from '../../shared/components/loader.component';



@Component({
  selector: 'app-document-view',
  standalone: true,
  imports: [CommonModule, LoaderComponent],
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
              <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Document Details</h1>
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

      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        @if (isLoading()) {
          <app-loader message="Loading document details..."></app-loader>
        } @else if (document()) {
        <div class="space-y-6">
          <!-- Document Info Card -->
          <div class="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
            <div class="flex items-start justify-between mb-6">
              <div class="flex-1">
                <h2 class="text-2xl font-bold text-gray-900 dark:text-white mb-2">
                  {{ document()!.originalFileName }}
                </h2>
                <div class="flex items-center space-x-2">
                  <span class="px-3 py-1 rounded-full text-sm font-semibold"
                    [ngClass]="{
                      'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200': document()!.status === 'pending',
                      'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200': document()!.status === 'processing',
                      'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200': document()!.status === 'indexed',
                      'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200': document()!.status === 'error'
                    }">
                    {{ document()!.status }}
                  </span>
                  @if (document()!.isPublic) {
                  <span class="px-3 py-1 rounded-full text-sm font-semibold bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200">
                    Public
                  </span>
                  }
                </div>
              </div>
            </div>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <label class="block text-sm font-medium text-gray-500 dark:text-gray-400 mb-1">Type</label>
                <p class="text-gray-900 dark:text-white">{{ document()!.contentType }}</p>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-500 dark:text-gray-400 mb-1">Size</label>
                <p class="text-gray-900 dark:text-white">{{ formatFileSize(document()!.fileSize) }}</p>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-500 dark:text-gray-400 mb-1">Uploaded</label>
                <p class="text-gray-900 dark:text-white">{{ document()!.createdAt | date: 'medium' }}</p>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-500 dark:text-gray-400 mb-1">Chunks</label>
                <p class="text-gray-900 dark:text-white">
                  {{ chunks().length > 0 ? chunks().length : 'Not processed' }}
                </p>
              </div>
            </div>

            <div class="mt-6 flex items-center space-x-4">
              <button (click)="processDocument()"
                [disabled]="isProcessing() || document()!.status === 'processing' || document()!.status === 'indexed'"
                class="px-6 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 disabled:opacity-50 disabled:cursor-not-allowed flex items-center">
                @if (isProcessing()) {
                <svg class="animate-spin -ml-1 mr-3 h-5 w-5 text-white" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                <span>Processing...</span>
                } @else {
                <span>Generate Embeddings</span>
                }
              </button>
              <button (click)="navigateBack()" class="px-6 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700">
                Back to List
              </button>
            </div>
          </div>

          <!-- Processing Status -->
          @if (processingStatus()) {
          <div class="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg p-4">
            <div class="flex items-start">
              <svg class="w-5 h-5 text-blue-600 dark:text-blue-400 mt-0.5 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <p class="text-blue-800 dark:text-blue-200">{{ processingStatus() }}</p>
            </div>
          </div>
          }

          <!-- Document Chunks -->
          <div class="bg-white dark:bg-gray-800 rounded-lg shadow">
            <div class="p-6 border-b border-gray-200 dark:border-gray-700">
              <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
                Document Chunks ({{ chunks().length }})
              </h3>
            </div>

            @if (isLoadingChunks()) {
              <app-loader message="Loading chunks..."></app-loader>
            } @else if (chunks().length === 0) {
            <div class="p-12 text-center">
              <svg class="w-16 h-16 text-gray-400 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
              <p class="text-gray-500 dark:text-gray-400 mb-4">
                No chunks available. Click "Generate Embeddings" to process this document.
              </p>
            </div>
            } @else {
            <div class="divide-y divide-gray-200 dark:divide-gray-700">
              @for (chunk of chunks(); track chunk.id) {
              <div class="p-6 hover:bg-gray-50 dark:hover:bg-gray-700/50">
                <div class="flex items-start justify-between mb-3">
                  <div class="flex items-center space-x-2">
                    <span class="px-3 py-1 bg-primary-100 dark:bg-primary-900 text-primary-700 dark:text-primary-200 rounded-full text-sm font-medium">
                      Chunk #{{ chunk.chunkIndex + 1 }}
                    </span>
                    <span class="text-sm text-gray-500 dark:text-gray-400">
                      {{ chunk.tokenCount }} tokens
                    </span>
                  </div>
                  <button (click)="copyToClipboard(chunk.content)"
                    class="p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-600"
                    title="Copy to clipboard">
                    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" />
                    </svg>
                  </button>
                </div>
                <div class="text-gray-700 dark:text-gray-300 whitespace-pre-wrap text-sm leading-relaxed">
                  {{ chunk.content }}
                </div>
                <div class="mt-2 text-xs text-gray-500 dark:text-gray-400">
                  Created: {{ chunk.createdAt | date: 'short' }}
                </div>
              </div>
              }
            </div>
            }
          </div>
        </div>
        } @else {
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow p-12 text-center">
          <svg class="w-16 h-16 text-gray-400 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.172 16.172a4 4 0 015.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <p class="text-gray-500 dark:text-gray-400 mb-4">Document not found</p>
          <button (click)="navigateBack()" class="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700">
            Back to Documents
          </button>
        </div>
        }
      </div>
    </div>
  `
})
export class DocumentViewComponent implements OnInit {
  private documentService = inject(DocumentService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  public themeService = inject(ThemeService);

  document = signal<Document | null>(null);
  chunks = signal<DocumentChunk[]>([]);
  isLoading = signal(false);
  isLoadingChunks = signal(false);
  isProcessing = signal(false);
  processingStatus = signal('');
  documentId: string = '';

  ngOnInit(): void {
    this.documentId = this.route.snapshot.paramMap.get('id') || '';
    if (this.documentId) {
      this.loadDocument();
      this.loadChunks();
    }
  }

  loadDocument(): void {
    this.isLoading.set(true);
    this.documentService.getDocument(this.documentId).subscribe({
      next: (doc) => {
        this.document.set(doc);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  loadChunks(): void {
    this.isLoadingChunks.set(true);
    // Assuming you add this method to DocumentService
    this.documentService.getDocumentChunks(this.documentId).subscribe({
      next: (chunks) => {
        this.chunks.set(chunks);
        this.isLoadingChunks.set(false);
      },
      error: () => {
        this.isLoadingChunks.set(false);
      }
    });
  }

  processDocument(): void {
    this.isProcessing.set(true);
    this.processingStatus.set('Processing document... This may take a few minutes.');

    this.documentService.processDocument(this.documentId).subscribe({
      next: (response: any) => {
        this.processingStatus.set(`Successfully processed ${response.chunkCount} chunks`);
        this.isProcessing.set(false);

        setTimeout(() => {
          this.processingStatus.set('');
          this.loadDocument();
          this.loadChunks();
        }, 3000);
      },
      error: (error) => {
        this.processingStatus.set(`Error: ${error.message}`);
        this.isProcessing.set(false);
      }
    });
  }

  copyToClipboard(text: string): void {
    navigator.clipboard.writeText(text).then(() => {
      // Could add a toast notification here
      console.log('Copied to clipboard');
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
