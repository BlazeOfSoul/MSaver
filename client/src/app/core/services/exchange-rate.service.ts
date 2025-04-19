import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environment';
import { ExchangeRatesResponse } from '../models/exchange-rate/exchange-rates-response.model';

@Injectable({ providedIn: 'root' })
export class ExchangeRateService {
    private apiUrl = environment.apiUrl;

    constructor(private http: HttpClient) {}

    getRates() {
        return this.http.get<ExchangeRatesResponse>(`${this.apiUrl}/exchangeRates`);
    }
}
