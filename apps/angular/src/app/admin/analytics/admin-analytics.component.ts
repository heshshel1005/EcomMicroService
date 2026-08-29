import { AsyncPipe, DatePipe, NgFor, NgIf } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LocalizationPipe } from '@abp/ng.core';
import { AnalyticsAdminService, SalesByDay, SalesSummary, TopProduct } from './analytics-admin.service';

@Component({
  selector: 'app-admin-analytics',
  standalone: true,
  imports: [NgIf, NgFor, AsyncPipe, DatePipe, FormsModule, LocalizationPipe],
  templateUrl: './admin-analytics.component.html',
  styleUrls: ['./admin-analytics.component.scss'],
})
export class AdminAnalyticsComponent implements OnInit {
  private readonly service = inject(AnalyticsAdminService);

  dateFrom?: string;
  dateTo?: string;

  loading = false;
  summary?: SalesSummary;
  byDay: SalesByDay[] = [];
  topProducts: TopProduct[] = [];

  ngOnInit(): void {
    const today = new Date();
    const from = new Date();
    from.setDate(today.getDate() - 29);
    this.dateFrom = from.toISOString().substring(0, 10);
    this.dateTo = today.toISOString().substring(0, 10);
    this.refresh();
  }

  refresh(): void {
    this.loading = true;
    const filter = {
      dateFrom: this.dateFrom ? `${this.dateFrom}T00:00:00Z` : null,
      dateTo: this.dateTo ? `${this.dateTo}T23:59:59Z` : null,
    };

    this.service.getSummary(filter).subscribe(summary => (this.summary = summary));
    this.service.getSalesByDay(filter).subscribe(days => (this.byDay = days));
    this.service.getTopProducts(filter).subscribe(products => {
      this.topProducts = products;
      this.loading = false;
    });
  }

  exportCsv(): void {
    const filter = {
      dateFrom: this.dateFrom ? `${this.dateFrom}T00:00:00Z` : null,
      dateTo: this.dateTo ? `${this.dateTo}T23:59:59Z` : null,
    };
    this.service.exportCsv(filter).subscribe(blob => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'analytics.csv';
      a.click();
      window.URL.revokeObjectURL(url);
    });
  }
}

