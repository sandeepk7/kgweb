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
    
    // Check for data from KGWin in URL parameters
    this.checkForKGWinData();
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

  private checkForKGWinData() {
    try {
      const urlParams = new URLSearchParams(window.location.search);
      const dataParam = urlParams.get('data');
      
      if (dataParam) {
        // Decode the data
        const decodedData = decodeURIComponent(dataParam);
        const kgWinData = JSON.parse(decodedData);
        
        // Show popup with KGWin data
        this.showKGWinDataPopup(kgWinData);
        
        // Clear the URL parameter to avoid showing popup again on refresh
        const newUrl = window.location.pathname + window.location.hash;
        window.history.replaceState({}, document.title, newUrl);
      }
    } catch (error) {
      console.error('Error parsing KGWin data:', error);
    }
  }

  private showKGWinDataPopup(data: any) {
    // Create a modal popup with the KGWin data
    const modal = document.createElement('div');
    modal.className = 'modal fade show';
    modal.style.cssText = 'display: block; z-index: 9999;';
    modal.innerHTML = `
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <div class="modal-header bg-success text-white">
            <h5 class="modal-title">
              <i class="fas fa-desktop me-2"></i>
              Data Received from KGWin Desktop Application
            </h5>
            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
          </div>
          <div class="modal-body">
            <div class="row">
              <div class="col-md-6">
                <h6><i class="fas fa-info-circle me-2"></i>Application Info</h6>
                <ul class="list-unstyled">
                  <li><strong>Application:</strong> ${data.application}</li>
                  <li><strong>Version:</strong> ${data.version}</li>
                  <li><strong>Status:</strong> <span class="badge bg-success">${data.status}</span></li>
                  <li><strong>User:</strong> ${data.user}</li>
                  <li><strong>Machine:</strong> ${data.machine}</li>
                  <li><strong>Timestamp:</strong> ${data.timestamp}</li>
                </ul>
              </div>
              <div class="col-md-6">
                <h6><i class="fas fa-list me-2"></i>Features</h6>
                <ul class="list-unstyled">
                  ${data.features.map((feature: string) => `<li><i class="fas fa-check text-success me-2"></i>${feature}</li>`).join('')}
                </ul>
              </div>
            </div>
            <div class="mt-3">
              <h6><i class="fas fa-code me-2"></i>Raw JSON Data</h6>
              <pre class="bg-light p-3 rounded"><code>${JSON.stringify(data, null, 2)}</code></pre>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
            <button type="button" class="btn btn-primary" onclick="window.location.reload()">Refresh Page</button>
          </div>
        </div>
      </div>
    `;
    
    // Add backdrop
    const backdrop = document.createElement('div');
    backdrop.className = 'modal-backdrop fade show';
    backdrop.style.cssText = 'z-index: 9998;';
    
    document.body.appendChild(backdrop);
    document.body.appendChild(modal);
    
    // Add event listener to close modal
    const closeButtons = modal.querySelectorAll('[data-bs-dismiss="modal"]');
    closeButtons.forEach(button => {
      button.addEventListener('click', () => {
        document.body.removeChild(modal);
        document.body.removeChild(backdrop);
      });
    });
    
    // Auto-close after 10 seconds
    setTimeout(() => {
      if (document.body.contains(modal)) {
        document.body.removeChild(modal);
        document.body.removeChild(backdrop);
      }
    }, 10000);
  }
}
