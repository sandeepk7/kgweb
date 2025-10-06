import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { WorkOrderService } from './work-order.service';

export interface WorkOrder {
  id?: string;
  title: string;
  description: string;
  priority: 'Low' | 'Medium' | 'High' | 'Critical';
  status: 'Draft' | 'Assigned' | 'In Progress' | 'Completed' | 'Cancelled';
  category: string;
  location: {
    address: string;
    longitude?: number;
    latitude?: number;
  };
  assignedTo?: string;
  estimatedHours?: number;
  actualHours?: number;
  startDate?: Date;
  dueDate?: Date;
  completedDate?: Date;
  customerInfo?: {
    name: string;
    phone: string;
    email: string;
  };
  equipment?: {
    id: string;
    name: string;
    type: string;
  }[];
  attachments?: string[];
  notes?: string;
  createdAt?: Date;
  updatedAt?: Date;
}

@Component({
  selector: 'app-work-order',
  templateUrl: './work-order.component.html',
  styleUrls: ['./work-order.component.css'],
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule]
})
export class WorkOrderComponent implements OnInit {
  workOrderForm!: FormGroup;
  isSubmitting = false;
  submitSuccess = false;
  submitError = '';
  
  mapObjectTitle?: string;

  priorities = ['Low', 'Medium', 'High', 'Critical'];
  statuses = ['Draft', 'Assigned', 'In Progress', 'Completed', 'Cancelled'];
  categories = [
    'Maintenance',
    'Repair',
    'Installation',
    'Inspection',
    'Emergency',
    'Preventive',
    'Upgrade',
    'Other'
  ];

  constructor(
    private fb: FormBuilder,
    private workOrderService: WorkOrderService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.checkForPopupData();
  }

  private initializeForm(): void {
    this.workOrderForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(5)]],
      description: ['', [Validators.required, Validators.minLength(10)]],
      priority: ['Medium', Validators.required],
      status: ['Draft', Validators.required],
      category: ['', Validators.required],
      location: this.fb.group({
        address: ['', Validators.required],
        longitude: [null],
        latitude: [null]
      }),
      assignedTo: [''],
      estimatedHours: [null, [Validators.min(0.5), Validators.max(100)]],
      dueDate: [''],
      customerInfo: this.fb.group({
        name: [''],
        phone: [''],
        email: ['', Validators.email]
      }),
      notes: ['']
    });
  }

  private checkForPopupData(): void {
    // 1) Prefer navigation state provided by CommunicationService
    try {
      const ctx = (window as any).__KG_CONTEXT__;      
      if (ctx) {        
        this.mapObjectTitle = ctx.title || '';
      }
    } catch {}
  }

  
  private validateLongitude(longitude: any): number | null {
    if (longitude === null || longitude === undefined || longitude === '') {
      return null;
    }
    
    const lon = parseFloat(longitude);
    if (isNaN(lon)) {
      return null;
    }
    
    // Longitude must be between -180 and 180
    if (lon < -180 || lon > 180) {
      console.warn(`Invalid longitude value: ${longitude}, clamping to valid range`);
      return Math.max(-180, Math.min(180, lon));
    }
    
    return lon;
  }

  private validateLatitude(latitude: any): number | null {
    if (latitude === null || latitude === undefined || latitude === '') {
      return null;
    }
    
    const lat = parseFloat(latitude);
    if (isNaN(lat)) {
      return null;
    }
    
    // Latitude must be between -90 and 90
    if (lat < -90 || lat > 90) {
      console.warn(`Invalid latitude value: ${latitude}, clamping to valid range`);
      return Math.max(-90, Math.min(90, lat));
    }
    
    return lat;
  }

  onSubmit(): void {
    if (this.workOrderForm.valid) {
      this.isSubmitting = true;
      this.submitError = '';
      this.submitSuccess = false;

      const workOrderData: WorkOrder = {
        ...this.workOrderForm.value,
        createdAt: new Date(),
        updatedAt: new Date()
      };

      this.workOrderService.createWorkOrder(workOrderData).subscribe({
        next: (response) => {
          this.isSubmitting = false;
          this.submitSuccess = true;
          this.workOrderForm.reset();
          this.initializeForm();
          console.log('Work order created successfully:', response);
        },
        error: (error) => {
          this.isSubmitting = false;
          this.submitError = error.message || 'Failed to create work order';
          console.error('Error creating work order:', error);
        }
      });
    } else {
      this.markFormGroupTouched();
    }
  }

  onReset(): void {
    this.workOrderForm.reset();
    this.initializeForm();
    this.submitSuccess = false;
    this.submitError = '';
  }

  private markFormGroupTouched(): void {
    Object.keys(this.workOrderForm.controls).forEach(key => {
      const control = this.workOrderForm.get(key);
      control?.markAsTouched();
      
      if (control instanceof FormGroup) {
        Object.keys(control.controls).forEach(nestedKey => {
          control.get(nestedKey)?.markAsTouched();
        });
      }
    });
  }

  getFieldError(fieldName: string): string {
    const field = this.workOrderForm.get(fieldName);
    if (field?.errors && field.touched) {
      if (field.errors['required']) return `${fieldName} is required`;
      if (field.errors['minlength']) return `${fieldName} must be at least ${field.errors['minlength'].requiredLength} characters`;
      if (field.errors['email']) return 'Please enter a valid email address';
      if (field.errors['min']) return `${fieldName} must be at least ${field.errors['min'].min}`;
      if (field.errors['max']) return `${fieldName} must be less than ${field.errors['max'].max}`;
    }
    return '';
  }

  getNestedFieldError(groupName: string, fieldName: string): string {
    const field = this.workOrderForm.get(`${groupName}.${fieldName}`);
    if (field?.errors && field.touched) {
      if (field.errors['required']) return `${fieldName} is required`;
      if (field.errors['email']) return 'Please enter a valid email address';
    }
    return '';
  }

  clearMapObject(): void {
    // Clear the context data
    this.mapObjectTitle = undefined;    
    (window as any).__KG_CONTEXT__ = undefined;
  }
}
