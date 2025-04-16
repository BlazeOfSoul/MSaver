import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, catchError, map, tap, throwError } from 'rxjs';
import { environment } from '../../../environment';
import { LoginResponse } from '../models/login.model';
import { RegisterRequest } from '../models/register.model';

@Injectable({
    providedIn: 'root',
})
export class AuthService {
    private apiUrl = environment.apiUrl;

    constructor(private http: HttpClient) {}

    isLoggedIn(): boolean {
        return typeof window !== 'undefined' && !!localStorage.getItem('token');
    }

    login(email: string, password: string): Observable<void> {
        return this.http.post<LoginResponse>(`${this.apiUrl}/auth/login`, { email, password }).pipe(
            tap((response) => {
                localStorage.setItem('token', response.token);
            }),
            map(() => {}),
            catchError((error) => {
                console.error('Login failed', error);
                return throwError(() => error);
            })
        );
    }

    register(data: RegisterRequest): Observable<void> {
        return this.http
            .post<LoginResponse>(`${this.apiUrl}/auth/register`, data)
            .pipe(map(() => {}));
    }

    logout(): void {
        localStorage.removeItem('token');
    }
}
