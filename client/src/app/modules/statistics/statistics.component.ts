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
    activeTabIndex = 0;
    statisticsData: StatisticsResponse | null = null;

    constructor(private transactionService: TransactionService) {}

    ngOnInit(): void {
        this.transactionService.getStatistics().subscribe((data) => {
            this.statisticsData = data;
        });
    }

    get availableYears(): number[] {
        return this.statisticsData?.availableYears ?? [];
    }

    get incomeChartData(): Record<number, any> {
        return this.statisticsData?.incomeChartDataByYear ?? {};
    }

    get expenseChartData(): Record<number, any> {
        return this.statisticsData?.expenseChartDataByYear ?? {};
    }

    get incomeTableData(): Record<number, Record<string, Record<number, number>>> {
        return this.statisticsData?.incomeTableData ?? {};
    }

    get expenseTableData(): Record<number, Record<string, Record<number, number>>> {
        return this.statisticsData?.expenseTableData ?? {};
    }

    get availableMonthsByYear(): Record<number, number[]> {
        return this.statisticsData?.availableMonthsByYear ?? {};
    }
}
