import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ToastModule } from 'primeng/toast';
import { AuthService } from '../../core/services/auth.service';

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
        private messageService: MessageService
    ) {}

    onLogin() {
        this.errorMessage = '';
        this.authService.login(this.email, this.password).subscribe({
            next: () => {
                this.router.navigate(['/home']);
            },
            error: () => {
                this.showErrorWithContent('Неверный email или пароль');
            },
        });
    }

    goToRegister() {
        this.router.navigate(['/register']);
    }

    // TODO: Switch to one method (register same)
    showErrorWithContent(messageContent: string) {
        this.messageService.add({
            severity: 'error',
            summary: 'Что-то пошло не так...',
            detail: messageContent,
        });
    }
}
