import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ChartModule } from 'primeng/chart';
import { DropdownModule } from 'primeng/dropdown';

@Component({
    selector: 'app-expense-chart-tab',
    standalone: true,
    imports: [CommonModule, DropdownModule, ChartModule, FormsModule],
    templateUrl: './expense-chart-tab.component.html',
    styleUrls: ['./expense-chart-tab.component.scss'],
})
export class ExpenseChartTabComponent {
    @Input() months: { name: string; index: number }[] = [];
    @Input() expenseChartDataByMonth: any[] = [];
    @Input() chartOptions: any;

    private _selectedMonthIndex = 0;

    @Input()
    get selectedMonthIndex(): number {
        return this._selectedMonthIndex;
    }
    set selectedMonthIndex(value: number) {
        this._selectedMonthIndex = value;
        this.selectedMonthIndexLocal = value;
    }

    @Output() selectedMonthIndexChange = new EventEmitter<number>();

    selectedMonthIndexLocal = 0;

    onMonthChange(event: number) {
        this.selectedMonthIndexLocal = event;
        this._selectedMonthIndex = event;
        this.selectedMonthIndexChange.emit(event);
    }
}
