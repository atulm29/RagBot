import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Tenant, Role } from '../models/tenant.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class TenantService {
  private http = inject(HttpClient);

  getTenants(): Observable<Tenant[]> {
    return this.http.get<Tenant[]>(`${environment.apiUrl}/tenant`);
  }

  getTenant(id: string): Observable<Tenant> {
    return this.http.get<Tenant>(`${environment.apiUrl}/tenant/${id}`);
  }

  getTenantRoles(tenantId: string): Observable<Role[]> {
    return this.http.get<Role[]>(`${environment.apiUrl}/tenant/${tenantId}/roles`);
  }
}
