import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../services/auth.service'; // проверь путь

@Injectable({ providedIn: 'root' })
export class RedirectIfAuthenticatedGuard implements CanActivate {
    constructor(private authService: AuthService, private router: Router) {}

    canActivate(): boolean {
        if (this.authService.isLoggedIn()) {
            this.router.navigate(['/home']);
            return false;
        }
        return true;
    }
}
