import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { MenubarModule } from 'primeng/menubar';
import { TabMenuModule } from 'primeng/tabmenu';
import { ToolbarModule } from 'primeng/toolbar';
import { AuthService } from '../../services/auth.service'; // проверь путь

@Component({
    selector: 'app-header',
    standalone: true,
    imports: [ToolbarModule, MenubarModule, TabMenuModule, ButtonModule],
    templateUrl: './header.component.html',
    styleUrl: './header.component.scss',
})
export class HeaderComponent {
    constructor(private router: Router, private authService: AuthService) {}

    goHome() {
        this.navigateTo('/home');
    }

    goCategories() {
        this.navigateTo('/categories');
    }

    goStatistics() {
        this.navigateTo('/statistics');
    }

    navigateTo(link: string) {
        this.router.navigate([link]);
    }

    logout() {
        this.authService.logout();
        this.router.navigate(['/login']);
    }

    isLoggedIn(): boolean {
        return this.authService.isLoggedIn();
    }
}
