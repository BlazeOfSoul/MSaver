import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class StorageService {
    isBrowser(): boolean {
        return typeof window !== 'undefined';
    }

    getItem(key: string): string | null {
        return this.isBrowser() ? sessionStorage.getItem(key) : null;
    }

    setItem(key: string, value: string): void {
        if (this.isBrowser()) {
            sessionStorage.setItem(key, value);
        }
    }

    removeItem(key: string): void {
        if (this.isBrowser()) {
            sessionStorage.removeItem(key);
        }
    }
}
