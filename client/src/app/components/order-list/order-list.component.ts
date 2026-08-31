import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { OrderService } from '../../services/order.service';
import { Order, OrderStatus, OrderStatusLabels } from '../../models/order.model';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './order-list.component.html',
  styleUrl: './order-list.component.css'
})
export class OrderListComponent implements OnInit {
  orders: Order[] = [];
  loading = true;
  error = '';
  statusFilter = '';
  readonly statusLabels = OrderStatusLabels;
  readonly statusOptions = Object.entries(OrderStatusLabels);

  constructor(private readonly orderService: OrderService) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.loading = true;
    this.error = '';
    this.orderService.getOrders().subscribe({
      next: (orders) => {
        this.orders = orders;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load orders. Is the API running?';
        this.loading = false;
      }
    });
  }

  get filteredOrders(): Order[] {
    if (!this.statusFilter) {
      return this.orders;
    }

    return this.orders.filter((order) => order.status === Number(this.statusFilter));
  }

  statusClass(status: OrderStatus): string {
    return `status-${OrderStatus[status].toLowerCase()}`;
  }
}
