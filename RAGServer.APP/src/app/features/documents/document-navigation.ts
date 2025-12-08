import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ThemeService } from '../../core/services/theme.service';

@Component({
  selector: 'app-document-navigation',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex items-center justify-between h-16">
          <!-- Left side - Logo and Title -->
          <div class="flex items-center space-x-4">
            <button (click)="navigateToChat()" class="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700">
              <svg class="w-6 h-6 text-gray-600 dark:text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
              </svg>
            </button>
            <h1 class="text-xl font-bold text-gray-900 dark:text-white">RAG Server</h1>
          </div>

          <!-- Center - Navigation Tabs -->
          <nav class="flex space-x-1">
            <button (click)="navigate('/documents')"
              [class.bg-primary-100]="isActive('/documents')"
              [class.text-primary-700]="isActive('/documents')"
              [class.dark:bg-primary-900]="isActive('/documents')"
              [class.dark:text-primary-200]="isActive('/documents')"
              class="px-4 py-2 rounded-lg text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors">
              <div class="flex items-center space-x-2">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
                <span>Documents</span>
              </div>
            </button>

            <button (click)="navigate('/documents/upload')"
              [class.bg-primary-100]="isActive('/documents/upload')"
              [class.text-primary-700]="isActive('/documents/upload')"
              [class.dark:bg-primary-900]="isActive('/documents/upload')"
              [class.dark:text-primary-200]="isActive('/documents/upload')"
              class="px-4 py-2 rounded-lg text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors">
              <div class="flex items-center space-x-2">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
                </svg>
                <span>Upload</span>
              </div>
            </button>

            <button (click)="navigate('/documents/configuration')"
              [class.bg-primary-100]="isActive('/documents/configuration')"
              [class.text-primary-700]="isActive('/documents/configuration')"
              [class.dark:bg-primary-900]="isActive('/documents/configuration')"
              [class.dark:text-primary-200]="isActive('/documents/configuration')"
              class="px-4 py-2 rounded-lg text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors">
              <div class="flex items-center space-x-2">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
                <span>Configuration</span>
              </div>
            </button>

            <button (click)="navigate('/search')"
              [class.bg-primary-100]="isActive('/search')"
              [class.text-primary-700]="isActive('/search')"
              [class.dark:bg-primary-900]="isActive('/search')"
              [class.dark:text-primary-200]="isActive('/search')"
              class="px-4 py-2 rounded-lg text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors">
              <div class="flex items-center space-x-2">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                </svg>
                <span>Search</span>
              </div>
            </button>
          </nav>

          <!-- Right side - Theme Toggle -->
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
  `
})
export class DocumentNavigationComponent {
  private router = inject(Router);
  public themeService = inject(ThemeService);

  navigate(path: string): void {
    this.router.navigate([path]);
  }

  navigateToChat(): void {
    this.router.navigate(['/chat']);
  }

  isActive(path: string): boolean {
    return this.router.url === path ||
           (path === '/documents' && this.router.url.startsWith('/documents') && this.router.url === '/documents');
  }
}
