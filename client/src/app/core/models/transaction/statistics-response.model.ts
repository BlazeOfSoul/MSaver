export interface ChartDataDto {
    labels: string[];
    data: number[];
}

export interface StatisticsResponse {
    incomeChartDataByYear: Record<number, ChartDataDto[]>;
    expenseChartDataByYear: Record<number, ChartDataDto[]>;
    incomeTableData: Record<number, Record<string, number[]>>;
    expenseTableData: Record<number, Record<string, number[]>>;
    availableYears: number[];
    availableMonthsByYear: Record<number, number[]>;
}
