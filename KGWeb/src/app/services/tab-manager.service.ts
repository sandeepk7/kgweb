import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class TabManagerService {
  private readonly TAB_ID_KEY = 'kgweb_tab_id';
  private readonly TAB_TIMESTAMP_KEY = 'kgweb_tab_timestamp';
  private readonly TAB_CHECK_INTERVAL = 2000; // 2 seconds
  private tabId: string;
  private heartbeatInterval: any;

  constructor() {
    this.tabId = this.generateTabId();
    this.initializeTabTracking();
  }

  private generateTabId(): string {
    return `kgweb_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  private initializeTabTracking(): void {
    // Set this tab's ID in localStorage
    localStorage.setItem(this.TAB_ID_KEY, this.tabId);
    localStorage.setItem(this.TAB_TIMESTAMP_KEY, Date.now().toString());

    // Start heartbeat to keep this tab "alive"
    this.startHeartbeat();

    // Check for existing tabs on page load
    this.checkForExistingTabs();

    // Listen for storage changes (when other tabs update localStorage)
    window.addEventListener('storage', (event) => {
      if (event.key === this.TAB_ID_KEY || event.key === this.TAB_TIMESTAMP_KEY) {
        this.checkForExistingTabs();
      }
    });

    // Listen for page visibility changes
    document.addEventListener('visibilitychange', () => {
      if (document.hidden) {
        this.stopHeartbeat();
      } else {
        this.startHeartbeat();
        this.checkForExistingTabs();
      }
    });

    // Clean up on page unload
    window.addEventListener('beforeunload', () => {
      this.cleanup();
    });
  }

  private startHeartbeat(): void {
    this.heartbeatInterval = setInterval(() => {
      localStorage.setItem(this.TAB_TIMESTAMP_KEY, Date.now().toString());
    }, this.TAB_CHECK_INTERVAL);
  }

  private stopHeartbeat(): void {
    if (this.heartbeatInterval) {
      clearInterval(this.heartbeatInterval);
      this.heartbeatInterval = null;
    }
  }

  private checkForExistingTabs(): void {
    const storedTabId = localStorage.getItem(this.TAB_ID_KEY);
    const storedTimestamp = localStorage.getItem(this.TAB_TIMESTAMP_KEY);
    
    if (!storedTabId || !storedTimestamp) {
      return;
    }

    const timestamp = parseInt(storedTimestamp);
    const now = Date.now();
    const timeDiff = now - timestamp;

    // If there's a recent timestamp (within 5 seconds) and it's not from this tab
    if (timeDiff < 5000 && storedTabId !== this.tabId) {
      console.log('Existing KGWeb tab detected, attempting to activate...');
      this.activateExistingTab();
    }
  }

  private activateExistingTab(): void {
    try {
      // Try to focus the window
      window.focus();
      
      // Show a notification to the user
      this.showTabExistsNotification();
      
      // Optionally, you could close this tab after a delay
      // setTimeout(() => {
      //   window.close();
      // }, 3000);
    } catch (error) {
      console.error('Error activating existing tab:', error);
    }
  }

  private showTabExistsNotification(): void {
    // Create a notification element
    const notification = document.createElement('div');
    notification.style.cssText = `
      position: fixed;
      top: 20px;
      right: 20px;
      background: #007ACC;
      color: white;
      padding: 15px 20px;
      border-radius: 8px;
      box-shadow: 0 4px 12px rgba(0,0,0,0.15);
      z-index: 10000;
      font-family: Arial, sans-serif;
      font-size: 14px;
      max-width: 300px;
      animation: slideIn 0.3s ease-out;
    `;
    
    notification.innerHTML = `
      <div style="display: flex; align-items: center; gap: 10px;">
        <span style="font-size: 16px;">🔗</span>
        <div>
          <div style="font-weight: bold; margin-bottom: 5px;">KGWeb Already Open</div>
          <div style="font-size: 12px; opacity: 0.9;">An existing KGWeb tab has been activated.</div>
        </div>
        <button onclick="this.parentElement.parentElement.remove()" 
                style="background: none; border: none; color: white; cursor: pointer; font-size: 18px; margin-left: 10px;">
          ×
        </button>
      </div>
    `;

    // Add CSS animation
    const style = document.createElement('style');
    style.textContent = `
      @keyframes slideIn {
        from { transform: translateX(100%); opacity: 0; }
        to { transform: translateX(0); opacity: 1; }
      }
    `;
    document.head.appendChild(style);

    document.body.appendChild(notification);

    // Auto-remove after 5 seconds
    setTimeout(() => {
      if (notification.parentElement) {
        notification.remove();
      }
    }, 5000);
  }

  private cleanup(): void {
    this.stopHeartbeat();
    
    // Only clear localStorage if this is the last tab
    const storedTabId = localStorage.getItem(this.TAB_ID_KEY);
    if (storedTabId === this.tabId) {
      localStorage.removeItem(this.TAB_ID_KEY);
      localStorage.removeItem(this.TAB_TIMESTAMP_KEY);
    }
  }

  // Public method to manually check for existing tabs
  public checkForExistingTabsManually(): void {
    this.checkForExistingTabs();
  }

  // Public method to get current tab ID
  public getCurrentTabId(): string {
    return this.tabId;
  }
}
