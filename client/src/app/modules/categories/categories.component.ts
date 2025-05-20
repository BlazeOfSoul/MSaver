import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { NotificationMessages } from '../../core/constants/notification-messages';
import { CategoryType } from '../../core/enums/transaction-type.enum';
import { Category } from '../../core/models/balance/category.model';
import { CategoryService } from '../../core/services/category.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
    selector: 'app-categories',
    standalone: true,
    imports: [CommonModule, TableModule, ButtonModule, CardModule],
    templateUrl: './categories.component.html',
})
export class CategoriesComponent {
    incomeCategories: Category[] = [];
    expenseCategories: Category[] = [];

    constructor(
        private categoryService: CategoryService,
        private notificationService: NotificationService
    ) {}

    ngOnInit() {
        this.loadCategories();
    }

    loadCategories() {
        this.categoryService.getUserCategories().subscribe({
            next: (data) => {
                this.incomeCategories = data.filter(
                    (category) => category.type === CategoryType.Income
                );
                this.expenseCategories = data.filter(
                    (category) => category.type === CategoryType.Expense
                );
            },
            error: () => {
                this.notificationService.showError(NotificationMessages.LoadCategoriesError);
            },
        });
    }

    addCategory() {
        // Реализация добавления категории
        console.log('Добавление новой категории');
        // this.notificationService.showSuccess('Категория добавлена');
    }

    editCategory(category: Category) {
        // Реализация редактирования категории
        console.log('Редактирование категории', category);
        // this.notificationService.showInfo('Редактирование категории');
    }

    deleteCategory(category: Category) {
        // Реализация удаления категории
        console.log('Удаление категории', category);
        // this.notificationService.showWarn('Категория удалена');
    }

    getRowStyle(category: Category) {
        return {
            'background-color': `${category.color}20`,
            'border-left': `4px solid ${category.color}`,
        };
    }
}
