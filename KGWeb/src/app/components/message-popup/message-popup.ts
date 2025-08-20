import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface MessageData {
  type: 'info' | 'success' | 'warning' | 'error';
  title: string;
  content: string;
  timestamp: Date;
  data?: any;
}

@Component({
  selector: 'app-message-popup',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="message-popup-overlay" *ngIf="isVisible" (click)="close()">
      <div class="message-popup" (click)="$event.stopPropagation()">
        <div class="message-header" [ngClass]="'message-' + message.type">
          <h5 class="message-title">{{ message.title }}</h5>
          <button class="close-btn" (click)="close()">&times;</button>
        </div>
        <div class="message-body">
          <div class="message-content">{{ message.content }}</div>
          <div class="message-timestamp">
            <small>{{ message.timestamp | date:'medium' }}</small>
          </div>
          <div class="message-data" *ngIf="message.data">
            <details>
              <summary>View Data</summary>
              <pre>{{ message.data | json }}</pre>
            </details>
          </div>
        </div>
        <div class="message-footer">
          <button class="btn btn-primary btn-sm" (click)="close()">Close</button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .message-popup-overlay {
      position: fixed;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      background-color: rgba(0, 0, 0, 0.5);
      display: flex;
      justify-content: center;
      align-items: center;
      z-index: 9999;
    }

    .message-popup {
      background: white;
      border-radius: 8px;
      box-shadow: 0 4px 20px rgba(0, 0, 0, 0.3);
      max-width: 500px;
      width: 90%;
      max-height: 80vh;
      overflow: hidden;
      animation: slideIn 0.3s ease-out;
    }

    @keyframes slideIn {
      from {
        transform: translateY(-50px);
        opacity: 0;
      }
      to {
        transform: translateY(0);
        opacity: 1;
      }
    }

    .message-header {
      padding: 15px 20px;
      border-bottom: 1px solid #e0e0e0;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .message-info {
      background-color: #d1ecf1;
      border-left: 4px solid #17a2b8;
    }

    .message-success {
      background-color: #d4edda;
      border-left: 4px solid #28a745;
    }

    .message-warning {
      background-color: #fff3cd;
      border-left: 4px solid #ffc107;
    }

    .message-error {
      background-color: #f8d7da;
      border-left: 4px solid #dc3545;
    }

    .message-title {
      margin: 0;
      font-size: 16px;
      font-weight: 600;
    }

    .close-btn {
      background: none;
      border: none;
      font-size: 24px;
      cursor: pointer;
      color: #666;
      padding: 0;
      width: 30px;
      height: 30px;
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: 50%;
      transition: background-color 0.2s;
    }

    .close-btn:hover {
      background-color: rgba(0, 0, 0, 0.1);
    }

    .message-body {
      padding: 20px;
      max-height: 400px;
      overflow-y: auto;
    }

    .message-content {
      margin-bottom: 15px;
      line-height: 1.5;
      white-space: pre-wrap;
    }

    .message-timestamp {
      margin-bottom: 15px;
      color: #666;
    }

    .message-data {
      margin-top: 15px;
      border-top: 1px solid #e0e0e0;
      padding-top: 15px;
    }

    .message-data summary {
      cursor: pointer;
      font-weight: 600;
      color: #007ACC;
      margin-bottom: 10px;
    }

    .message-data pre {
      background-color: #f8f9fa;
      border: 1px solid #e0e0e0;
      border-radius: 4px;
      padding: 10px;
      font-size: 12px;
      overflow-x: auto;
      max-height: 200px;
      overflow-y: auto;
    }

    .message-footer {
      padding: 15px 20px;
      border-top: 1px solid #e0e0e0;
      text-align: right;
    }
  `]
})
export class MessagePopup {
  @Input() message: MessageData = {
    type: 'info',
    title: 'Message',
    content: '',
    timestamp: new Date()
  };

  @Input() isVisible = false;

  @Output() closeEvent = new EventEmitter<void>();

  close(): void {
    this.closeEvent.emit();
  }
}
