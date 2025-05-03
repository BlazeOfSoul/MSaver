import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CalendarModule } from 'primeng/calendar';
import { DialogModule } from 'primeng/dialog';
import { DropdownModule } from 'primeng/dropdown';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { NotificationMessages } from '../../../core/constants/notification-messages';
import { CategoryType } from '../../../core/enums/transaction-type.enum';
import { Category } from '../../../core/models/balance/category.model';
import { Transaction } from '../../../core/models/balance/transaction';
import { NotificationService } from '../../../core/services/notification.service';
import { TransactionService } from '../../../core/services/transaction.service';

@Component({
    selector: 'app-transaction-dialog',
    standalone: true,
    imports: [
        DialogModule,
        CalendarModule,
        DropdownModule,
        InputNumberModule,
        InputTextModule,
        FormsModule,
    ],
    templateUrl: './transaction-dialog.component.html',
    styleUrl: './transaction-dialog.component.scss',
})
export class TransactionDialogComponent {
    @Input() set visible(value: boolean) {
        this._visible = value;
    }

    get visible(): boolean {
        return this._visible;
    }

    @Output() visibleChange = new EventEmitter<boolean>();

    private _visible = false;

    @Input() categories: Category[] = [];
    @Output() close = new EventEmitter<void>();
    @Output() submit = new EventEmitter<Transaction>();

    constructor(
        private transactionService: TransactionService,
        private notificationService: NotificationService
    ) {}

    categoryType: CategoryType | null = null;
    categoryTypes = [
        { label: 'Доход', value: CategoryType.Income },
        { label: 'Расход', value: CategoryType.Expense },
    ];

    selectedCategory: Category | null = null;
    amount: number | null = null;
    description = '';
    date: Date = new Date();

    showTransactionDialog = false;

    get filteredCategories(): Category[] {
        return this.categoryType !== null
            ? this.categories.filter((c) => c.type === this.categoryType)
            : [];
    }

    submitTransaction() {
        if (!this.selectedCategory || !this.amount) {
            return;
        }

        const transaction = {
            categoryId: this.selectedCategory.id,
            amount: this.amount,
            description: this.description,
            date: this.date,
        };

        this.transactionService.addTransaction(transaction).subscribe({
            next: () => {
                this.resetForm();
                this.onClose();
                this.notificationService.showSuccess(NotificationMessages.TransactionSuccess);

                this.submit.emit();
            },
            error: () => {
                this.notificationService.showError(NotificationMessages.TransactionUnsuccess);
            },
        });
    }

    resetForm(): void {
        this.categoryType = null;
        this.selectedCategory = null;
        this.amount = null;
        this.description = '';
        this.date = new Date();
    }

    onClose(): void {
        this._visible = false;
        this.visibleChange.emit(false);
        this.close.emit();
    }
}
