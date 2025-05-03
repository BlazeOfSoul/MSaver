import { environment } from '../../../environment';

const API_BASE = environment.apiUrl;

export const ApiEndpoints = {
    Auth: {
        Login: `${API_BASE}/auth/login`,
        Register: `${API_BASE}/auth/register`,
    },
    ExchangeRates: {
        Get: `${API_BASE}/exchangeRates`,
    },
    Transactions: {
        Create: `${API_BASE}/transactions`,
    },
    Categories: {
        GetAll: `${API_BASE}/categories`,
    },
    Balance: {
        Get: `${API_BASE}/Balance/current`,
    },
};
