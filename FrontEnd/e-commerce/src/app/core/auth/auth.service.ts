import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, of } from 'rxjs';
import type { KeycloakTokenResponse } from '../api/api-types';

const STORAGE_KEY = 'auth_tokens';
const API_BASE = 'https://localhost:5001';

export interface StoredTokens {
  access_token: string;
  refresh_token?: string;
  expires_at: number;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly accessToken = signal<string | null>(this.loadStoredAccessToken());
  private readonly refreshToken = signal<string | null>(this.loadStoredRefreshToken());

  readonly isAuthenticated = computed(() => !!this.accessToken());

  getAccessToken(): string | null {
    return this.accessToken();
  }

  /** JWT'den roller (realm_access.roles veya role). Admin paneli için kullanılır. */
  hasRole(role: string): boolean {
    const token = this.accessToken();
    if (!token) return false;
    try {
      const payload = token.split('.')[1];
      if (!payload) return false;
      const json = JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/')));
      const realmRoles = json.realm_access?.roles as string[] | undefined;
      if (Array.isArray(realmRoles) && realmRoles.includes(role)) return true;
      const r = json.role as string | undefined;
      return r === role;
    } catch {
      return false;
    }
  }

  isAdmin(): boolean {
    return this.hasRole('Admin');
  }

  private loadStoredAccessToken(): string | null {
    try {
      const raw = sessionStorage.getItem(STORAGE_KEY);
      if (!raw) return null;
      const data = JSON.parse(raw) as StoredTokens;
      if (data.expires_at && Date.now() >= data.expires_at) {
        sessionStorage.removeItem(STORAGE_KEY);
        return null;
      }
      return data.access_token ?? null;
    } catch {
      return null;
    }
  }

  private loadStoredRefreshToken(): string | null {
    try {
      const raw = sessionStorage.getItem(STORAGE_KEY);
      if (!raw) return null;
      const data = JSON.parse(raw) as StoredTokens;
      return data.refresh_token ?? null;
    } catch {
      return null;
    }
  }

  private storeTokens(res: KeycloakTokenResponse): void {
    const expiresAt = res.expires_in
      ? Date.now() + res.expires_in * 1000
      : Date.now() + 5 * 60 * 1000;
    const stored: StoredTokens = {
      access_token: res.access_token,
      refresh_token: res.refresh_token ?? undefined,
      expires_at: expiresAt,
    };
    sessionStorage.setItem(STORAGE_KEY, JSON.stringify(stored));
    this.accessToken.set(res.access_token);
    this.refreshToken.set(res.refresh_token ?? null);
  }

  login(username: string, password: string): Observable<KeycloakTokenResponse | null> {
    return this.http
      .post<KeycloakTokenResponse>(`${API_BASE}/api/auth/login`, { username, password })
      .pipe(
        tap((res) => this.storeTokens(res)),
        catchError(() => of(null))
      );
  }

  refresh(): Observable<KeycloakTokenResponse | null> {
    const refresh = this.refreshToken();
    if (!refresh) return of(null);
    return this.http
      .post<KeycloakTokenResponse>(`${API_BASE}/api/auth/refresh`, { refreshToken: refresh })
      .pipe(
        tap((res) => res && this.storeTokens(res)),
        catchError(() => {
          this.logout();
          return of(null);
        })
      );
  }

  logout(): void {
    sessionStorage.removeItem(STORAGE_KEY);
    this.accessToken.set(null);
    this.refreshToken.set(null);
  }

  register(payload: RegisterPayload): Observable<{ message: string; userId: string } | null> {
    return this.http
      .post<{ message: string; userId: string }>(`${API_BASE}/api/auth/register`, payload)
      .pipe(catchError(() => of(null)));
  }
}

export interface RegisterPayload {
  username: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  address?: string;
  cityId?: number;
  districtId?: number;
  cardLast4?: string;
}
