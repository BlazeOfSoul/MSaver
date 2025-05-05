import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiEndpoints } from '../constants/api-endpoints';
import { Category } from '../models/balance/category.model';

@Injectable({
    providedIn: 'root',
})
export class CategoryService {
    constructor(private http: HttpClient) {}

    getUserCategories(): Observable<Category[]> {
        return this.http.get<Category[]>(ApiEndpoints.Categories.GetAll);
    }
}
