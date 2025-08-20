import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { SignalRService } from './signalr';

export interface KGWinStatus {
  isRunning: boolean;
  isConnected: boolean;
  lastChecked: Date;
  error?: string;
}

@Injectable({
  providedIn: 'root'
})
export class KGWinService {
  private statusSubject = new BehaviorSubject<KGWinStatus>({
    isRunning: false,
    isConnected: false,
    lastChecked: new Date()
  });

  public status$ = this.statusSubject.asObservable();

  constructor(private signalRService: SignalRService) {
    // Subscribe to SignalR connection status
    this.signalRService.connectionStatus$.subscribe(isConnected => {
      const currentStatus = this.statusSubject.value;
      this.statusSubject.next({
        ...currentStatus,
        isConnected,
        lastChecked: new Date()
      });
    });
  }

  /**
   * Launch the KGWin desktop application using automatic methods
   */
  async launchKGWin(assetId?: string, layerId?: string): Promise<boolean> {
    try {
      // Try multiple launch methods for better browser compatibility
      const success = await this.tryLaunchMethods(assetId, layerId);
      
      if (!success) {
        throw new Error('All launch methods failed');
      }

      console.log('KGWin application launch initiated');
      
      // Update status
      const currentStatus = this.statusSubject.value;
      this.statusSubject.next({
        ...currentStatus,
        isRunning: true,
        lastChecked: new Date()
      });

      // Check connection status after a delay
      setTimeout(() => {
        this.checkStatus();
      }, 3000);

      return true;
    } catch (error) {
      console.error('Failed to launch KGWin:', error);
      this.updateStatusError('Failed to launch KGWin application');
      return false;
    }
  }

  /**
   * Check if KGWin application is running and accessible
   */
  async checkStatus(): Promise<KGWinStatus> {
    try {
      // Try to connect to SignalR hub to check if KGWin is running
      const response = await fetch('http://localhost:5000/communicationHub', {
        method: 'GET',
        mode: 'cors'
      });
      
      const isRunning = response.ok;
      const isConnected = this.signalRService.getConnectionStatus();
      
      const status: KGWinStatus = {
        isRunning,
        isConnected,
        lastChecked: new Date()
      };

      this.statusSubject.next(status);
      return status;
    } catch (error) {
      console.log('KGWin not running or not accessible:', error);
      const status: KGWinStatus = {
        isRunning: false,
        isConnected: false,
        lastChecked: new Date(),
        error: 'Application not accessible'
      };
      
      this.statusSubject.next(status);
      return status;
    }
  }

  /**
   * Get the current status without making a network request
   */
  getCurrentStatus(): KGWinStatus {
    return this.statusSubject.value;
  }

  /**
   * Update status with an error
   */
  private updateStatusError(error: string): void {
    const currentStatus = this.statusSubject.value;
    this.statusSubject.next({
      ...currentStatus,
      error,
      lastChecked: new Date()
    });
  }

  /**
   * Clear any error status
   */
  clearError(): void {
    const currentStatus = this.statusSubject.value;
    this.statusSubject.next({
      ...currentStatus,
      error: undefined
    });
  }

  /**
   * Try launching with custom protocol (requires registration)
   */
  private async tryLaunchMethods(assetId?: string, layerId?: string): Promise<boolean> {
    return await this.tryCustomProtocol(assetId, layerId);
  }

  /**
   * Try launching with custom protocol (requires registration)
   */
  private async tryCustomProtocol(assetId?: string, layerId?: string): Promise<boolean> {
    try {
      let protocolUrl = 'kgwin://launch';
      
      // Add parameters if provided
      if (assetId || layerId) {
        const params = new URLSearchParams();
        if (assetId) params.append('assetId', assetId);
        if (layerId) params.append('layerId', layerId);
        protocolUrl += '?' + params.toString();
      }
      
      window.location.href = protocolUrl;
      return true;
    } catch (error) {
      console.log('Custom protocol failed:', error);
      return false;
    }
  }



  /**
   * Check if the application is ready for communication
   */
  isReady(): boolean {
    const status = this.statusSubject.value;
    return status.isRunning && status.isConnected;
  }
}
