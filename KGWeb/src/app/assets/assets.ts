import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SignalRService, AssetLocation, MapMarker, SignalRMessage } from '../services/signalr';
import { MessagePopup, MessageData } from '../components/message-popup/message-popup';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-assets',
  standalone: true,
  imports: [CommonModule, FormsModule, MessagePopup],
  templateUrl: './assets.html',
  styleUrl: './assets.css'
})
export class Assets implements OnInit, OnDestroy {
  connectionStatus = false;
  connectionState = ''; // Added to show detailed connection state
  assets: AssetLocation[] = [];
  markers: MapMarker[] = [];
  messages: SignalRMessage[] = [];
  newAsset: AssetLocation = {
    id: '',
    name: '',
    type: '',
    latitude: 0,
    longitude: 0,
    status: 'active',
    description: '',
    lastUpdated: new Date()
  };

  // Message popup properties
  showMessagePopup = false;
  currentMessage: MessageData = {
    type: 'info',
    title: 'Message',
    content: '',
    timestamp: new Date()
  };

  private subscriptions: Subscription[] = [];

  constructor(private signalRService: SignalRService) {}

  ngOnInit(): void {
    this.subscriptions.push(
      this.signalRService.connectionStatus$.subscribe(status => {
        this.connectionStatus = status;
        // Auto-register this client name on (re)connection
        if (status) {
          void this.signalRService.registerClientName('KGWeb');
        }
      }),
      this.signalRService.assetLocations$.subscribe(assets => {
        this.assets = assets;
      }),
      this.signalRService.mapMarkers$.subscribe(markers => {
        this.markers = markers;
      }),
      this.signalRService.messages$.subscribe(messages => {
        this.messages = messages;
        // Show popup for new messages
        if (messages.length > 0) {
          const latestMessage = messages[messages.length - 1];
          this.showMessage(latestMessage);
        }
      })
    );

    // Update connection state periodically
    setInterval(() => {
      this.connectionState = this.signalRService.getConnectionState();
    }, 1000);
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    // Don't stop connection on destroy since it's auto-managed by the service
  }

  private showMessage(signalRMessage: SignalRMessage): void {
    // Determine message type based on content
    let messageType: 'info' | 'success' | 'warning' | 'error' = 'info';
    let title = signalRMessage.user;
    let data: any = null;

    // Parse message content to determine type and extract data
    if (signalRMessage.message.includes('JSON Data:')) {
      try {
        const jsonStart = signalRMessage.message.indexOf('JSON Data:') + 10;
        const jsonContent = signalRMessage.message.substring(jsonStart).trim();
        data = JSON.parse(jsonContent);
        messageType = 'success';
        title = 'JSON Data Received';
      } catch (e) {
        messageType = 'error';
        title = 'Invalid JSON Data';
      }
    } else if (signalRMessage.message.includes('Server Status:')) {
      messageType = 'info';
      title = 'Server Status';
    } else if (signalRMessage.message.includes('Error')) {
      messageType = 'error';
      title = 'Error Message';
    } else if (signalRMessage.message.includes('Success') || signalRMessage.message.includes('Sent')) {
      messageType = 'success';
      title = 'Success Message';
    }

    this.currentMessage = {
      type: messageType,
      title: title,
      content: signalRMessage.message,
      timestamp: signalRMessage.timestamp,
      data: data
    };

    this.showMessagePopup = true;
  }

  closeMessagePopup(): void {
    this.showMessagePopup = false;
  }

  async startConnection(): Promise<void> { await this.signalRService.startConnection(); }
  async stopConnection(): Promise<void> { await this.signalRService.stopConnection(); }
  async requestAssetData(): Promise<void> { 
    await this.signalRService.requestAssetLocations();
    await this.signalRService.requestMapMarkers();
  }

  async registerClientName(): Promise<void> {
    const name = prompt('Enter your name to register with the server:') || 'Anonymous User';
    await this.signalRService.registerClientName(name);
  }

  async addAsset(): Promise<void> {
    if (!this.newAsset.name || !this.newAsset.type) {
      alert('Please fill in all required fields');
      return;
    }

    // Generate ID if not provided
    if (!this.newAsset.id) {
      this.newAsset.id = this.generateId();
    }

    this.newAsset.lastUpdated = new Date();
    await this.signalRService.sendAssetLocation(this.newAsset);
    this.resetNewAssetForm();
  }

  async updateAssetStatus(asset: AssetLocation, status: 'active' | 'inactive' | 'maintenance'): Promise<void> {
    const updatedAsset = { ...asset, status, lastUpdated: new Date() };
    await this.signalRService.sendAssetLocation(updatedAsset);
  }

  async deleteAsset(asset: AssetLocation): Promise<void> {
    if (confirm(`Are you sure you want to delete asset "${asset.name}"?`)) {
      // Remove from local list
      this.assets = this.assets.filter(a => a.id !== asset.id);
      // Send updated list to server
      await this.signalRService.sendAssetLocations(this.assets);
    }
  }

  async addCustomMarker(): Promise<void> {
    const marker: MapMarker = {
      id: this.generateId(),
      assetId: this.newAsset.id || 'custom',
      latitude: this.newAsset.latitude,
      longitude: this.newAsset.longitude,
      markerType: 'custom',
      title: this.newAsset.name || 'Custom Marker',
      description: this.newAsset.description || 'Custom marker created from asset form',
      color: this.getStatusColor(this.newAsset.status),
      timestamp: new Date()
    };

    await this.signalRService.sendMapMarker(marker);
  }

  private generateId(): string {
    return 'id_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
  }

  private getStatusColor(status: string): string {
    switch (status) {
      case 'active': return '#28A745';
      case 'inactive': return '#6C757D';
      case 'maintenance': return '#FFC107';
      default: return '#007BFF';
    }
  }

  private resetNewAssetForm(): void {
    this.newAsset = {
      id: '',
      name: '',
      type: '',
      latitude: 0,
      longitude: 0,
      status: 'active',
      description: '',
      lastUpdated: new Date()
    };
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'active': return 'badge bg-success';
      case 'inactive': return 'badge bg-secondary';
      case 'maintenance': return 'badge bg-warning';
      default: return 'badge bg-info';
    }
  }

  getConnectionStateClass(): string {
    switch (this.connectionState) {
      case 'Connected': return 'text-success';
      case 'Connecting': return 'text-warning';
      case 'Reconnecting': return 'text-warning';
      case 'Disconnected': return 'text-danger';
      default: return 'text-muted';
    }
  }
}
