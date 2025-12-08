import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoadingService } from '../../core/services/loading.service';

@Component({
  selector: 'app-global-loader',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (loadingService.isLoading()) {
    <div class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 backdrop-blur-sm">
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-2xl p-8 max-w-sm w-full mx-4">
        <!-- Spinner -->
        <div class="flex justify-center mb-4">
          <div class="relative">
            <div class="w-20 h-20 border-4 border-primary-200 dark:border-primary-800 rounded-full"></div>
            <div class="w-20 h-20 border-4 border-primary-600 border-t-transparent rounded-full animate-spin absolute top-0 left-0"></div>
          </div>
        </div>

        <!-- Message -->
        <p class="text-center text-gray-900 dark:text-white font-medium text-lg">
          {{ loadingService.message() }}
        </p>

        <!-- Progress indicator dots -->
        <div class="flex justify-center mt-4 space-x-2">
          <div class="w-2 h-2 bg-primary-600 rounded-full animate-bounce" style="animation-delay: 0ms"></div>
          <div class="w-2 h-2 bg-primary-600 rounded-full animate-bounce" style="animation-delay: 150ms"></div>
          <div class="w-2 h-2 bg-primary-600 rounded-full animate-bounce" style="animation-delay: 300ms"></div>
        </div>
      </div>
    </div>
    }
  `,
  styles: [`
    @keyframes bounce {
      0%, 100% {
        transform: translateY(0);
      }
      50% {
        transform: translateY(-0.5rem);
      }
    }
  `]
})
export class GlobalLoaderComponent {
  public loadingService = inject(LoadingService);
}
