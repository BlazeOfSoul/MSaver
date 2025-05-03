import { Component } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { NotificationMessages } from '../../core/constants/notification-messages';
import { Category } from '../../core/models/balance/category.model';
import { Rate } from '../../core/models/exchange-rate/rate.model';
import { AuthService } from '../../core/services/auth.service';
import { BalanceService } from '../../core/services/balance.service';
import { CategoryService } from '../../core/services/category.service';
import { ExchangeRateService } from '../../core/services/exchange-rate.service';
import { NotificationService } from '../../core/services/notification.service';
import { SummaryCardComponent } from '../../shared/components/summary-card/summary-card.component';
import { TransactionDialogComponent } from '../../shared/components/transaction-dialog/transaction-dialog.component';
@Component({
    selector: 'app-home',
    standalone: true,
    imports: [ButtonModule, TableModule, SummaryCardComponent, TransactionDialogComponent],
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

    isTransactionDialogVisible: boolean = false;

    constructor(
        private authService: AuthService,
        private exchangeRateService: ExchangeRateService,
        private balanceService: BalanceService,
        private categoryService: CategoryService,
        private notificationService: NotificationService
    ) {}

    ngOnInit() {
        const currentUsername = this.authService.getUsernameFromToken();
        if (currentUsername) {
            this.user.username = currentUsername;
        }

        this.balanceService.getCurrentBalance().subscribe({
            next: ({ incomeTotal, expenseTotal, balance }) => {
                this.income = incomeTotal;
                this.expenses = expenseTotal;
                this.balance = balance;
            },
            error: () => {
                this.notificationService.showError(NotificationMessages.LoadBalanceError);
            },
        });

        this.exchangeRateService.getRates().subscribe({
            next: (data) => {
                this.fiatRates = data.fiat;
                this.cryptoRates = data.crypto;
            },
            error: () => {
                this.notificationService.showError(NotificationMessages.LoadRatesError);
            },
        });

        this.categoryService.getUserCategories().subscribe({
            next: (data) => {
                this.categories = data;
            },
            error: () => {
                this.notificationService.showError(NotificationMessages.LoadCategoriesError);
            },
        });
    }

    openTransactionDialog(): void {
        this.isTransactionDialogVisible = true;
    }

    handleTransactionDialogClose(): void {
        this.isTransactionDialogVisible = false;
    }

    handleTransactionSubmit(): void {
        this.isTransactionDialogVisible = false;

        this.balanceService.getCurrentBalance().subscribe({
            next: ({ incomeTotal, expenseTotal, balance }) => {
                this.income = incomeTotal;
                this.expenses = expenseTotal;
                this.balance = balance;
            },
            error: () => {
                this.notificationService.showError(NotificationMessages.LoadBalanceError);
            },
        });
    }
}
