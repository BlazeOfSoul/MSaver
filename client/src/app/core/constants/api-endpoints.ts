import { environment } from '../../../environment';

const API_BASE = environment.apiUrl;

export const ApiEndpoints = {
    Auth: {
        Login: `${API_BASE}/auth/login`,
        Register: `${API_BASE}/auth/register`,
    },
    ExchangeRates: {
        Base: `${API_BASE}/exchangeRates`,
    },
    Transactions: {
        Base: `${API_BASE}/transactions`,
        Statistics: `${API_BASE}/transactions/statistics`,
    },
    Categories: {
        Base: `${API_BASE}/categories`,
    },
    Balance: {
        Get: `${API_BASE}/Balance/current`,
    },
};
