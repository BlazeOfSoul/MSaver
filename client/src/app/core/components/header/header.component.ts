import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { MenubarModule } from 'primeng/menubar';
import { TabMenuModule } from 'primeng/tabmenu';
import { ToolbarModule } from 'primeng/toolbar';

@Component({
    selector: 'app-header',
    standalone: true,
    imports: [ToolbarModule, MenubarModule, TabMenuModule],
    templateUrl: './header.component.html',
    styleUrl: './header.component.scss',
})
export class HeaderComponent {
    constructor(private router: Router) {}

    items: MenuItem[] | undefined;

    ngOnInit() {
        this.items = [
            {
                label: 'Главная',
                icon: 'pi pi-home',
                routerLink: '/home',
            },
        ];
    }

    goHome() {
        this.navigateTo('/home');
    }

    navigateTo(link: string) {
        this.router.navigate([link]);
    }
}
