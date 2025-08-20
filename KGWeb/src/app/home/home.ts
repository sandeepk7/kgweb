import { Component, OnInit, OnDestroy } from '@angular/core';
import { KGWinService, KGWinStatus } from '../services/kgwin.service';
import { Subscription } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-home',
  imports: [FormsModule, CommonModule],
  templateUrl: './home.html',
  styleUrl: './home.css'
})
export class Home implements OnInit, OnDestroy {
  kgWinStatus: KGWinStatus = {
    isRunning: false,
    isConnected: false,
    lastChecked: new Date()
  };
  
  // Dropdown options for Asset ID
  assetOptions = [
    { value: 'ASSET_001', label: 'Asset 001 - Main Building' },
    { value: 'ASSET_002', label: 'Asset 002 - Warehouse A' },
    { value: 'ASSET_003', label: 'Asset 003 - Office Complex' },
    { value: 'ASSET_004', label: 'Asset 004 - Parking Garage' },
    { value: 'ASSET_005', label: 'Asset 005 - Maintenance Shed' }
  ];

  // Dropdown options for Layer ID
  layerOptions = [
    { value: 'LAYER_001', label: 'Layer 001 - Building Footprint' },
    { value: 'LAYER_002', label: 'Layer 002 - Electrical Systems' },
    { value: 'LAYER_003', label: 'Layer 003 - Plumbing Network' },
    { value: 'LAYER_004', label: 'Layer 004 - HVAC Systems' },
    { value: 'LAYER_005', label: 'Layer 005 - Security Cameras' }
  ];

  // Selected values (default to first options)
  selectedAssetId: string = 'ASSET_001';
  selectedLayerId: string = 'LAYER_001';
  
  private statusSubscription: Subscription;

  constructor(private kgWinService: KGWinService) {
    this.statusSubscription = this.kgWinService.status$.subscribe(status => {
      this.kgWinStatus = status;
    });
  }

  ngOnInit() {
    // Check KGWin status on component initialization
    this.checkKGWinStatus();
  }

  ngOnDestroy() {
    if (this.statusSubscription) {
      this.statusSubscription.unsubscribe();
    }
  }

  async launchKGWin() {
    try {
      const success = await this.kgWinService.launchKGWin(this.selectedAssetId, this.selectedLayerId);
      
      if (success) {
        this.showMessage(`KGWin application launch initiated with Asset: ${this.selectedAssetId}, Layer: ${this.selectedLayerId}!`, 'success');
      } else {
        this.showMessage('Failed to launch KGWin application. Please ensure it is installed.', 'error');
      }
    } catch (error) {
      console.error('Error launching KGWin:', error);
      this.showMessage('An error occurred while launching KGWin.', 'error');
    }
  }

  async checkKGWinStatus() {
    try {
      await this.kgWinService.checkStatus();
    } catch (error) {
      console.error('Error checking KGWin status:', error);
    }
  }

  getStatusText(): string {
    if (this.kgWinStatus.isRunning && this.kgWinStatus.isConnected) {
      return 'Connected';
    } else if (this.kgWinStatus.isRunning) {
      return 'Running';
    } else {
      return 'Not Running';
    }
  }

  getStatusClass(): string {
    if (this.kgWinStatus.isRunning && this.kgWinStatus.isConnected) {
      return 'text-success';
    } else if (this.kgWinStatus.isRunning) {
      return 'text-warning';
    } else {
      return 'text-danger';
    }
  }

  private showMessage(message: string, type: 'success' | 'error' | 'warning' | 'info') {
    // Create a simple toast notification
    const toast = document.createElement('div');
    toast.className = `alert alert-${type === 'error' ? 'danger' : type} alert-dismissible fade show position-fixed`;
    toast.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    toast.innerHTML = `
      ${message}
      <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    document.body.appendChild(toast);
    
    // Auto-remove after 5 seconds
    setTimeout(() => {
      if (toast.parentNode) {
        toast.parentNode.removeChild(toast);
      }
    }, 5000);
  }
}
