import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { DropdownModule } from 'primeng/dropdown';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule } from 'primeng/table';
import { Category } from '../../core/models/balance/category.model';
import { Rate } from '../../core/models/exchange-rate/rate.model';
import { AuthService } from '../../core/services/auth.service';
import { BalanceService } from '../../core/services/balance.service';
import { ExchangeRateService } from '../../core/services/exchange-rate.service';
import { SummaryCardComponent } from '../../shared/components/summary-card/summary-card.component';
@Component({
    selector: 'app-home',
    standalone: true,
    imports: [
        ButtonModule,
        TableModule,
        DialogModule,
        DropdownModule,
        InputTextModule,
        InputNumberModule,
        FormsModule,
        SummaryCardComponent,
    ],
    templateUrl: './home.component.html',
    styleUrl: './home.component.scss',
})
export class HomeComponent {
    user = { username: '' };
    income = 0;
    balance = 0;
    expenses = 0;

    fiatRates: Rate[] = [];
    cryptoRates: Rate[] = [];

    categories: Category[] = [];
    selectedCategory: Category | null = null;
    transactionDescription = '';
    transactionAmount: number | null = null;

    showTransactionDialog = false;

    constructor(
        private authService: AuthService,
        private exchangeRateService: ExchangeRateService,
        private balanceService: BalanceService
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

        this.balanceService.getCurrentBalance().subscribe({
            next: ({ incomeTotal, expenseTotal, balance }) => {
                this.income = incomeTotal;
                this.expenses = expenseTotal;
                this.balance = balance;
            },
            error: (err) => {
                console.error('Ошибка при получении баланса:', err);
            },
        });
    }

    openTransactionDialog() {
        this.showTransactionDialog = true;
    }

    submitTransaction() {
        if (!this.selectedCategory || !this.transactionAmount) return;

        // Тут будет запрос на бэкенд
        console.log('Добавляем транзакцию:', {
            category: this.selectedCategory,
            description: this.transactionDescription,
            amount: this.transactionAmount,
        });

        this.showTransactionDialog = false;

        // Очистка
        this.transactionDescription = '';
        this.transactionAmount = null;
        this.selectedCategory = null;
    }
}
