import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { OrderService } from '../../services/order.service';

@Component({
  selector: 'app-order-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './order-create.component.html',
  styleUrl: './order-create.component.css'
})
export class OrderCreateComponent {
  submitting = false;
  error = '';
  successMessage = '';
  validationMessage = '';
  readonly form;

  constructor(
    private readonly fb: FormBuilder,
    private readonly orderService: OrderService,
    private readonly router: Router
  ) {
    this.form = this.fb.group({
      externalReference: ['', [Validators.required, Validators.maxLength(100)]],
      currency: ['USD', [Validators.required, Validators.maxLength(3)]],
      notes: [''],
      customerName: ['', Validators.required],
      customerEmail: ['', [Validators.required, Validators.email]],
      lineItems: this.fb.array([this.createLineItemGroup()])
    });
  }

  get lineItems(): FormArray {
    return this.form.get('lineItems') as FormArray;
  }

  createLineItemGroup() {
    return this.fb.group({
      sku: ['', Validators.required],
      name: ['', Validators.required],
      quantity: [1, [Validators.required, Validators.min(1)]],
      unitPrice: [0, [Validators.required, Validators.min(0)]]
    });
  }

  addLineItem(): void {
    this.lineItems.push(this.createLineItemGroup());
  }

  removeLineItem(index: number): void {
    if (this.lineItems.length > 1) {
      this.lineItems.removeAt(index);
    }
  }

  isInvalid(controlName: string): boolean {
    const control = this.form.get(controlName);
    return !!control && control.invalid && control.touched;
  }

  isLineItemInvalid(index: number, controlName: string): boolean {
    const control = this.lineItems.at(index).get(controlName);
    return !!control && control.invalid && control.touched;
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.validationMessage = 'Please fix the highlighted fields before submitting.';
      this.error = '';
      return;
    }

    const value = this.form.getRawValue();
    this.submitting = true;
    this.error = '';
    this.successMessage = '';
    this.validationMessage = '';

    this.orderService.createOrder({
      externalReference: value.externalReference!,
      currency: value.currency!,
      notes: value.notes || undefined,
      customer: {
        name: value.customerName!,
        email: value.customerEmail!
      },
      lineItems: value.lineItems!.map((item) => ({
        sku: item!.sku!,
        name: item!.name!,
        quantity: Number(item!.quantity),
        unitPrice: Number(item!.unitPrice)
      }))
    }).subscribe({
      next: (order) => {
        this.submitting = false;
        this.successMessage = order.wasDuplicate
          ? `Order already exists for reference "${order.externalReference}". Redirecting to existing order.`
          : 'Order submitted successfully.';
        setTimeout(() => this.router.navigate(['/orders', order.id]), 800);
      },
      error: (err) => {
        this.submitting = false;
        this.error = err.error?.error ?? 'Failed to submit order.';
      }
    });
  }
}
