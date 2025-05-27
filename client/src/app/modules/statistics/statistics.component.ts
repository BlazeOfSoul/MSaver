import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { TabViewModule } from 'primeng/tabview';
import { StatisticsResponse } from '../../core/models/transaction/statistics-response.model';
import { TransactionService } from '../../core/services/transaction.service';
import { ChartTabComponent } from '../../shared/chart-tab/chart-tab.component';
import { SummaryTableTabComponent } from '../../shared/components/summary-table-tab/summary-table-tab.component';

@Component({
    selector: 'app-statistics',
    standalone: true,
    imports: [CommonModule, ChartTabComponent, SummaryTableTabComponent, TabViewModule],
    templateUrl: './statistics.component.html',
    styleUrls: ['./statistics.component.scss'],
})
export class StatisticsComponent implements OnInit {
    tabs = ['Доходы', 'Расходы', 'Таблица'];
    activeTabIndex = 0;

    months = [
        { name: 'Январь', index: 0 },
        { name: 'Февраль', index: 1 },
        { name: 'Март', index: 2 },
        { name: 'Апрель', index: 3 },
        { name: 'Май', index: 4 },
        { name: 'Июнь', index: 5 },
        { name: 'Июль', index: 6 },
        { name: 'Август', index: 7 },
        { name: 'Сентябрь', index: 8 },
        { name: 'Октябрь', index: 9 },
        { name: 'Ноябрь', index: 10 },
        { name: 'Декабрь', index: 11 },
    ];

    selectedMonthIndex = new Date().getMonth();
    selectedYear = new Date().getFullYear();

    statisticsData: StatisticsResponse | null = null;

    constructor(private transactionService: TransactionService) {}

    ngOnInit(): void {
        this.transactionService.getStatistics().subscribe((data) => {
            this.statisticsData = data;
        });
    }

    get incomeChartDataForSelected(): any {
        if (!this.statisticsData) return null;
        return (
            this.statisticsData.incomeChartDataByYear[this.selectedYear]?.[
                this.selectedMonthIndex
            ] ?? null
        );
    }

    get expenseChartDataForSelected(): any {
        if (!this.statisticsData) return null;
        return (
            this.statisticsData.expenseChartDataByYear[this.selectedYear]?.[
                this.selectedMonthIndex
            ] ?? null
        );
    }

    get incomeTableDataForSelected(): Record<string, number[]> {
        return this.statisticsData?.incomeTableData[this.selectedYear] ?? {};
    }

    get expenseTableDataForSelected(): Record<string, number[]> {
        return this.statisticsData?.expenseTableData[this.selectedYear] ?? {};
    }

    get availableMonthsForSelectedYear(): { name: string; index: number }[] {
        const availableIndexes =
            this.statisticsData?.availableMonthsByYear[this.selectedYear] ?? [];
        return this.months.filter((m) => availableIndexes.includes(m.index));
    }
}
