import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { OrderService } from '../../services/order.service';
import { Order, OrderStatus, OrderStatusLabels } from '../../models/order.model';

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './order-detail.component.html',
  styleUrl: './order-detail.component.css'
})
export class OrderDetailComponent implements OnInit {
  order?: Order;
  loading = true;
  error = '';
  statusMessage = '';
  selectedStatus?: OrderStatus;
  updatingStatus = false;
  readonly statusLabels = OrderStatusLabels;
  readonly statusOptions = Object.entries(OrderStatusLabels);

  constructor(
    private readonly route: ActivatedRoute,
    private readonly orderService: OrderService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.error = 'Order id is required.';
      this.loading = false;
      return;
    }

    this.orderService.getOrder(id).subscribe({
      next: (order) => {
        this.order = order;
        this.selectedStatus = order.status;
        this.loading = false;
      },
      error: () => {
        this.error = 'Order not found.';
        this.loading = false;
      }
    });
  }

  updateStatus(): void {
    if (!this.order || this.selectedStatus === undefined || this.selectedStatus === this.order.status) {
      return;
    }

    this.updatingStatus = true;
    this.statusMessage = '';

    this.orderService.updateStatus(this.order.id, { newStatus: this.selectedStatus }).subscribe({
      next: (order) => {
        this.order = order;
        this.selectedStatus = order.status;
        this.updatingStatus = false;
        this.statusMessage = 'Status updated successfully.';
      },
      error: (err) => {
        this.updatingStatus = false;
        this.statusMessage = err.error?.error ?? 'Failed to update status.';
      }
    });
  }

  statusClass(status: OrderStatus): string {
    return `status-${OrderStatus[status].toLowerCase()}`;
  }
}
