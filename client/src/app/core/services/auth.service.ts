import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, catchError, map, tap, throwError } from 'rxjs';
import { LoginResponse } from '../models/login.model';

@Injectable({
    providedIn: 'root',
})
export class AuthService {
    private apiUrl = 'http://localhost:5000';

    constructor(private http: HttpClient) {}

    isLoggedIn(): boolean {
        if (typeof window !== 'undefined' && window.localStorage) {
            return !!localStorage.getItem('token');
        }
        return false;
    }

    login(email: string, password: string): Observable<void> {
        return this.http
            .post<LoginResponse>(`${this.apiUrl}/api/auth/login`, { email, password })
            .pipe(
                tap((response) => {
                    localStorage.setItem('token', response.token);
                }),
                map(() => {}), // Возвращаем void
                catchError((error) => {
                    console.error('Login failed', error);
                    return throwError(() => error);
                })
            );
    }

    logout(): void {
        localStorage.removeItem('token');
    }
}
