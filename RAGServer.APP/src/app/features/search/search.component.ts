import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { DocumentService } from '../../core/services/document.service';
import { AuthService } from '../../core/services/auth.service';
import { ThemeService } from '../../core/services/theme.service';
import { SearchResult } from '../../core/models/document.model';

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.css']
})
export class SearchComponent {
  private documentService = inject(DocumentService);
  private authService = inject(AuthService);
  private router = inject(Router);
  public themeService = inject(ThemeService);

  searchQuery = '';
  topK = 5;
  minSimilarity = 0.6;
  results = signal<SearchResult[]>([]);
  isSearching = signal(false);
  hasSearched = signal(false);
  errorMessage = signal('');

  search(): void {
    if (!this.searchQuery.trim()) return;

    const user = this.authService.getCurrentUser();
    if (!user) return;

    this.isSearching.set(true);
    this.errorMessage.set('');

    this.documentService.searchDocuments({
      query: this.searchQuery,
      tenantId: user.tenantId,
      roleId: user.roleId,
      topK: this.topK,
      minSimilarity: this.minSimilarity
    }).subscribe({
      next: (results) => {
        this.results.set(results);
        this.hasSearched.set(true);
        this.isSearching.set(false);
      },
      error: (error) => {
        this.errorMessage.set(error.message || 'Search failed');
        this.isSearching.set(false);
      }
    });
  }

  navigateToChat(): void {
    this.router.navigate(['/chat']);
  }
}
