import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { WorkOrder } from './work-order.component';

@Injectable({
  providedIn: 'root'
})
export class WorkOrderService {
  private workOrders: WorkOrder[] = [];

  constructor() {}

  createWorkOrder(workOrder: WorkOrder): Observable<WorkOrder> {
    // Generate a unique ID
    const newWorkOrder: WorkOrder = {
      ...workOrder,
      id: this.generateId(),
      createdAt: new Date(),
      updatedAt: new Date()
    };

    // In a real application, this would be an HTTP POST request
    // For now, we'll simulate the API call
    return new Observable(observer => {
      setTimeout(() => {
        this.workOrders.push(newWorkOrder);
        observer.next(newWorkOrder);
        observer.complete();
      }, 1000); // Simulate network delay
    });
  }

  getWorkOrders(): Observable<WorkOrder[]> {
    // In a real application, this would be an HTTP GET request
    return of(this.workOrders);
  }

  getWorkOrderById(id: string): Observable<WorkOrder | undefined> {
    const workOrder = this.workOrders.find(wo => wo.id === id);
    return of(workOrder);
  }

  updateWorkOrder(id: string, updates: Partial<WorkOrder>): Observable<WorkOrder | undefined> {
    const index = this.workOrders.findIndex(wo => wo.id === id);
    if (index !== -1) {
      this.workOrders[index] = {
        ...this.workOrders[index],
        ...updates,
        updatedAt: new Date()
      };
      return of(this.workOrders[index]);
    }
    return of(undefined);
  }

  deleteWorkOrder(id: string): Observable<boolean> {
    const index = this.workOrders.findIndex(wo => wo.id === id);
    if (index !== -1) {
      this.workOrders.splice(index, 1);
      return of(true);
    }
    return of(false);
  }

  private generateId(): string {
    return 'WO-' + Date.now() + '-' + Math.random().toString(36).substr(2, 9);
  }
}
