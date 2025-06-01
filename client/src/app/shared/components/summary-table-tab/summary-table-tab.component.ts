import { CommonModule } from '@angular/common';
import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
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
export class SummaryTableTabComponent implements OnInit, OnChanges {
    @Input() dataByYear: Record<number, Record<string, Record<number, number>>> = {};

    @Input() availableYears: number[] = [];

    @Input() availableMonthsByYear: Record<number, number[]> = {};

    selectedYear!: number;

    yearOptions: { label: string; value: number }[] = [];

    ngOnInit(): void {
        this.buildYearOptions();
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['availableYears'] && !changes['availableYears'].firstChange) {
            this.buildYearOptions();
        }
    }

    private buildYearOptions(): void {
        this.yearOptions = (this.availableYears ?? [])
            .slice()
            .sort((a, b) => b - a) // по убыванию
            .map((y) => ({ label: y.toString(), value: y }));

        const currentYear = new Date().getFullYear();
        this.selectedYear = this.yearOptions.some((o) => o.value === currentYear)
            ? currentYear
            : this.yearOptions.length
            ? this.yearOptions[0].value
            : currentYear;
    }

    get months(): { name: string; index: number }[] {
        const names = [
            'Январь',
            'Февраль',
            'Март',
            'Апрель',
            'Май',
            'Июнь',
            'Июль',
            'Август',
            'Сентябрь',
            'Октябрь',
            'Ноябрь',
            'Декабрь',
        ];
        const idx = this.availableMonthsByYear[this.selectedYear] ?? [];
        return idx.map((i) => ({ name: names[i], index: i }));
    }

    get data(): Record<string, Record<number, number>> {
        return this.dataByYear[this.selectedYear] ?? {};
    }

    getCategories(): string[] {
        return Object.keys(this.data);
    }

    getMonthSum(monthIdx: number): number {
        return this.getCategories().reduce(
            (sum, cat) => sum + (this.data[cat]?.[monthIdx] ?? 0),
            0
        );
    }

    getCategoryTotal(cat: string): number {
        return Object.values(this.data[cat] ?? {}).reduce((a, b) => a + b, 0);
    }

    getCategoryAverage(cat: string): number {
        const categoryData = this.data[cat] ?? {};
        const months = this.availableMonthsByYear[this.selectedYear] ?? [];
        const vals = months.map((monthIdx) => categoryData[monthIdx] ?? 0);
        const average = vals.reduce((a, b) => a + b, 0) / (vals.length || 1);
        return +average.toFixed(2);
    }

    getTotalSum(): number {
        return this.months.reduce((acc, m) => acc + this.getMonthSum(m.index), 0);
    }

    getTotalAverage(): number {
        const total = this.getTotalSum();
        const count = this.months.length;
        return total / count;
    }

    private formatCurrency(value: number): string {
        return new Intl.NumberFormat('ru-RU', {
            style: 'currency',
            currency: 'BYN',
            minimumFractionDigits: 2,
        }).format(value);
    }

    getFormattedValue(cat: string, monthIdx: number): string {
        const val = this.data[cat]?.[monthIdx];
        return val != null ? this.formatCurrency(val) : '-';
    }

    getFormattedNumber(val: number): string {
        return this.formatCurrency(val);
    }

    hasData(): boolean {
        return Object.values(this.data).some((monthMap) =>
            this.months.some((m) => {
                const v = monthMap[m.index];
                return v != null && v !== 0;
            })
        );
    }
}
