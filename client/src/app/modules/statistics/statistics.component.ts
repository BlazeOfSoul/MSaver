import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { TabViewModule } from 'primeng/tabview';
import { ExpenseChartTabComponent } from '../../shared/components/expense-chart-tab/expense-chart-tab.component';
import { IncomeChartTabComponent } from '../../shared/components/income-chart-tab/income-chart-tab.component';
import { SummaryTableTabComponent } from '../../shared/components/summary-table-tab/summary-table-tab.component';

@Component({
    selector: 'app-statistics',
    standalone: true,
    imports: [
        CommonModule,
        IncomeChartTabComponent,
        ExpenseChartTabComponent,
        SummaryTableTabComponent,
        TabViewModule,
    ],
    templateUrl: './statistics.component.html',
    styleUrls: ['./statistics.component.scss'],
})
export class StatisticsComponent {
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

    selectedMonthIndex = 0;

    // Пример мок-данных для графиков (структура зависит от PrimeNG Chart)
    incomeChartDataByMonth = this.months.map(() => ({
        labels: ['Зарплата', 'Инвестиции', 'Подарки'],
        datasets: [
            {
                label: 'Доходы',
                backgroundColor: '#4caf50',
                data: [1200, 300, 150],
            },
        ],
    }));

    expenseChartDataByMonth = this.months.map(() => ({
        labels: ['Еда', 'Транспорт', 'Развлечения'],
        datasets: [
            {
                label: 'Расходы',
                backgroundColor: '#f44336',
                data: [400, 200, 150],
            },
        ],
    }));

    chartOptions = {
        responsive: true,
        scales: {
            y: {
                beginAtZero: true,
            },
        },
    };

    // Мок-данные для таблицы (ключ — категория, значение — массив по месяцам)
    incomeTableData: Record<string, number[]> = {
        Зарплата: [1200, 1200, 1200, 1200, 1200, 1200, 1200, 1200, 1200, 1200, 1200, 1200],
        Инвестиции: [300, 350, 400, 300, 250, 300, 350, 300, 400, 300, 350, 300],
    };

    expenseTableData: Record<string, number[]> = {
        Еда: [400, 420, 450, 430, 410, 400, 390, 410, 430, 440, 450, 460],
        Транспорт: [200, 210, 220, 200, 190, 200, 210, 200, 220, 230, 210, 200],
        Развлечения: [150, 140, 130, 160, 150, 140, 130, 150, 160, 140, 130, 150],
    };
}
