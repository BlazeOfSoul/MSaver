import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root',
})
export class AuthService {
    isLoggedIn(): boolean {
        return !!localStorage.getItem('token');
    }

    login(username: string, password: string): boolean {
        if (username && password) {
            localStorage.setItem('token', 'fake-token');
            return true;
        }
        return false;
    }

    logout(): void {
        localStorage.removeItem('token');
    }
}
