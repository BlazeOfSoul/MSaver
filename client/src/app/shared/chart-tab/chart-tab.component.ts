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
import { ChartData } from '../../core/models/transaction/chart-data';

@Component({
    selector: 'app-chart-tab',
    standalone: true,
    imports: [CommonModule, DropdownModule, ChartModule, FormsModule],
    templateUrl: './chart-tab.component.html',
    styleUrls: ['./chart-tab.component.scss'],
})
export class ChartTabComponent implements OnInit, OnChanges {
    @Input() availableMonthsByYear: Record<number, number[]> = {};
    @Input() availableYears: number[] = [];
    @Input() chartDataByYearAndMonth: Record<number, (ChartData | null)[]> = {};

    @Output() selectedMonthIndexChange = new EventEmitter<number>();
    @Output() selectedYearChange = new EventEmitter<number>();

    selectedYear: number | null = null;
    selectedMonth: { name: string; index: number } | null = null;

    yearOptions: { label: string; value: number }[] = [];
    monthOptions: { name: string; index: number }[] = [];

    chartOptions = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                display: false,
            },
        },
    };

    chartDataToDisplay: {
        labels: string[];
        datasets: { label: string; data: number[]; backgroundColor: string }[];
    } | null = null;

    private monthNames = [
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

    ngOnInit() {
        this.buildYearOptions();
        this.selectInitialYearAndMonth();
    }

    ngOnChanges(changes: SimpleChanges) {
        if (changes['availableYears']) {
            this.buildYearOptions();
            if (
                this.selectedYear === null ||
                !this.yearOptions.some((o) => o.value === this.selectedYear)
            ) {
                this.selectedYear = this.yearOptions.length ? this.yearOptions[0].value : null;
                this.onYearChangedInternal();
            }
        }

        if (changes['availableMonthsByYear'] || changes['selectedYear']) {
            this.buildMonthOptions();

            if (
                this.selectedMonth &&
                !this.monthOptions.some((m) => m.index === this.selectedMonth!.index)
            ) {
                this.selectedMonth = this.monthOptions.length ? this.monthOptions[0] : null;
                this.emitSelectedMonth();
            }
        }
    }

    private buildYearOptions() {
        this.yearOptions = (this.availableYears ?? [])
            .slice()
            .sort((a, b) => b - a)
            .map((y) => ({ label: y.toString(), value: y }));
    }

    private buildMonthOptions() {
        if (this.selectedYear !== null && this.availableMonthsByYear[this.selectedYear]) {
            this.monthOptions = this.availableMonthsByYear[this.selectedYear]
                .map((idx) => ({
                    name: this.monthNames[idx],
                    index: idx,
                }))
                .sort((a, b) => a.index - b.index);
        } else {
            this.monthOptions = [];
        }
    }

    private selectInitialYearAndMonth() {
        if (this.yearOptions.length > 0) {
            this.selectedYear = this.yearOptions[0].value;
            this.onYearChangedInternal();
        }
    }

    private onYearChangedInternal() {
        this.buildMonthOptions();
        this.selectedMonth = this.monthOptions.length
            ? this.monthOptions[this.monthOptions.length - 1]
            : null;
        this.emitSelectedYear();
        this.emitSelectedMonth();
        this.updateChartData();
    }

    onYearChange(event: any) {
        this.selectedYear = event.value;
        this.onYearChangedInternal();
    }

    onMonthChange(event: any) {
        this.selectedMonth = event.value;
        this.emitSelectedMonth();
        this.updateChartData();
    }

    private emitSelectedYear() {
        if (this.selectedYear !== null) {
            this.selectedYearChange.emit(this.selectedYear);
        }
    }

    private emitSelectedMonth() {
        if (this.selectedMonth) {
            this.selectedMonthIndexChange.emit(this.selectedMonth.index);
        }
    }

    private updateChartData() {
        if (this.selectedYear === null || this.selectedMonth === null) {
            this.chartDataToDisplay = null;
            return;
        }

        const yearData = this.chartDataByYearAndMonth[this.selectedYear];
        const monthIndex = this.selectedMonth.index;

        if (!yearData || !yearData[monthIndex]) {
            this.chartDataToDisplay = { labels: [], datasets: [] };
            return;
        }

        const data = yearData[monthIndex];
        this.chartDataToDisplay = {
            labels: data.labels,
            datasets: [
                {
                    label: 'Категории',
                    data: data.data,
                    backgroundColor: '#42A5F5',
                },
            ],
        };
    }
}
