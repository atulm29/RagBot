import { Routes } from '@angular/router';
import { guestGuard } from './core/guards/guest.guard';
import { authGuard } from './core/guards/auth.guard';


export const routes: Routes = [
  {
    path: '',
    redirectTo: '/chat',
    pathMatch: 'full'
  },
  {
    path: 'auth',
    canActivate: [guestGuard],
    children: [
      {
        path: 'login',
        loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
      },
      {
        path: 'register',
        loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
      },
      {
        path: '',
        redirectTo: 'login',
        pathMatch: 'full'
      }
    ]
  },
  {
    path: 'chat',
    canActivate: [authGuard],
    loadComponent: () => import('./features/chat/chat.component').then(m => m.ChatComponent)
  },
  {
    path: 'documents',
    children: [
      {
        path: '',
        loadComponent: () => import('./features/documents/document-list.component').then(m => m.DocumentListComponent)
      },
      {
        path: 'upload',
        loadComponent: () => import('./features/documents/upload-document.component').then(m => m.UploadDocumentComponent)
      },
      {
        path: ':id',
        loadComponent: () => import('./features/documents/document-view.component').then(m => m.DocumentViewComponent)
      }
    ]
  },
  {
    path: 'configuration',
    loadComponent: () => import('./features/ragconfiguration/rag-configuration.component').then(m => m.RagConfigurationComponent)
  },
  // {
  //   path: 'documents',
  //   canActivate: [authGuard],
  //   loadComponent: () => import('./features/documents/documents.component').then(m => m.DocumentsComponent)
  // },
  {
    path: 'search',
    canActivate: [authGuard],
    loadComponent: () => import('./features/search/search.component').then(m => m.SearchComponent)
  },
  {
    path: '**',
    redirectTo: '/chat'
  }
];
