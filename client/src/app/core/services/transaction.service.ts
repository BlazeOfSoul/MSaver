import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environment';
import { Transaction } from '../models/balance/transaction';

@Injectable({
    providedIn: 'root',
})
export class TransactionService {
    private apiUrl = `${environment.apiUrl}/Transactions`;

    constructor(private http: HttpClient) {}

    addTransaction(transaction: Transaction): Observable<any> {
        return this.http.post<any>(`${this.apiUrl}`, transaction);
    }
}
