import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiEndpoints } from '../constants/api-endpoints';
import { StatisticsResponse } from '../models/transaction/statistics-response.model';
import { Transaction } from '../models/transaction/transaction';

@Injectable({
    providedIn: 'root',
})
export class TransactionService {
    constructor(private http: HttpClient) {}

    addTransaction(transaction: Transaction): Observable<any> {
        return this.http.post<any>(ApiEndpoints.Transactions.Base, transaction);
    }

    getStatistics(): Observable<StatisticsResponse> {
        return this.http.get<StatisticsResponse>(ApiEndpoints.Transactions.Statistics);
    }
}
