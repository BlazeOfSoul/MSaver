import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { Category } from '../../../core/models/category/category.model';

@Component({
    selector: 'app-categories-table',
    standalone: true,
    imports: [CommonModule, TableModule, ButtonModule, CardModule],
    templateUrl: './categories-table.component.html',
    styleUrls: ['./categories-table.component.scss'],
})
export class CategoriesTableComponent {
    @Input() categories: Category[] = [];
    @Input() title: string = '';
    @Output() edit = new EventEmitter<Category>();
    @Output() delete = new EventEmitter<Category>();

    getRowStyle(category: Category) {
        return {
            'background-color': `${category.color}20`,
            'border-left': `4px solid ${category.color}`,
        };
    }

    onEdit(category: Category) {
        this.edit.emit(category);
    }

    onDelete(category: Category) {
        this.delete.emit(category);
    }
}
