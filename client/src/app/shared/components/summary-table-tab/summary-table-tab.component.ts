import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DropdownModule } from 'primeng/dropdown';
import { TableModule } from 'primeng/table';

@Component({
    selector: 'app-summary-table-tab',
    standalone: true,
    imports: [CommonModule, TableModule, DropdownModule, FormsModule],
    templateUrl: './summary-table-tab.component.html',
    styleUrls: ['./summary-table-tab.component.scss'],
})
export class SummaryTableTabComponent {
    @Input() months: { name: string; index: number }[] = [];
    @Input() years: number[] = [];
    @Input() incomeData: Record<string, number[]> = {};
    @Input() expenseData: Record<string, number[]> = {};
    @Input() selectedYear!: number;

    selectedType: 'income' | 'expense' = 'income';

    get typeOptions() {
        return [
            { label: 'Доходы', value: 'income' },
            { label: 'Расходы', value: 'expense' },
        ];
    }

    get currentDataSnapshot(): Record<string, number[]> {
        return this.selectedType === 'income' ? this.incomeData : this.expenseData;
    }

    getCategories(): string[] {
        return Object.keys(this.currentDataSnapshot);
    }

    getMonthSum(index: number): number {
        let sum = 0;
        for (const category of this.getCategories()) {
            sum += this.currentDataSnapshot[category]?.[index] ?? 0;
        }
        return sum;
    }

    getCategoryTotal(category: string): number {
        const values = this.currentDataSnapshot[category] ?? [];
        return values.reduce((a, b) => a + b, 0);
    }

    getCategoryAverage(category: string): number {
        const values = this.currentDataSnapshot[category] ?? [];
        if (values.length === 0) return 0;
        return +(values.reduce((a, b) => a + b, 0) / values.length).toFixed(2);
    }

    getTotalSum(): number {
        return this.months.reduce((acc, _, i) => acc + this.getMonthSum(i), 0);
    }

    getTotalAverage(): number {
        const total = this.getTotalSum();
        const count = this.months.length;
        return count === 0 ? 0 : +(total / count).toFixed(2);
    }

    getFormattedValue(category: string, index: number): string {
        const value = this.currentDataSnapshot[category]?.[index];
        return value !== undefined && value !== null
            ? new Intl.NumberFormat('ru-RU', {
                  style: 'currency',
                  currency: 'BYN',
                  minimumFractionDigits: 2,
              }).format(value)
            : '-';
    }

    getFormattedNumber(value: number): string {
        return new Intl.NumberFormat('ru-RU', {
            style: 'currency',
            currency: 'BYN',
            minimumFractionDigits: 2,
        }).format(value);
    }

    hasDataForSelectedYear(): boolean {
        const data = this.currentDataSnapshot;
        return Object.values(data).some((arr) =>
            arr.some((val) => val !== undefined && val !== null)
        );
    }
}
