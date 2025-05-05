import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, catchError, map, tap, throwError } from 'rxjs';
import { ApiEndpoints } from '../constants/api-endpoints';
import { LoginResponse } from '../models/auth/login-response.model';
import { RegisterRequest } from '../models/auth/register-request.model';
import { StorageService } from './storage.service';

@Injectable({
    providedIn: 'root',
})
export class AuthService {
    private readonly tokenKey = 'token';

    constructor(private http: HttpClient, private storage: StorageService) {}

    isLoggedIn(): boolean {
        return !!this.storage.getItem(this.tokenKey);
    }

    login(email: string, password: string): Observable<void> {
        return this.http.post<LoginResponse>(ApiEndpoints.Auth.Login, { email, password }).pipe(
            tap((response) => {
                this.storage.setItem(this.tokenKey, response.token);
            }),
            map(() => {}),
            catchError((error) => {
                return throwError(() => error);
            })
        );
    }

    register(data: RegisterRequest): Observable<void> {
        return this.http.post<LoginResponse>(ApiEndpoints.Auth.Register, data).pipe(
            tap((response) => {
                this.storage.setItem(this.tokenKey, response.token);
            }),
            map(() => {}),
            catchError((error) => {
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
            return null;
        }
    }

    private decodeBase64Utf8(base64: string): string {
        const binary = atob(base64);
        const bytes = Uint8Array.from(binary, (char) => char.charCodeAt(0));
        return new TextDecoder().decode(bytes);
    }
}
