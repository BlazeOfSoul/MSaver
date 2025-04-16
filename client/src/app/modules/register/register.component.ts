import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ToastModule } from 'primeng/toast';

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
        private http: HttpClient,
        private messageService: MessageService
    ) {}

    onRegister() {
        if (this.password !== this.confirmPassword) {
            this.showErrorWithContent('Пароли не совпадают');
            return;
        }

        const user = {
            username: this.username,
            email: this.email,
            password: this.password,
        };
        //TODO: Take out
        this.http.post('http://localhost:5000/api/Auth/register', user).subscribe({
            next: () => {
                this.router.navigate(['/login']);
            },
            error: () => {
                this.showErrorWithContent('Ошибка при регистрации');
            },
        });
    }

    goToLogin() {
        this.router.navigate(['/login']);
    }

    showErrorWithContent(messageContent: string) {
        this.messageService.add({
            severity: 'error',
            summary: 'Что-то пошло не так...',
            detail: messageContent,
        });
    }
}
