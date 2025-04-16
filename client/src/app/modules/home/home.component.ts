import { Component } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { SummaryCardComponent } from '../../shared/components/summary-card/summary-card.component';

@Component({
    selector: 'app-home',
    standalone: true,
    imports: [ButtonModule, SummaryCardComponent],
    templateUrl: './home.component.html',
    styleUrl: './home.component.scss',
})
export class HomeComponent {
    user = {
        username: 'Юзер',
    };

    monthName = 'апрель';
    income = 2500;
    balance = 1000;
    expenses = 1500;
}
