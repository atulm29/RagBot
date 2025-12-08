import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class LoadingService {
  private loadingState = signal(false);
  private loadingMessage = signal('Loading...');

  isLoading = this.loadingState.asReadonly();
  message = this.loadingMessage.asReadonly();

  show(message: string = 'Loading...'): void {
    this.loadingMessage.set(message);
    this.loadingState.set(true);
  }

  hide(): void {
    this.loadingState.set(false);
  }

  updateMessage(message: string): void {
    this.loadingMessage.set(message);
  }
}
