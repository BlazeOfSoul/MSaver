import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { TabViewModule } from 'primeng/tabview';
import { ChartTabComponent } from '../../shared/chart-tab/chart-tab.component';
import { SummaryTableTabComponent } from '../../shared/components/summary-table-tab/summary-table-tab.component';

@Component({
    selector: 'app-statistics',
    standalone: true,
    imports: [CommonModule, ChartTabComponent, SummaryTableTabComponent, TabViewModule],
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

    incomeChartDataByMonth = [
        {
            labels: [
                'Зарплата',
                'Фриланс',
                'Инвестиции',
                'Сдача жилья',
                'Кэшбэк',
                'Продажа вещей',
                'Подарки',
                'Бонусы',
                'Проценты по вкладу',
                'Дивиденды',
                'Возврат налогов',
                'Пассивный доход',
                'Премия',
                'Подработка',
                'Компенсация',
                'Доход с YouTube',
                'Реферальные',
                'Крипта',
                'Субсидии',
                'Другое',
            ],
            datasets: [
                {
                    data: [
                        1200, 800, 500, 400, 150, 200, 300, 250, 100, 180, 130, 220, 400, 350, 170,
                        90, 160, 600, 140, 110,
                    ],
                    backgroundColor: [
                        'rgba(255, 99, 132, 0.1)',
                        'rgba(54, 162, 235, 0.1)',
                        'rgba(255, 206, 86, 0.1)',
                        'rgba(75, 192, 192, 0.1)',
                        'rgba(153, 102, 255, 0.1)',
                        'rgba(255, 159, 64, 0.1)',
                        'rgba(199, 199, 199, 0.1)',
                        'rgba(255, 99, 255, 0.1)',
                        'rgba(99, 255, 132, 0.1)',
                        'rgba(255, 206, 186, 0.1)',
                        'rgba(86, 255, 206, 0.1)',
                        'rgba(192, 75, 192, 0.1)',
                        'rgba(102, 153, 255, 0.1)',
                        'rgba(159, 64, 255, 0.1)',
                        'rgba(255, 99, 132, 0.1)',
                        'rgba(64, 159, 255, 0.1)',
                        'rgba(99, 132, 255, 0.1)',
                        'rgba(132, 255, 99, 0.1)',
                        'rgba(235, 64, 52, 0.1)',
                        'rgba(255, 215, 0, 0.1)',
                    ],
                    borderColor: [
                        '#ff6384',
                        '#36a2eb',
                        '#ffce56',
                        '#4bc0c0',
                        '#9966ff',
                        '#ff9f40',
                        '#c7c7c7',
                        '#ff63ff',
                        '#63ff84',
                        '#ffceba',
                        '#56ffce',
                        '#c04bc0',
                        '#6699ff',
                        '#9f40ff',
                        '#ff6384',
                        '#409fff',
                        '#6384ff',
                        '#84ff63',
                        '#eb4034',
                        '#ffd700',
                    ],
                    borderWidth: 2,
                },
            ],
        },
    ];

    expenseChartDataByMonth = this.months.map(() => ({
        labels: ['Еда', 'Транспорт', 'Развлечения'],
        datasets: [
            {
                label: 'Расходы',
                data: [1000, 500, 400],
                backgroundColor: ['rgba(255, 99, 132, 0.1)', 'rgba(54, 162, 235, 0.1)'],
                borderColor: ['#ff6384', '#36a2eb'],
                borderWidth: 2,
            },
        ],
    }));

    incomeTableData: Record<string, number[]> = {
        Зарплата: [1000, 1200, 1200, 1200, 1200, 1200, 1200, 1200, 1200, 1200, 1200, 1200],
        Инвестиции: [300, 350, 400, 300, 250, 300, 350, 300, 400, 300, 350, 300],
    };

    expenseTableData: Record<string, number[]> = {
        Еда: [400, 420, 450, 430, 410, 400, 390, 410, 430, 440, 450, 460],
        Транспорт: [200, 210, 220, 200, 190, 200, 210, 200, 220, 230, 210, 200],
        Развлечения: [150, 140, 130, 160, 150, 140, 130, 150, 160, 140, 130, 150],
    };
}
