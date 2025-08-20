import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';

export interface AssetLocation {
  id: string;
  name: string;
  type: string;
  latitude: number;
  longitude: number;
  status: 'active' | 'inactive' | 'maintenance';
  description?: string;
  lastUpdated: Date;
  properties?: { [key: string]: any };
}

export interface MapMarker {
  id: string;
  assetId: string;
  latitude: number;
  longitude: number;
  markerType: 'asset' | 'incident' | 'maintenance' | 'custom';
  title: string;
  description?: string;
  color?: string;
  icon?: string;
  timestamp: Date;
}

export interface SignalRMessage {
  user: string;
  message: string;
  timestamp: Date;
}

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: HubConnection;
  private connectionStatus = new BehaviorSubject<boolean>(false);
  private assetLocationsSubject = new BehaviorSubject<AssetLocation[]>([]);
  private mapMarkersSubject = new BehaviorSubject<MapMarker[]>([]);
  private messagesSubject = new BehaviorSubject<SignalRMessage[]>([]);
  private autoReconnect = true;
  private reconnectInterval = 5000; // 5 seconds

  public connectionStatus$ = this.connectionStatus.asObservable();
  public assetLocations$ = this.assetLocationsSubject.asObservable();
  public mapMarkers$ = this.mapMarkersSubject.asObservable();
  public messages$ = this.messagesSubject.asObservable();

  constructor() {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl('http://localhost:5000/communicationHub') // Updated to match KGWin's CommunicationHub
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect([0, 2000, 10000, 30000]) // Auto-reconnect with exponential backoff
      .build();

    this.setupConnectionHandlers();
    this.autoConnect();
  }

  private setupConnectionHandlers(): void {
    // Handle connection events
    this.hubConnection.onclose((error) => {
      console.log('SignalR connection closed:', error);
      this.connectionStatus.next(false);
      
      // Auto-reconnect if enabled
      if (this.autoReconnect) {
        setTimeout(() => {
          console.log('Attempting to reconnect...');
          this.startConnection();
        }, this.reconnectInterval);
      }
    });

    // Handle reconnection events
    this.hubConnection.onreconnecting((error) => {
      console.log('SignalR reconnecting:', error);
      this.connectionStatus.next(false);
    });

    this.hubConnection.onreconnected((connectionId) => {
      console.log('SignalR reconnected with connection ID:', connectionId);
      this.connectionStatus.next(true);
    });

    // Handle messages from WPF app
    this.hubConnection.on('ReceiveMessage', (user: string, message: string) => {
      console.log('Received message:', user, message);
      const newMessage: SignalRMessage = {
        user,
        message,
        timestamp: new Date()
      };
      
      const currentMessages = this.messagesSubject.value;
      const updatedMessages = [...currentMessages, newMessage];
      this.messagesSubject.next(updatedMessages);
    });

    // Handle asset location updates from WPF app
    this.hubConnection.on('ReceiveAssetLocations', (assets: AssetLocation[]) => {
      console.log('Received asset locations:', assets);
      this.assetLocationsSubject.next(assets);
    });

    // Handle map markers from WPF app
    this.hubConnection.on('ReceiveMapMarkers', (markers: MapMarker[]) => {
      console.log('Received map markers:', markers);
      this.mapMarkersSubject.next(markers);
    });

    // Handle individual asset updates
    this.hubConnection.on('ReceiveAssetUpdate', (asset: AssetLocation) => {
      console.log('Received asset update:', asset);
      const currentAssets = this.assetLocationsSubject.value;
      const updatedAssets = currentAssets.map(a => a.id === asset.id ? asset : a);
      this.assetLocationsSubject.next(updatedAssets);
    });

    // Handle connection status updates
    this.hubConnection.on('ConnectionStatus', (status: boolean) => {
      console.log('Connection status update:', status);
      this.connectionStatus.next(status);
    });
  }

  private async autoConnect(): Promise<void> {
    try {
      await this.startConnection();
    } catch (error) {
      console.log('Auto-connect failed, will retry:', error);
      // Retry after a delay
      setTimeout(() => {
        this.autoConnect();
      }, this.reconnectInterval);
    }
  }

  public async startConnection(): Promise<void> {
    try {
      await this.hubConnection.start();
      console.log('SignalR connection established');
      this.connectionStatus.next(true);
    } catch (error) {
      console.error('Error starting SignalR connection:', error);
      this.connectionStatus.next(false);
      throw error;
    }
  }

  public async stopConnection(): Promise<void> {
    this.autoReconnect = false; // Disable auto-reconnect when manually stopping
    try {
      await this.hubConnection.stop();
      console.log('SignalR connection stopped');
      this.connectionStatus.next(false);
    } catch (error) {
      console.error('Error stopping SignalR connection:', error);
    }
  }

  public enableAutoReconnect(): void {
    this.autoReconnect = true;
  }

  public disableAutoReconnect(): void {
    this.autoReconnect = false;
  }

  public setReconnectInterval(interval: number): void {
    this.reconnectInterval = interval;
  }

  /**
   * Get the current connection status
   */
  public getConnectionStatus(): boolean {
    return this.connectionStatus.value;
  }

  // Send asset location data to WPF app
  public async sendAssetLocation(asset: AssetLocation): Promise<void> {
    try {
      await this.hubConnection.invoke('SendAssetLocation', asset);
      console.log('Asset location sent:', asset);
    } catch (error) {
      console.error('Error sending asset location:', error);
    }
  }

  // Send multiple asset locations
  public async sendAssetLocations(assets: AssetLocation[]): Promise<void> {
    try {
      await this.hubConnection.invoke('SendAssetLocations', assets);
      console.log('Asset locations sent:', assets);
    } catch (error) {
      console.error('Error sending asset locations:', error);
    }
  }

  // Send map marker data
  public async sendMapMarker(marker: MapMarker): Promise<void> {
    try {
      await this.hubConnection.invoke('SendMapMarker', marker);
      console.log('Map marker sent:', marker);
    } catch (error) {
      console.error('Error sending map marker:', error);
    }
  }

  // Send multiple map markers
  public async sendMapMarkers(markers: MapMarker[]): Promise<void> {
    try {
      await this.hubConnection.invoke('SendMapMarkers', markers);
      console.log('Map markers sent:', markers);
    } catch (error) {
      console.error('Error sending map markers:', error);
    }
  }

  // Request asset locations from WPF app
  public async requestAssetLocations(): Promise<void> {
    try {
      await this.hubConnection.invoke('RequestAssetLocations');
      console.log('Requested asset locations');
    } catch (error) {
      console.error('Error requesting asset locations:', error);
    }
  }

  // Request map markers from WPF app
  public async requestMapMarkers(): Promise<void> {
    try {
      await this.hubConnection.invoke('RequestMapMarkers');
      console.log('Requested map markers');
    } catch (error) {
      console.error('Error requesting map markers:', error);
    }
  }

  public async joinAssetGroup(groupName: string): Promise<void> {
    try {
      await this.hubConnection.invoke('JoinAssetGroup', groupName);
      console.log('Joined asset group:', groupName);
    } catch (error) {
      console.error('Error joining asset group:', error);
    }
  }

  public async leaveAssetGroup(groupName: string): Promise<void> {
    try {
      await this.hubConnection.invoke('LeaveAssetGroup', groupName);
      console.log('Left asset group:', groupName);
    } catch (error) {
      console.error('Error leaving asset group:', error);
    }
  }

  public isConnected(): boolean {
    return this.hubConnection.state === 'Connected';
  }

  public getConnectionState(): string {
    return this.hubConnection.state;
  }

  public get connection(): HubConnection {
    return this.hubConnection;
  }

  public getMessages(): SignalRMessage[] {
    return this.messagesSubject.value;
  }

  public clearMessages(): void {
    this.messagesSubject.next([]);
  }

  // Register client name with the hub
  public async registerClientName(name: string): Promise<void> {
    try {
      await this.hubConnection.invoke('RegisterClientName', name);
      console.log('Client name registered:', name);
    } catch (error) {
      console.error('Error registering client name:', error);
    }
  }
}
