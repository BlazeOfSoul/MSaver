import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
    selector: 'app-summary-table-tab',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './summary-table-tab.component.html',
    styleUrls: ['./summary-table-tab.component.scss'],
})
export class SummaryTableTabComponent {
    @Input() months: { name: string }[] = [];
    @Input() incomeTableData: Record<string, number[]> = {};
    @Input() expenseTableData: Record<string, number[]> = {};

    getCategories(): string[] {
        const all = new Set<string>([
            ...Object.keys(this.incomeTableData),
            ...Object.keys(this.expenseTableData),
        ]);
        return Array.from(all);
    }

    getMonthSum(index: number): number {
        let sum = 0;
        for (const key in this.incomeTableData) sum += this.incomeTableData[key][index] ?? 0;
        for (const key in this.expenseTableData) sum += this.expenseTableData[key][index] ?? 0;
        return sum;
    }

    getCategoryTotal(category: string): number {
        const income = this.incomeTableData[category] ?? [];
        const expense = this.expenseTableData[category] ?? [];
        const values = [...income, ...expense];
        return values.reduce((a, b) => a + b, 0);
    }

    getCategoryAverage(category: string): number {
        const income = this.incomeTableData[category] ?? [];
        const expense = this.expenseTableData[category] ?? [];
        const all = [...income, ...expense];
        const count = all.length;
        if (count === 0) return 0;
        return +(all.reduce((a, b) => a + b, 0) / count).toFixed(2);
    }

    getTotalSum(): number {
        return this.months.reduce((acc, m, i) => acc + this.getMonthSum(i), 0);
    }
}
