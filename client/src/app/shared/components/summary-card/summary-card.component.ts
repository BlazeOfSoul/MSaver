import { Component, Input } from '@angular/core';
import { CardModule } from 'primeng/card';

@Component({
    selector: 'app-summary-card',
    standalone: true,
    imports: [CardModule],
    templateUrl: './summary-card.component.html',
    styleUrls: ['./summary-card.component.scss'],
})
export class SummaryCardComponent {
    @Input() title!: string;
    @Input() value!: number | string;
    @Input() backgroundColor: string = 'rgba(255, 255, 255, 0.8)';
    @Input() textColor: string = '#000000';
}
