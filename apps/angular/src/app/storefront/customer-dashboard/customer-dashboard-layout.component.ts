import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { LocalizationPipe } from '@abp/ng.core';

@Component({
  selector: 'app-customer-dashboard-layout',
  templateUrl: './customer-dashboard-layout.component.html',
  styleUrls: ['./customer-dashboard-layout.component.scss'],
  imports: [RouterOutlet, RouterLink, RouterLinkActive, LocalizationPipe],
})
export class CustomerDashboardLayoutComponent {}
