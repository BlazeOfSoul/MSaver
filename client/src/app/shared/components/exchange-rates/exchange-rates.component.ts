import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { TableModule } from 'primeng/table';
import { Rate } from '../../../core/models/exchange-rate/rate.model';

@Component({
    selector: 'app-exchange-rates',
    standalone: true,
    imports: [CommonModule, TableModule],
    templateUrl: './exchange-rates.component.html',
    styleUrl: './exchange-rates.component.scss',
})
export class ExchangeRatesComponent {
    @Input() fiatRates: Rate[] = [];
    @Input() cryptoRates: Rate[] = [];
}
