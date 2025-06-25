import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ApiEndpoints } from '../constants/api-endpoints';
import { ExchangeRatesResponse } from '../models/exchange-rate/exchange-rates-response.model';

@Injectable({ providedIn: 'root' })
export class ExchangeRateService {
    constructor(private http: HttpClient) {}

    getRates() {
        return this.http.get<ExchangeRatesResponse>(ApiEndpoints.ExchangeRates.Base);
    }
}
