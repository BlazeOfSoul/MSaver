import { Injectable } from '@angular/core';
import { MessageService } from 'primeng/api';
import { NotificationMessages } from '../constants/notification-messages';

@Injectable({
    providedIn: 'root',
})
export class NotificationService {
    constructor(private messageService: MessageService) {}

    showSuccess(message: string) {
        this.messageService.add({
            severity: 'success',
            summary: NotificationMessages.NotificationSuccessSummary,
            detail: message,
        });
    }

    showError(message: string) {
        this.messageService.add({
            severity: 'error',
            summary: NotificationMessages.NotificationErrorSummary,
            detail: message,
        });
    }

    showInfo(message: string) {
        this.messageService.add({
            severity: 'info',
            summary: NotificationMessages.NotificationInfoSummary,
            detail: message,
        });
    }
}
