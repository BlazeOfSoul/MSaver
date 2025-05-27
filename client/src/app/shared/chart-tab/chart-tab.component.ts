import { CommonModule } from '@angular/common';
import {
    Component,
    EventEmitter,
    Input,
    OnChanges,
    OnInit,
    Output,
    SimpleChanges,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ChartModule } from 'primeng/chart';
import { DropdownModule } from 'primeng/dropdown';

@Component({
    selector: 'app-chart-tab',
    standalone: true,
    imports: [CommonModule, DropdownModule, ChartModule, FormsModule],
    templateUrl: './chart-tab.component.html',
    styleUrls: ['./chart-tab.component.scss'],
})
export class ChartTabComponent implements OnInit, OnChanges {
    @Input() months: { name: string; index: number }[] = [];
    @Input() years: number[] = [];
    @Input() chartDataByMonthAndYear: { [year: number]: any[] } = {};

    @Input() data: any;

    @Output() selectedMonthIndexChange = new EventEmitter<number>();
    @Output() selectedYearChange = new EventEmitter<number>();

    selectedMonth: { name: string; index: number } | null = null;
    selectedYear: number | null = null;

    // кеш для данных графика
    private _cachedChartData: any = null;
    private _prevInputData: any = null;

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
        if (this.years.length > 0) {
            this.selectedYear = this.years[this.years.length - 1];
            this.emitSelectedYear();
        }

        if (this.months.length > 0) {
            this.selectedMonth = this.months[this.months.length - 1];
            this.emitSelectedMonth();
        }
    }

    ngOnChanges(changes: SimpleChanges) {
        if (this.selectedMonth && !this.months.some((m) => m.index === this.selectedMonth!.index)) {
            this.selectedMonth = this.months[this.months.length - 1] ?? null;
            this.emitSelectedMonth();
        }

        if (changes['data']) {
            if (this.data !== this._prevInputData) {
                this._prevInputData = this.data;
                this._cachedChartData = this.createChartData(this.data);
            }
        }
    }

    get currentChartData() {
        return this._cachedChartData;
    }

    private createChartData(data: any) {
        if (!data) return null;

        return {
            labels: data.labels,
            datasets: [
                {
                    label: 'Сумма',
                    backgroundColor: '#42A5F5',
                    data: data.data,
                },
            ],
        };
    }

    onMonthChange(event: any) {
        this.selectedMonth = event.value;
        this.emitSelectedMonth();
    }

    onYearChange(event: any) {
        this.selectedYear = event.value;
        this.emitSelectedYear();
    }

    private emitSelectedMonth() {
        if (this.selectedMonth) {
            this.selectedMonthIndexChange.emit(this.selectedMonth.index);
        }
    }

    private emitSelectedYear() {
        if (this.selectedYear !== null) {
            this.selectedYearChange.emit(this.selectedYear);
        }
    }
}
