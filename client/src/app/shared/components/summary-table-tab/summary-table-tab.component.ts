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
    @Input() months: { name: string }[] = [];
    @Input() incomeTableData: Record<string, number[]> = {};
    @Input() expenseTableData: Record<string, number[]> = {};

    selectedType: 'income' | 'expense' = 'income';

    get typeOptions() {
        return [
            { label: 'Доходы', value: 'income' },
            { label: 'Расходы', value: 'expense' },
        ];
    }

    getCategories(): string[] {
        const data = this.selectedType === 'income' ? this.incomeTableData : this.expenseTableData;
        return Object.keys(data);
    }

    getMonthSum(index: number): number {
        const data = this.selectedType === 'income' ? this.incomeTableData : this.expenseTableData;
        let sum = 0;
        for (const key in data) sum += data[key][index] ?? 0;
        return sum;
    }

    getCategoryTotal(category: string): number {
        const data = this.selectedType === 'income' ? this.incomeTableData : this.expenseTableData;
        const values = data[category] ?? [];
        return values.reduce((a, b) => a + b, 0);
    }

    getCategoryAverage(category: string): number {
        const data = this.selectedType === 'income' ? this.incomeTableData : this.expenseTableData;
        const all = data[category] ?? [];
        const count = all.length;
        if (count === 0) return 0;
        return +(all.reduce((a, b) => a + b, 0) / count).toFixed(2);
    }

    getTotalSum(): number {
        return this.months.reduce((acc, _, i) => acc + this.getMonthSum(i), 0);
    }

    getTotalAverage(): number {
        const total = this.getTotalSum();
        const count = this.months.length;
        return count === 0 ? 0 : +(total / count).toFixed(2);
    }
}
