import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-loader',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="flex flex-col items-center justify-center p-12">
      <!-- Spinner -->
      <div class="relative">
        <div class="w-16 h-16 border-4 border-primary-200 dark:border-primary-800 rounded-full"></div>
        <div class="w-16 h-16 border-4 border-primary-600 border-t-transparent rounded-full animate-spin absolute top-0 left-0"></div>
      </div>

      <!-- Message -->
      @if (message) {
      <p class="mt-4 text-gray-600 dark:text-gray-400 text-center">{{ message }}</p>
      }

      <!-- Sub-message -->
      @if (subMessage) {
      <p class="mt-2 text-sm text-gray-500 dark:text-gray-500 text-center">{{ subMessage }}</p>
      }
    </div>
  `,
  styles: [`
    @keyframes spin {
      to {
        transform: rotate(360deg);
      }
    }
  `]
})
export class LoaderComponent {
  @Input() message: string = 'Loading...';
  @Input() subMessage?: string;
}
