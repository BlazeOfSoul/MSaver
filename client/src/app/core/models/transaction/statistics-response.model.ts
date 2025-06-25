import { ChartData } from './chart-data';

export interface StatisticsResponse {
    incomeChartDataByYear: Record<number, ChartData[]>;
    expenseChartDataByYear: Record<number, ChartData[]>;
    incomeTableData: Record<number, Record<string, number[]>>;
    expenseTableData: Record<number, Record<string, number[]>>;
    availableYears: number[];
    availableMonthsByYear: Record<number, number[]>;
}
