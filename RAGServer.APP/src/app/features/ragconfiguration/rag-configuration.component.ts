import { Component, inject, signal, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { ThemeService } from '../../core/services/theme.service';
import { environment } from '../../../environments/environment';

interface RagConfiguration {
  chunkSize: number;
  chunkOverlap: number;
  chunkingStrategy: string;
  embeddingModel: string;
  topK: number;
  similarityThreshold: number;
  retrievalMethod: string;
  maxCharsPerInstance: number;
  textModel: string;
  embeddingBatchSize: number;
  maxRetryAttempts: number;
}

@Component({
  selector: 'app-rag-configuration',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
      <!-- Header -->
      <div class="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div class="flex items-center justify-between">
            <div class="flex items-center space-x-4">
              <button (click)="navigateToChat()" class="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700">
                <svg class="w-6 h-6 text-gray-600 dark:text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
                </svg>
              </button>
              <h1 class="text-2xl font-bold text-gray-900 dark:text-white">RAG Configuration</h1>
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

      <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        @if (saveSuccess()) {
        <div class="mb-6 p-4 bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-lg flex items-start">
          <svg class="w-5 h-5 text-green-600 dark:text-green-400 mt-0.5 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <p class="text-green-800 dark:text-green-200">Configuration saved successfully!</p>
        </div>
        }

        @if (saveError()) {
        <div class="mb-6 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg flex items-start">
          <svg class="w-5 h-5 text-red-600 dark:text-red-400 mt-0.5 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <p class="text-red-800 dark:text-red-200">{{ saveError() }}</p>
        </div>
        }

        <!-- Chunking Settings -->
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow p-6 mb-6">
          <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Chunking Settings</h2>
          <p class="text-sm text-gray-600 dark:text-gray-400 mb-6">
            Configure how documents are divided into chunks for embedding and retrieval.
          </p>

          <div class="space-y-6">
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Chunk Size (characters)
              </label>
              <input type="number" [(ngModel)]="config.chunkSize" min="100" max="2000" step="100"
                class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 dark:bg-gray-700 dark:text-white" />
              <p class="mt-1 text-xs text-gray-500 dark:text-gray-400">
                Recommended: 500-1000 characters per chunk
              </p>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Chunk Overlap (characters)
              </label>
              <input type="number" [(ngModel)]="config.chunkOverlap" min="0" max="500" step="10"
                class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 dark:bg-gray-700 dark:text-white" />
              <p class="mt-1 text-xs text-gray-500 dark:text-gray-400">
                Overlap helps maintain context between chunks
              </p>
            </div>

            <div class="hidden">
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Chunking Strategy
              </label>
              <select [(ngModel)]="config.chunkingStrategy"
                class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 dark:bg-gray-700 dark:text-white">
                <option value="paragraph-based">Paragraph-based</option>
                <option value="sentence-based">Sentence-based</option>
                <option value="fixed-size">Fixed Size</option>
                <option value="semantic">Semantic Chunking</option>
              </select>
              <p class="mt-1 text-xs text-gray-500 dark:text-gray-400">
                Choose how to split document content
              </p>
            </div>
          </div>

          <div class="mt-6 flex space-x-4">
            <button (click)="saveChunkingSettings()" [disabled]="isSaving()"
              class="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed">
              Save Chunking Settings
            </button>
            <button (click)="resetChunkingSettings()"
              class="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700">
              Reset
            </button>
          </div>
        </div>

        <!-- Embedding Models -->
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow p-6 mb-6">
          <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Embedding Models</h2>
          <p class="text-sm text-gray-600 dark:text-gray-400 mb-6">
            Select the model to use for document vectorization.
          </p>

          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Embedding Model
            </label>
            <select [(ngModel)]="config.embeddingModel"
              class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 dark:bg-gray-700 dark:text-white">
              <option value="openai-ada-002">OpenAI text-embedding-ada-002</option>
              <option value="openai-3-small">OpenAI text-embedding-3-small</option>
              <option value="openai-3-large">OpenAI text-embedding-3-large</option>
              <option value="cohere-embed">Cohere Embed v3</option>
              <option value="palm-gecko">Google PaLM 2 (Gecko)</option>
              <option value="text-embedding-004">text-embedding-004</option>
            </select>
            <p class="mt-1 text-xs text-gray-500 dark:text-gray-400">
              Different models have different dimensions and performance characteristics
            </p>
          </div>

          <div class="mt-6 flex space-x-4">
            <button (click)="saveEmbeddingSettings()" [disabled]="isSaving()"
              class="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed">
              Save Embedding Settings
            </button>
            <button (click)="resetEmbeddingSettings()"
              class="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700">
              Reset
            </button>
          </div>
        </div>

        <!-- Retrieval Settings -->
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow p-6 mb-6">
          <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Retrieval Settings</h2>
          <p class="text-sm text-gray-600 dark:text-gray-400 mb-6">
            Configure how chunks are retrieved during query time.
          </p>

          <div class="space-y-6">
            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Top K Results
              </label>
              <input type="number" [(ngModel)]="config.topK" min="1" max="20" step="1"
                class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 dark:bg-gray-700 dark:text-white" />
              <p class="mt-1 text-xs text-gray-500 dark:text-gray-400">
                Number of most relevant chunks to retrieve
              </p>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Similarity Threshold (0-1)
              </label>
              <input type="number" [(ngModel)]="config.similarityThreshold" min="0" max="1" step="0.05"
                class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 dark:bg-gray-700 dark:text-white" />
              <div class="mt-2 flex justify-between text-xs text-gray-500 dark:text-gray-400">
                <span>Less relevant</span>
                <span>Current: {{ config.similarityThreshold }}</span>
                <span>More relevant</span>
              </div>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Retrieval Method
              </label>
              <select [(ngModel)]="config.retrievalMethod"
                class="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 dark:bg-gray-700 dark:text-white">
                <option value="similarity-search">Similarity Search</option>
                <option value="mmr">Maximal Marginal Relevance (MMR)</option>
                <option value="hybrid">Hybrid (Semantic + Keyword)</option>
                <option value="rerank">Similarity + Reranking</option>
              </select>
              <p class="mt-1 text-xs text-gray-500 dark:text-gray-400">
                Choose retrieval algorithm
              </p>
            </div>
          </div>

          <div class="mt-6 flex space-x-4">
            <button (click)="saveRetrievalSettings()" [disabled]="isSaving()"
              class="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed">
              Save Retrieval Settings
            </button>
            <button (click)="resetRetrievalSettings()"
              class="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700">
              Reset
            </button>
          </div>
        </div>

        <!-- Save All Button -->
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
          <div class="flex items-center justify-between">
            <div>
              <h3 class="text-sm font-medium text-gray-900 dark:text-white">Save All Settings</h3>
              <p class="text-xs text-gray-500 dark:text-gray-400 mt-1">
                Apply all configuration changes at once
              </p>
            </div>
            <button (click)="saveAllSettings()" [disabled]="isSaving()"
              class="px-6 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 disabled:opacity-50 disabled:cursor-not-allowed">
              @if (isSaving()) {
              <span class="flex items-center">
                <svg class="animate-spin -ml-1 mr-3 h-5 w-5 text-white" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                Saving...
              </span>
              } @else {
              <span>Save All Configuration</span>
              }
            </button>
          </div>
        </div>

        <!-- Navigation Links -->
        <div class="mt-8 text-center">
          <button (click)="navigateToChat()"
            class="text-primary-600 hover:text-primary-700 dark:text-primary-400 dark:hover:text-primary-300 font-medium">
            ← Back to Chats
          </button>
        </div>
      </div>
    </div>
  `
})
export class RagConfigurationComponent implements OnInit {
  private http = inject(HttpClient);
  private router = inject(Router);
  public themeService = inject(ThemeService);

  constructor(private cd: ChangeDetectorRef) { }


  config: RagConfiguration = {
    chunkSize: 1000,
    chunkOverlap: 200,
    chunkingStrategy: 'paragraph-based',
    embeddingModel: 'text-embedding-004',
    topK: 5,
    similarityThreshold: 0.6,
    retrievalMethod: 'similarity-search',
    maxCharsPerInstance: 12000,
    textModel: 'gemini-2.0-flash',
    embeddingBatchSize: 5,
    maxRetryAttempts: 5
  };

  defaultConfig: RagConfiguration = { ...this.config };
  isSaving = signal(false);
  saveSuccess = signal(false);
  saveError = signal('');

  ngOnInit(): void {
    this.loadConfiguration();
  }

  loadConfiguration(): void {
    this.http.get<RagConfiguration>(`${environment.apiUrl}/configuration/rag`).subscribe({
      next: (config) => {
        this.config = config;
        this.defaultConfig = { ...config };
        this.cd.markForCheck();
      },
      error: () => {
        // Use default config if load fails
      }
    });
  }

  saveChunkingSettings(): void {
    this.saveSettings({
      ...this.config,
      //chunkSize: this.config.chunkSize,
      //chunkOverlap: this.config.chunkOverlap,
    });
  }

  saveEmbeddingSettings(): void {
    this.saveSettings({
      embeddingModel: this.config.embeddingModel
    });
  }

  saveRetrievalSettings(): void {
    this.saveSettings({
      ...this.config, //topK: this.config.topK,
      similarityThreshold: this.config.similarityThreshold,
      retrievalMethod: this.config.retrievalMethod
    });
  }

  saveAllSettings(): void {
    this.saveSettings(this.config);
  }

  private saveSettings(settings: Partial<RagConfiguration>): void {
    this.isSaving.set(true);
      this.saveSuccess.set(false);
    this.saveError.set('');

    this.http.put(`${environment.apiUrl}/configuration/rag`, settings).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.saveSuccess.set(true);
        setTimeout(() => this.saveSuccess.set(false), 3000);
      },
      error: (error) => {
        this.isSaving.set(false);
        this.saveError.set(error.message || 'Failed to save configuration');
        setTimeout(() => this.saveError.set(''), 5000);
      }
    });
  }

  resetChunkingSettings(): void {
    this.config.chunkSize = this.defaultConfig.chunkSize;
    this.config.chunkOverlap = this.defaultConfig.chunkOverlap;
    this.config.chunkingStrategy = this.defaultConfig.chunkingStrategy;
  }

  resetEmbeddingSettings(): void {
    this.config.embeddingModel = this.defaultConfig.embeddingModel;
  }

  resetRetrievalSettings(): void {
    this.config.topK = this.defaultConfig.topK;
    this.config.similarityThreshold = this.defaultConfig.similarityThreshold;
    this.config.retrievalMethod = this.defaultConfig.retrievalMethod;
  }

  navigateToChat(): void {
    this.router.navigate(['/chat']);
  }
}
