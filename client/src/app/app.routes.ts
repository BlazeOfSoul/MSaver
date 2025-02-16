import { Routes } from '@angular/router';
import { LayoutComponent } from './core/components/layout/layout.component';
import { HomeComponent } from './modules/home/home.component';

export const routes: Routes = [
    {
        path: '',
        component: LayoutComponent,
        children: [
            { path: '', pathMatch: 'full', redirectTo: 'home' },
            { path: 'home', component: HomeComponent },
            // { path: 'error', component: ErrorViewComponent },
        ],
    },
];
