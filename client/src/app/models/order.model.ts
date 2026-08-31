export enum OrderStatus {
  Pending = 0,
  Confirmed = 1,
  Fulfilled = 2,
  Cancelled = 3
}

export const OrderStatusLabels: Record<OrderStatus, string> = {
  [OrderStatus.Pending]: 'Pending',
  [OrderStatus.Confirmed]: 'Confirmed',
  [OrderStatus.Fulfilled]: 'Fulfilled',
  [OrderStatus.Cancelled]: 'Cancelled'
};

export interface Customer {
  name: string;
  email: string;
}

export interface LineItem {
  sku: string;
  name: string;
  quantity: number;
  unitPrice: number;
}

export interface LineItemResponse extends LineItem {
  id: string;
  lineTotal: number;
}

export interface CreateOrderRequest {
  externalReference: string;
  currency: string;
  notes?: string;
  customer: Customer;
  lineItems: LineItem[];
}

export interface Order {
  id: string;
  externalReference: string;
  createdAt: string;
  updatedAt?: string;
  status: OrderStatus;
  currency: string;
  notes?: string;
  customer: Customer;
  lineItems: LineItemResponse[];
  subtotal: number;
  total: number;
  wasDuplicate?: boolean;
}

export interface UpdateOrderStatusRequest {
  newStatus: OrderStatus;
}
