import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, map, Observable, of, tap } from 'rxjs';

export type UserDto = {
  id: string;
  email: string;
};

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly apiUrl = '/api/auth';

  public currentUser = signal<UserDto | null>(null);
  public isAuthenticated = computed(() => this.currentUser() !== null);

  public checkAuth(): Observable<boolean> {
    return this.http.get<UserDto>(`${this.apiUrl}/@me`).pipe(
      tap((user) => {
        this.currentUser.set(user);
      }),
      map(() => true),
      catchError((_error: HttpErrorResponse) => {
        this.currentUser.set(null);
        return of(false);
      }),
    );
  }

  public register(credentials: { email: string; password: string }): Observable<UserDto> {
    return this.http.post<UserDto>(`${this.apiUrl}/register`, credentials).pipe(
      tap((user) => {
        this.currentUser.set(user);
        this.router.navigate(['/']);
      }),
    );
  }

  public login(credentials: { email: string; password: string }): Observable<UserDto> {
    return this.http.post<UserDto>(`${this.apiUrl}/login`, credentials).pipe(
      tap((user) => {
        this.currentUser.set(user);
        this.router.navigate(['/']);
      }),
    );
  }

  public logout(): Observable<object> {
    return this.http.post(`${this.apiUrl}/logout`, {}).pipe(
      tap(() => {
        this.currentUser.set(null);
        this.router.navigate(['/auth/login']);
      }),
    );
  }
}
