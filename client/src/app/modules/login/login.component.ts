import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ToastModule } from 'primeng/toast';
import { NotificationMessages } from '../../core/constants/notification-messages';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [InputTextModule, ButtonModule, CardModule, FormsModule, PasswordModule, ToastModule],
    templateUrl: './login.component.html',
    styleUrl: './login.component.scss',
})
export class LoginComponent {
    email = '';
    password = '';
    errorMessage = '';

    constructor(
        private authService: AuthService,
        private router: Router,
        private notificationService: NotificationService
    ) {}

    onLogin() {
        this.authService.login(this.email, this.password).subscribe({
            next: () => {
                this.router.navigate(['/home']);
            },
            error: () => {
                this.notificationService.showError(NotificationMessages.BadCredentials);
            },
        });
    }

    goToRegister() {
        this.router.navigate(['/register']);
    }
}
