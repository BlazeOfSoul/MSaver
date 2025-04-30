import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, catchError, map, tap, throwError } from 'rxjs';
import { environment } from '../../../environment';
import { LoginResponse } from '../models/auth/login-response.model';
import { RegisterRequest } from '../models/auth/register-request.model';
import { StorageService } from './storage.service';

@Injectable({
    providedIn: 'root',
})
export class AuthService {
    private apiUrl = environment.apiUrl;
    private readonly tokenKey = 'token';

    constructor(private http: HttpClient, private storage: StorageService) {}

    isLoggedIn(): boolean {
        return !!this.storage.getItem(this.tokenKey);
    }

    login(email: string, password: string): Observable<void> {
        return this.http.post<LoginResponse>(`${this.apiUrl}/auth/login`, { email, password }).pipe(
            tap((response) => {
                this.storage.setItem(this.tokenKey, response.token);
            }),
            map(() => {}),
            catchError((error) => {
                console.error('Login failed', error);
                return throwError(() => error);
            })
        );
    }

    register(data: RegisterRequest): Observable<void> {
        return this.http.post<LoginResponse>(`${this.apiUrl}/auth/register`, data).pipe(
            tap((response) => {
                this.storage.setItem(this.tokenKey, response.token);
            }),
            map(() => {}),
            catchError((error) => {
                console.error('Registration failed', error);
                return throwError(() => error);
            })
        );
    }

    logout(): void {
        this.storage.removeItem(this.tokenKey);
    }

    getToken(): string | null {
        return this.storage.getItem(this.tokenKey);
    }

    getUsernameFromToken(): string | null {
        const token = this.getToken();
        if (!token) return null;

        try {
            const payload = token.split('.')[1];
            const decodedPayload = this.decodeBase64Utf8(payload);
            const tokenData = JSON.parse(decodedPayload);
            return tokenData['unique_name'] || null;
        } catch (e) {
            console.error('Ошибка при декодировании токена', e);
            return null;
        }
    }

    private decodeBase64Utf8(base64: string): string {
        const binary = atob(base64);
        const bytes = Uint8Array.from(binary, (char) => char.charCodeAt(0));
        return new TextDecoder().decode(bytes);
    }
}
