import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ToastModule } from 'primeng/toast';
import { RegisterRequest } from '../../core/models/auth/register-request.model';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
    selector: 'app-register',
    standalone: true,
    imports: [InputTextModule, ButtonModule, CardModule, FormsModule, PasswordModule, ToastModule],
    templateUrl: './register.component.html',
    styleUrls: ['./register.component.scss'],
})
export class RegisterComponent {
    username = '';
    email = '';
    password = '';
    confirmPassword = '';

    constructor(
        private router: Router,
        private authService: AuthService,
        private notificationService: NotificationService
    ) {}

    onRegister() {
        if (this.password !== this.confirmPassword) {
            this.notificationService.showError('Пароли не совпадают');
            return;
        }

        const request: RegisterRequest = {
            username: this.username,
            email: this.email,
            password: this.password,
        };

        this.authService.register(request).subscribe({
            next: () => this.router.navigate(['/login']),
            error: () => this.notificationService.showError('Ошибка при регистрации'),
        });
    }

    goToLogin() {
        this.router.navigate(['/login']);
    }
}
