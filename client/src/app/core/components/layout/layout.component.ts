import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { FooterComponent } from '../footer/footer.component';
import { HeaderComponent } from '../header/header.component';

@Component({
    selector: 'app-layout',
    standalone: true,
    imports: [HeaderComponent, FooterComponent, RouterOutlet],
    templateUrl: './layout.component.html',
    styleUrl: './layout.component.scss',
})
export class LayoutComponent {}
