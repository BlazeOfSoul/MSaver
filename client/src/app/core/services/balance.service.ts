import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiEndpoints } from '../constants/api-endpoints';
import { BalanceResponse } from '../models/balance/balance-response.model';

@Injectable({
    providedIn: 'root',
})
export class BalanceService {
    constructor(private http: HttpClient) {}

    getCurrentBalance(): Observable<BalanceResponse> {
        return this.http.get<BalanceResponse>(ApiEndpoints.Balance.Get);
    }
}
