import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { ColorPickerModule } from 'primeng/colorpicker';
import { DialogModule } from 'primeng/dialog';
import { DropdownModule } from 'primeng/dropdown';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule } from 'primeng/table';
import { NotificationMessages } from '../../core/constants/notification-messages';
import { CategoryType } from '../../core/enums/transaction-type.enum';
import { Category } from '../../core/models/category/category.model';
import { CategoryCreateRequest } from '../../core/models/category/create-category-request';
import { CategoryService } from '../../core/services/category.service';
import { NotificationService } from '../../core/services/notification.service';
import { CategoriesTableComponent } from '../../shared/components/categories-table/categories-table.component';

@Component({
    selector: 'app-categories',
    standalone: true,
    imports: [
        CommonModule,
        TableModule,
        ButtonModule,
        CardModule,
        CategoriesTableComponent,
        DialogModule,
        FormsModule,
        InputTextModule,
        DropdownModule,
        ColorPickerModule,
    ],
    templateUrl: './categories.component.html',
})
export class CategoriesComponent implements OnInit {
    incomeCategories: Category[] = [];
    expenseCategories: Category[] = [];

    isDialogVisible = false;
    isEditMode = false;

    categoryId?: string | null;

    categoryName = '';
    categoryType: CategoryType | null = null;
    categoryColor = '#000000';

    categoryTypeOptions = [
        { label: 'Доход', value: CategoryType.Income },
        { label: 'Расход', value: CategoryType.Expense },
    ];

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

    openCreateDialog() {
        this.isEditMode = false;
        this.categoryId = null;

        this.categoryName = '';
        this.categoryType = null;
        this.categoryColor = '#000000';

        this.isDialogVisible = true;
    }

    openEditDialog(category: Category) {
        this.isEditMode = true;
        this.categoryId = category.id;

        this.categoryName = category.name;
        this.categoryType = category.type;
        this.categoryColor = category.color;

        this.isDialogVisible = true;
    }

    saveCategory() {
        if (this.isEditMode && this.categoryId) {
            const updatedCategory: Category = {
                id: this.categoryId,
                name: this.categoryName,
                type: this.categoryType!,
                color: this.categoryColor,
            };

            this.categoryService.updateCategory(updatedCategory).subscribe({
                next: () => {
                    this.notificationService.showSuccess(
                        NotificationMessages.CategoryUpdateSuccess
                    );
                    this.loadCategories();
                    this.closeDialog();
                },
                error: () => {
                    this.notificationService.showError(NotificationMessages.CategoryUpdateError);
                },
            });
        } else {
            const newCategory: CategoryCreateRequest = {
                name: this.categoryName,
                type: this.categoryType!,
                color: this.categoryColor,
            };

            this.categoryService.createCategory(newCategory).subscribe({
                next: () => {
                    this.notificationService.showSuccess(
                        NotificationMessages.CategoryCreateSuccess
                    );
                    this.loadCategories();
                    this.closeDialog();
                },
                error: () => {
                    this.notificationService.showError(NotificationMessages.CategoryCreateError);
                },
            });
        }
    }

    deleteCategory(category: Category) {
        this.categoryService.deleteCategory(category.id).subscribe({
            next: () => {
                this.notificationService.showSuccess(NotificationMessages.CategoryDeleteSuccess);
                this.loadCategories();
            },
            error: () => {
                this.notificationService.showError(NotificationMessages.CategoryDeleteError);
            },
        });
    }

    closeDialog() {
        this.isDialogVisible = false;
    }
}
