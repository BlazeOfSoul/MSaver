import { Routes } from '@angular/router';
import { LayoutComponent } from './core/components/layout/layout.component';
import { AuthGuard } from './core/guards/auth.guard';
import { RedirectIfAuthenticatedGuard } from './core/guards/redirect-if-authenticated.guard';
import { CategoriesComponent } from './modules/categories/categories.component';
import { HomeComponent } from './modules/home/home.component';
import { LoginComponent } from './modules/login/login.component';
import { RegisterComponent } from './modules/register/register.component';
import { StatisticsComponent } from './modules/statistics/statistics.component';

export const routes: Routes = [
    { path: 'login', component: LoginComponent, canActivate: [RedirectIfAuthenticatedGuard] },
    { path: 'register', component: RegisterComponent, canActivate: [RedirectIfAuthenticatedGuard] },
    {
        path: '',
        component: LayoutComponent,
        canActivate: [AuthGuard],
        children: [
            { path: '', pathMatch: 'full', redirectTo: 'home' },
            { path: 'home', component: HomeComponent },
            { path: 'categories', component: CategoriesComponent },
            { path: 'statistics', component: StatisticsComponent },
        ],
    },
    {
        path: '**',
        redirectTo: '',
        pathMatch: 'full',
    },
];
