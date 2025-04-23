import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environment';
import { Category } from '../models/balance/category.model';

@Injectable({
    providedIn: 'root',
})
export class CategoryService {
    private apiUrl = `${environment.apiUrl}/categories`;

    constructor(private http: HttpClient) {}

    getUserCategories(): Observable<Category[]> {
        return this.http.get<Category[]>(`${this.apiUrl}`);
    }
}
