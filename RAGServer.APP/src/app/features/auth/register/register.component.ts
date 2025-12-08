import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { TenantService } from '../../../core/services/tenant.service';
import { Tenant, Role } from '../../../core/models/tenant.model';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private tenantService = inject(TenantService);
  private router = inject(Router);

  registerForm: FormGroup;
  isLoading = signal(false);
  errorMessage = signal('');
  tenants = signal<Tenant[]>([]);
  roles = signal<Role[]>([]);

  constructor() {
    this.registerForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      username: ['', [Validators.required, Validators.minLength(3)]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      tenantId: ['', [Validators.required]],
      roleId: ['', [Validators.required]]
    });
  }

  ngOnInit(): void {
    this.loadTenants();
  }

  loadTenants(): void {
    this.tenantService.getTenants().subscribe({
      next: (tenants) => {
        this.tenants.set(tenants);
      },
      error: (error) => {
        this.errorMessage.set('Failed to load organizations');
      }
    });
  }

  onTenantChange(): void {
    const tenantId = this.registerForm.get('tenantId')?.value;
    if (tenantId) {
      this.tenantService.getTenantRoles(tenantId).subscribe({
        next: (roles) => {
          this.roles.set(roles);
          this.registerForm.patchValue({ roleId: '' });
        },
        error: (error) => {
          this.errorMessage.set('Failed to load roles');
        }
      });
    }
  }

  onSubmit(): void {
    if (this.registerForm.valid) {
      this.isLoading.set(true);
      this.errorMessage.set('');

      this.authService.register(this.registerForm.value).subscribe({
        next: () => {
          this.router.navigate(['/auth/login']);
        },
        error: (error) => {
          this.errorMessage.set(error.message || 'Registration failed');
          this.isLoading.set(false);
        },
        complete: () => {
          this.isLoading.set(false);
        }
      });
    }
  }
}
