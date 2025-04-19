import { Rate } from './rate.model';

export interface ExchangeRatesResponse {
    fiat: Rate[];
    crypto: Rate[];
}
