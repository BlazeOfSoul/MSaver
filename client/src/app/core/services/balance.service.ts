import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environment';
import { BalanceResponse } from '../models/balance/balance-response.model';

@Injectable({
    providedIn: 'root',
})
export class BalanceService {
    private apiUrl = environment.apiUrl;

    constructor(private http: HttpClient) {}

    getCurrentBalance(): Observable<BalanceResponse> {
        return this.http.get<BalanceResponse>(`${this.apiUrl}/balance/current`);
    }
}
