import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SignalRService, SignalRMessage, AssetLocation, MapMarker } from '../services/signalr';
import { Subscription } from 'rxjs';

export interface ChatMessage {
  content: string;
  timestamp: Date;
  type: 'text' | 'json' | 'system';
  sender: string;
  isFromMe: boolean;
}

@Component({
  selector: 'app-signalr',
  templateUrl: './signalr.component.html',
  styleUrls: ['./signalr.component.css'],
  standalone: true,
  imports: [CommonModule, FormsModule]
})
export class SignalrComponent implements OnInit, OnDestroy {
  connectionStatus = false;
  connectionState = 'Disconnected';
  chatMessages: ChatMessage[] = [];
  messageInput = '';
  clientName = 'KGWeb';
  private subscriptions: Subscription[] = [];
  private suppressNextAssetEcho = false;
  private suppressNextMarkerEcho = false;

  constructor(private signalRService: SignalRService) {}

  ngOnInit(): void {
    this.subscriptions.push(
      this.signalRService.connectionStatus$.subscribe(status => {
        this.connectionStatus = status;
        this.connectionState = status ? 'Connected' : 'Disconnected';
        if (status) {
          this.addSystemMessage('Connected to SignalR Hub', 'KGWin');
          this.signalRService.registerClientName(this.clientName);
        } else {
          this.addSystemMessage('Disconnected from SignalR Hub', 'KGWin');
        }
      }),

      this.signalRService.messages$.subscribe(messages => {
        if (messages.length > 0) {
          const lastMessage = messages[messages.length - 1];
          // Ignore echo of messages we just sent from this client
          if (lastMessage.user !== this.clientName) {
            let type: 'text' | 'json' = 'text';
            try {
              const trimmed = (lastMessage.message || '').trim();
              if (trimmed.startsWith('{') || trimmed.startsWith('[')) {
                JSON.parse(trimmed);
                type = 'json';
              }
            } catch {}
            this.addReceivedMessage(lastMessage.user, lastMessage.message, type);
          }
        }
      }),

      this.signalRService.assetLocations$.subscribe(assets => {
        if (assets.length > 0) {
          const lastAsset = assets[assets.length - 1];
          if (this.suppressNextAssetEcho) {
            this.suppressNextAssetEcho = false;
          } else {
            this.addReceivedMessage('KGWin', JSON.stringify(lastAsset, null, 2), 'json');
          }
        }
      }),

      this.signalRService.mapMarkers$.subscribe(markers => {
        if (markers.length > 0) {
          const lastMarker = markers[markers.length - 1];
          if (this.suppressNextMarkerEcho) {
            this.suppressNextMarkerEcho = false;
          } else {
            this.addReceivedMessage('KGWin', JSON.stringify(lastMarker, null, 2), 'json');
          }
        }
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  addSystemMessage(content: string, sender: string = 'KGWin'): void {
    const message: ChatMessage = {
      content,
      timestamp: new Date(),
      type: 'system',
      sender,
      isFromMe: false
    };
    this.chatMessages.push(message);
  }

  addSentMessage(content: string, type: 'text' | 'json' = 'text'): void {
    const message: ChatMessage = {
      content,
      timestamp: new Date(),
      type,
      sender: 'You',
      isFromMe: true
    };
    this.chatMessages.push(message);
  }

  addReceivedMessage(sender: string, content: string, type: 'text' | 'json' = 'text'): void {
    const message: ChatMessage = {
      content,
      timestamp: new Date(),
      type,
      sender,
      isFromMe: false
    };
    this.chatMessages.push(message);
  }

  async sendMessage(): Promise<void> {
    if (!this.messageInput.trim()) {
      return;
    }

    const content = this.messageInput.trim();
    
    try {
      // Determine special commands for asset/marker when input is valid JSON with a hint
      if (content.includes('"markerType"')) {
        const marker = JSON.parse(content);
        this.signalRService.sendMapMarker(marker);
        this.addSentMessage(JSON.stringify(marker, null, 2), 'json');
        this.suppressNextMarkerEcho = true;
        this.messageInput = '';
        return;
      }
      if (content.includes('"status"') || content.includes('"type"')) {
        const asset = JSON.parse(content);
        this.signalRService.sendAssetLocation(asset);
        this.addSentMessage(JSON.stringify(asset, null, 2), 'json');
        this.suppressNextAssetEcho = true;
        this.messageInput = '';
        return;
      }

      // Otherwise, send as JSON if valid; if not, send as plain text
      try {
        const parsedFallback = JSON.parse(content);
        const prettyFallback = JSON.stringify(parsedFallback);
        await this.signalRService.connection?.invoke('SendMessage', this.clientName, prettyFallback);
        this.addSentMessage(prettyFallback, 'json');
      } catch {
        await this.signalRService.connection?.invoke('SendMessage', this.clientName, content);
        this.addSentMessage(content, 'text');
      }
      
      this.messageInput = '';
    } catch (error) {
      alert('Invalid JSON format');
    }
  }

  async sendTestAsset(): Promise<void> {
    const testAsset: AssetLocation = {
      id: this.generateId(),
      name: 'Test Asset from KGWeb',
      type: 'pump',
      latitude: 40.7128,
      longitude: -74.0060,
      status: 'active',
      lastUpdated: new Date()
    };

    await this.signalRService.sendAssetLocation(testAsset);
    this.addSentMessage(JSON.stringify(testAsset, null, 2), 'json');
  }

  async sendTestMarker(): Promise<void> {
    const testMarker: MapMarker = {
      id: this.generateId(),
      assetId: this.generateId(),
      title: 'Test Marker from KGWeb',
      description: 'This is a test marker sent from the web chat',
      latitude: 40.7128,
      longitude: -74.0060,
      markerType: 'custom',
      color: '#FF0000',
      icon: '📍',
      timestamp: new Date()
    };

    await this.signalRService.sendMapMarker(testMarker);
    this.addSentMessage(JSON.stringify(testMarker, null, 2), 'json');
  }

  clearChat(): void {
    this.chatMessages = [];
    this.addSystemMessage('Chat history cleared');
  }

  onMessageTypeChange(): void { /* removed dropdown */ }

  insertTestAssetJson(): void {
    this.messageInput = JSON.stringify({
      id: 'asset-123',
      name: 'Test Asset',
      type: 'pump',
      latitude: 40.7128,
      longitude: -74.0060,
      status: 'Active',
      lastUpdated: new Date().toISOString()
    }, null, 2);
  }

  insertTestMarkerJson(): void {
    this.messageInput = JSON.stringify({
      id: 'marker-123',
      assetId: 'asset-123',
      title: 'Test Marker',
      description: 'Test description',
      latitude: 40.7128,
      longitude: -74.0060,
      markerType: 'custom',
      color: '#FF0000',
      icon: '📍',
      timestamp: new Date().toISOString()
    }, null, 2);
  }

  private generateId(): string {
    return Math.random().toString(36).substr(2, 9);
  }

  getConnectionStateClass(): string { return this.connectionStatus ? 'text-success' : 'text-danger'; }

  startConnection(): void {
    this.signalRService.startConnection();
  }

  stopConnection(): void {
    this.signalRService.stopConnection();
  }

  onEnterKey(event: Event): void {
    const keyboardEvent = event as KeyboardEvent;
    if (!keyboardEvent.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }
}
