import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ChartModule } from 'primeng/chart';
import { DropdownModule } from 'primeng/dropdown';

@Component({
    selector: 'app-chart-tab',
    standalone: true,
    imports: [CommonModule, DropdownModule, ChartModule, FormsModule],
    templateUrl: './chart-tab.component.html',
    styleUrl: './chart-tab.component.scss',
})
export class ChartTabComponent {
    @Input() months: { name: string; index: number }[] = [];
    @Input() chartDataByMonth: any[] = [];

    selectedMonth: { name: string; index: number } | null = null;

    @Output() selectedMonthIndexChange = new EventEmitter<number>();

    chartOptions = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                display: false,
            },
        },
    };

    ngOnInit() {
        if (this.months.length > 0) {
            this.selectedMonth = this.months[0];
            this.emitSelectedMonth();
        }
    }

    get currentChartData() {
        if (!this.selectedMonth) return null;
        return this.chartDataByMonth[this.selectedMonth.index];
    }

    onMonthChange(event: any) {
        this.selectedMonth = event.value;
        this.emitSelectedMonth();
    }

    private emitSelectedMonth() {
        if (this.selectedMonth) {
            this.selectedMonthIndexChange.emit(this.selectedMonth.index);
        }
    }
}
