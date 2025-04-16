import { HttpClient } from '@angular/common/http';
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
    email = '';
    password = '';
    confirmPassword = '';

    constructor(private router: Router, private http: HttpClient) {}

    onRegister() {
        if (this.password !== this.confirmPassword) {
            alert('Пароли не совпадают');
            return;
        }

        const user = {
            username: this.username,
            email: this.email,
            password: this.password,
        };

        this.http.post('http://localhost:5000/api/Auth/register', user).subscribe({
            next: () => {
                this.router.navigate(['/login']);
            },
            error: (err) => {
                console.error(err);
                alert('Ошибка при регистрации');
            },
        });
    }

    goToLogin() {
        this.router.navigate(['/login']);
    }
}
