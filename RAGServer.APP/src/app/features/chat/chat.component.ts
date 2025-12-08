import { Component, inject, signal, OnInit, effect, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ChatService } from '../../core/services/chat.service';
import { ConversationService } from '../../core/services/conversation.service';
import { AuthService } from '../../core/services/auth.service';
import { TenantService } from '../../core/services/tenant.service';
import { ThemeService } from '../../core/services/theme.service';
import { Conversation, Message, ChatRequest } from '../../core/models/conversation.model';
import { Role } from '../../core/models/tenant.model';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css']
})
export class ChatComponent implements OnInit {
  private chatService = inject(ChatService);
  private conversationService = inject(ConversationService);
  private authService = inject(AuthService);
  private tenantService = inject(TenantService);
  private router = inject(Router);
  public themeService = inject(ThemeService);

  @ViewChild('messagesContainer') messagesContainer?: ElementRef;

  conversations = signal<Conversation[]>([]);
  currentConversation = signal<Conversation | null>(null);
  roles = signal<Role[]>([]);
  currentUser = signal(this.authService.getCurrentUser());

  messageInput = '';
  selectedRoleId = '';
  useRag = true;
  isLoading = signal(false);
  isStreaming = signal(false);
  streamingMessage = signal('');
  errorMessage = signal('');

  constructor() {
    effect(() => {
      if (this.currentConversation()) {
        setTimeout(() => this.scrollToBottom(), 100);
      }
    });
  }

  ngOnInit(): void {
    const user = this.authService.getCurrentUser();
    if (user) {
      this.loadRoles(user.tenantId);
      this.loadConversations();
      this.selectedRoleId = user.roleId;
    }
  }

  loadRoles(tenantId: string): void {
    this.tenantService.getTenantRoles(tenantId).subscribe({
      next: (roles) => this.roles.set(roles)
    });
  }

  loadConversations(): void {
    this.conversationService.getConversations().subscribe({
      next: (conversations) => this.conversations.set(conversations)
    });
  }

  onRoleChange(): void {
    this.currentConversation.set(null);
  }

  createNewConversation(): void {
    const user = this.authService.getCurrentUser();
    if (!user || !this.selectedRoleId) return;

    this.conversationService.createConversation({
      tenantId: user.tenantId,
      roleId: this.selectedRoleId,
      title: 'New Conversation'
    }).subscribe({
      next: (conversation) => {
        this.conversations.update(convs => [conversation, ...convs]);
        this.currentConversation.set(conversation);
      }
    });
  }

  selectConversation(conversation: Conversation): void {
    this.conversationService.getConversation(conversation.id).subscribe({
      next: (conv) => this.currentConversation.set(conv)
    });
  }

  sendMessage(): void {

    const conv = this.currentConversation();
    const user = this.authService.getCurrentUser();

    if (!conv || !this.messageInput.trim() || !user) return;

    const request: ChatRequest = {
      conversationId: conv.id,
      message: this.messageInput,
      tenantId: user.tenantId,
      roleId: this.selectedRoleId,
      useRag: this.useRag
    };

    const userMessage: Message = {
      id: crypto.randomUUID(),
      role: 'user',
      content: this.messageInput,
      createdAt: new Date().toISOString()
    };

    this.currentConversation.update(c => c ? {
      ...c,
      messages: [...c.messages, userMessage]
    } : null);

    this.messageInput = '';
    this.isStreaming.set(true);
    this.streamingMessage.set('');
    this.errorMessage.set('');

    this.chatService.streamMessage(request).subscribe({
      next: (chunk) => {
        this.streamingMessage.update(msg => msg + chunk);
      },
      error: (error) => {
        this.errorMessage.set(error.message || 'Failed to send message');
        this.isStreaming.set(false);
      },
      complete: () => {
        const assistantMessage: Message = {
          id: crypto.randomUUID(),
          role: 'assistant',
          content: this.streamingMessage(),
          createdAt: new Date().toISOString()
        };

        this.currentConversation.update(c => c ? {
          ...c,
          messages: [...c.messages, assistantMessage]
        } : null);

        this.isStreaming.set(false);
        this.streamingMessage.set('');
      }
    });
  }

  onEnterPress(event: any): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  scrollToBottom(): void {
    if (this.messagesContainer) {
      this.messagesContainer.nativeElement.scrollTop =
        this.messagesContainer.nativeElement.scrollHeight;
    }
  }

  navigateToDocuments(): void {
    this.router.navigate(['/documents']);
  }

  navigateToSearch(): void {
    this.router.navigate(['/search']);
  }

  navigateToConfiguration(): void {
    this.router.navigate(['/configuration']);
  }

  logout(): void {
    this.authService.logout();
  }
}
