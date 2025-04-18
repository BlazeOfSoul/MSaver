import { Component } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { AuthService } from '../../core/services/auth.service';
import { SummaryCardComponent } from '../../shared/components/summary-card/summary-card.component';

@Component({
    selector: 'app-home',
    standalone: true,
    imports: [ButtonModule, SummaryCardComponent, TableModule],
    templateUrl: './home.component.html',
    styleUrl: './home.component.scss',
})
export class HomeComponent {
    user = {
        username: '',
    };

    monthName = 'апрель';
    income = 2500;
    balance = 1000;
    expenses = 1500;

    fiatRates = [
        { currency: 'USD', rate: 3.0814 },
        { currency: 'EUR', rate: 3.505 },
        { currency: 'RUB (за 100)', rate: 3.6695 },
    ];

    cryptoRates = [
        { currency: 'Bitcoin (BTC)', rate: 84535 },
        { currency: 'Ethereum (ETH)', rate: 1578.76 },
        { currency: 'Solana (SOL)', rate: 131.45 },
    ];

    constructor(private authService: AuthService) {}

    ngOnInit() {
        const currentUsername = this.authService.getUsernameFromToken();
        if (currentUsername) {
            this.user.username = currentUsername;
        }
    }
}
