import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiEndpoints } from '../constants/api-endpoints';
import { Category } from '../models/category/category.model';
import { CategoryCreateRequest } from '../models/category/create-category-request';

@Injectable({
    providedIn: 'root',
})
export class CategoryService {
    constructor(private http: HttpClient) {}

    getUserCategories(): Observable<Category[]> {
        return this.http.get<Category[]>(ApiEndpoints.Categories.Base);
    }

    createCategory(category: CategoryCreateRequest): Observable<Category> {
        return this.http.post<Category>(ApiEndpoints.Categories.Base, category);
    }

    updateCategory(category: Category): Observable<void> {
        return this.http.put<void>(`${ApiEndpoints.Categories.Base}/${category.id}`, category);
    }

    deleteCategory(id: string): Observable<void> {
        return this.http.delete<void>(`${ApiEndpoints.Categories.Base}/${id}`);
    }
}
