import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';

@Component({
    selector: 'app-register',
    standalone: true,
    imports: [InputTextModule, ButtonModule, CardModule, FormsModule, PasswordModule],
    templateUrl: './register.component.html',
    styleUrls: ['./register.component.scss'],
})
export class RegisterComponent {
    username = '';
    password = '';
    confirmPassword = '';

    constructor(private router: Router) {}

    onRegister() {
        if (this.password !== this.confirmPassword) {
            alert('Пароли не совпадают');
            return;
        }

        const success = true;
        // const success = this.authService.register(this.username, this.password);
        if (success) {
            this.router.navigate(['/login']);
        } else {
            alert('Ошибка регистрации');
        }
    }

    goToLogin() {
        this.router.navigate(['/login']);
    }
}
