import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiEndpoints } from '../constants/api-endpoints';
import { Transaction } from '../models/balance/transaction';

@Injectable({
    providedIn: 'root',
})
export class TransactionService {
    constructor(private http: HttpClient) {}

    addTransaction(transaction: Transaction): Observable<any> {
        return this.http.post<any>(ApiEndpoints.Transactions.Create, transaction);
    }
}
