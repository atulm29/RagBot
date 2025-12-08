import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { RouterModule, RouterOutlet } from '@angular/router';
import { GlobalLoaderComponent } from './shared/components/gloabal-loader.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, CommonModule, RouterModule, GlobalLoaderComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('RAGServerAPP');
}
