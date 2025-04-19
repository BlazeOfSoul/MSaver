import { Component } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { Rate } from '../../core/models/exchange-rate/rate.model';
import { AuthService } from '../../core/services/auth.service';
import { ExchangeRateService } from '../../core/services/exchange-rate.service';
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

    income = 2500;
    balance = 1000;
    expenses = 1500;

    fiatRates: Rate[] = [];
    cryptoRates: Rate[] = [];

    constructor(
        private authService: AuthService,
        private exchangeRateService: ExchangeRateService
    ) {}

    ngOnInit() {
        const currentUsername = this.authService.getUsernameFromToken();
        if (currentUsername) {
            this.user.username = currentUsername;
        }

        this.exchangeRateService.getRates().subscribe({
            next: (data) => {
                this.fiatRates = data.fiat;
                this.cryptoRates = data.crypto;
            },
            error: (err) => {
                console.error('Ошибка при загрузке курсов:', err);
            },
        });
    }
}
