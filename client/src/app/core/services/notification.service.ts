import { Injectable } from '@angular/core';
import { MessageService } from 'primeng/api';

@Injectable({
    providedIn: 'root',
})
export class NotificationService {
    constructor(private messageService: MessageService) {}

    showError(message: string) {
        this.messageService.add({
            severity: 'error',
            summary: 'Что-то пошло не так...',
            detail: message,
        });
    }

    showSuccess(message: string) {
        this.messageService.add({
            severity: 'success',
            summary: 'Успешно',
            detail: message,
        });
    }

    showInfo(message: string) {
        this.messageService.add({
            severity: 'info',
            summary: 'Информация',
            detail: message,
        });
    }
}
