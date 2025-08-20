import { Component, OnInit, OnDestroy, NgZone, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

type PendingResolver = {
  resolve: (value: any) => void;
  reject: (reason?: any) => void;
  timeout: any;
}

interface ChatUser {
  id: string;
  name: string;
  clientId: string;
  isSelf: boolean;
}

interface ChatMessage {
  id: string;
  fromId: string;
  fromName: string;
  toId?: string;
  content: string;
  isJson: boolean;
  timestamp: Date;
  isFromSelf: boolean;
  type?: string; // Add type property for system messages
}

@Component({
  selector: 'app-ext-comm',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ext.component.html',
  styleUrls: ['./ext.component.css']
})
export class ExtCommComponent implements OnInit, OnDestroy {
  extensionDetected = false;
  kgwinConnected = false;
  status = 'Not connected';
  private requestCounter = 0;
  private pending: Map<string, PendingResolver> = new Map();

  // Chat properties
  clientName = 'KGWeb';
  isRegistered = false;
  connectedUsers: ChatUser[] = [];
  selectedUser: ChatUser | null = null;
  chatMessages: ChatMessage[] = [];
  messageInput = '';
  private detectionTimer: any = null;
  private kgwinStatusTimer: any = null;
  // Hide manual registration UI – auto register instead
  showRegisterUi = false;

  constructor(private zone: NgZone, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    window.addEventListener('message', this.onWindowMessage);
    this.pingExtension();
  }

  ngOnDestroy(): void {
    window.removeEventListener('message', this.onWindowMessage);
    this.pending.clear();
    if (this.detectionTimer) {
      clearInterval(this.detectionTimer);
      this.detectionTimer = null;
    }
    if (this.kgwinStatusTimer) {
      clearInterval(this.kgwinStatusTimer);
      this.kgwinStatusTimer = null;
    }
  }

  private onWindowMessage = (event: MessageEvent) => {
    if (event.source !== window || !event.data) return;
    const data = event.data;
    if (data.source !== 'KGCONNECT_EXT') return;

    console.log('KGWeb received message:', data);

    if (data.type === 'READY') {
      this.zone.run(() => {
        this.extensionDetected = true;
        this.status = 'Extension ready';
        this.cdr.markForCheck();
        console.log('Extension ready, attempting registration...');
        if (!this.isRegistered) {
          this.register();
        }
      });
      return;
    }

    if (data.type === 'RESPONSE' && data.requestId) {
      const key = String(data.requestId);
      const pending = this.pending.get(key);
      if (pending) {
        clearTimeout(pending.timeout);
        this.pending.delete(key);
        // Resolve inside Angular zone so awaiting code triggers change detection
        this.zone.run(() => {
          console.log('Received RESPONSE:', data.payload);
          pending.resolve(data.payload);
          this.cdr.markForCheck();
        });
      }
    }

    if (data.type === 'CTRL_RESPONSE' && data.requestId) {
      const key = String(data.requestId);
      const pending = this.pending.get(key);
      if (pending) {
        clearTimeout(pending.timeout);
        this.pending.delete(key);
        this.zone.run(() => {
          console.log('Received CTRL_RESPONSE:', data.payload);
          pending.resolve(data.payload);
          this.cdr.markForCheck();
        });
      }
    }

    if (data.type === 'CHAT_DELIVER') {
      this.zone.run(() => {
        console.log('Received CHAT_DELIVER:', data.payload);
        this.handleChatMessage(data.payload);
        this.cdr.markForCheck();
      });
    }
  };

  private send(action: string, payload?: any, timeoutMs = 5000): Promise<any> {
    const requestId = `${Date.now()}-${++this.requestCounter}`;
    const message = { source: 'KGWEB', type: 'KGCONNECT_PAGE_REQUEST', action, requestId, payload };
    console.log('KGWeb sending message:', message);
    window.postMessage(message, '*');
    return new Promise((resolve, reject) => {
      const t = setTimeout(() => {
        this.pending.delete(requestId);
        reject(new Error('Extension timed out'));
      }, timeoutMs);
      this.pending.set(requestId, { resolve, reject, timeout: t });
    });
  }

  private sendCtrl(type: string, payload?: any, timeoutMs = 5000): Promise<any> {
    const requestId = `${Date.now()}-${++this.requestCounter}`;
    const message = { source: 'KGWEB', type: 'KGCONNECT_CTRL', requestId, payload: { type, ...payload } };
    console.log('KGWeb sending CTRL message:', message);
    window.postMessage(message, '*');
    return new Promise((resolve, reject) => {
      const t = setTimeout(() => {
        this.pending.delete(requestId);
        reject(new Error('Extension timed out'));
      }, timeoutMs);
      this.pending.set(requestId, { resolve, reject, timeout: t });
    });
  }

  async pingExtension() {
    this.status = 'Pinging extension...';
    try {
      const result = await this.send('PING');
      this.zone.run(() => {
        this.extensionDetected = !!result?.ok;
        this.status = this.extensionDetected ? 'Connected' : 'Extension not responding';
        this.register();
        this.cdr.markForCheck();        
      });
    } catch (error: any) {
      this.zone.run(() => {
        this.extensionDetected = false;
        this.status = 'Extension not detected';
        this.cdr.markForCheck();
      });
    }
    // Start a short-lived retry loop until connected
    if (!this.extensionDetected && !this.detectionTimer) {
      let attempts = 0;
      this.detectionTimer = setInterval(async () => {
        attempts++;
        try {
          const result = await this.send('PING');
          if (result?.ok) {
            this.zone.run(() => {
              this.extensionDetected = true;
              this.status = 'Connected';
              this.cdr.markForCheck();
            });
            clearInterval(this.detectionTimer);
            this.detectionTimer = null;
          }
        } catch {}
        if (attempts >= 10 && this.detectionTimer) {
          clearInterval(this.detectionTimer);
          this.detectionTimer = null;
        }
      }, 1000);
    }
  }

  async register() {
    try {
      this.status = 'Registering...';
      const result = await this.sendCtrl('REGISTER', { name: this.clientName });
      
      if (result?.ok) {
        this.isRegistered = true;
        this.connectedUsers = result.users || [];
        this.status = `Registered as ${this.clientName}`;
        
        // Start checking KGWin status
        this.startKGWinStatusTimer();
        
        // Don't add KGWin as a user - KGWin is the receiver, not a user in the list
        // Just ensure we're registered and can send messages
        
        console.log('Registration successful:', result);
      } else {
        this.status = result?.error || 'Registration failed';
        console.error('Registration failed:', result);
      }
    } catch (error: any) {
      this.status = `Registration error: ${error.message}`;
      console.error('Registration error:', error);
    }
  }

  async refreshUsers() {
    try {
      this.status = 'Refreshing users...';
      const result = await this.sendCtrl('GET_USERS');
      this.connectedUsers = result?.users || [];
      
      // Don't add KGWin to users list - it's the receiver
      this.status = 'Users refreshed';
    } catch (error: any) {
      this.status = `Refresh error: ${error.message}`;
    }
  }

  selectUser(user: ChatUser) {
    this.selectedUser = user;
    this.loadChatHistory();
  }

  async sendMessage() {
    if (!this.isRegistered || !this.messageInput.trim()) {
      console.log('Cannot send message:', { isRegistered: this.isRegistered, hasInput: !!this.messageInput.trim() });
      return;
    }

    try {
      const isJson = this.isJsonContent(this.messageInput);
      this.status = 'Sending message...';
      
      // Send to KGWin directly (no user selection needed)
      const result = await this.sendCtrl('SEND_MESSAGE', {
        content: this.messageInput.trim(),
        isJson,
        toClientId: 'kgwin-client' // Send to KGWin
      });

      if (result?.ok) {
        // Add message to chat
        const message: ChatMessage = {
          id: result.message?.id || Date.now().toString(),
          fromId: 'self',
          fromName: this.clientName,
          toId: 'kgwin-client',
          content: this.messageInput.trim(),
          isJson,
          timestamp: new Date(),
          isFromSelf: true
        };

        this.chatMessages.push(message);
        this.messageInput = '';
        this.status = 'Message sent';
        console.log('Message sent successfully:', message);
      } else {
        this.status = result?.error || 'Send failed';
        console.error('Send failed:', result);
      }
    } catch (error: any) {
      this.status = `Send error: ${error.message}`;
      console.error('Send error:', error);
    }
  }

  async getHistory() {
    try {
      this.status = 'Loading history...';
      const result = await this.sendCtrl('GET_HISTORY', { withClientId: 'kgwin-client' });
      
      if (result?.messages) {
        this.chatMessages = result.messages.map((m: any) => ({
          id: m.id,
          fromId: m.fromClientId,
          fromName: m.fromName,
          toId: m.toClientId,
          content: m.content,
          isJson: m.isJson,
          timestamp: new Date(m.timestamp),
          isFromSelf: m.fromClientId === 'self'
        }));
        this.status = 'History loaded';
      }
    } catch (error: any) {
      this.status = `History error: ${error.message}`;
    }
  }

  insertTestJson() {
    const testJson = {
      type: 'asset',
      id: Date.now().toString(),
      name: 'Test Asset',
      location: { lat: 40.7128, lng: -74.0060 },
      properties: { status: 'active', priority: 'high' }
    };
    this.messageInput = JSON.stringify(testJson, null, 2);
  }

  insertTestMarkerJson() {
    const testJson = {
      type: 'marker',
      id: Date.now().toString(),
      name: 'Test Marker',
      position: { lat: 40.7128, lng: -74.0060 },
      properties: { 
        type: 'point',
        color: '#ff0000',
        size: 'medium'
      }
    };
    this.messageInput = JSON.stringify(testJson, null, 2);
  }

  clearChat() {
    this.chatMessages = [];
    this.status = 'Chat cleared';
  }

  onEnterPress(event: any) {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  private handleChatMessage(payload: any) {
    const message: ChatMessage = {
      id: payload.id,
      fromId: payload.fromClientId,
      fromName: payload.fromName,
      toId: payload.toClientId,
      content: payload.content,
      isJson: payload.isJson,
      timestamp: new Date(payload.timestamp),
      isFromSelf: false
    };

    // Add to users if not exists
    const existingUser = this.connectedUsers.find(u => u.id === payload.fromClientId);
    if (!existingUser) {
      const newUser: ChatUser = {
        id: payload.fromClientId,
        name: payload.fromName,
        clientId: payload.fromClientId,
        isSelf: false
      };
      this.connectedUsers.push(newUser);
    }

    this.chatMessages.push(message);
  }

  private loadChatHistory() {
    if (this.selectedUser) {
      this.getHistory();
    }
  }

  private isJsonContent(content: string): boolean {
    try {
      JSON.parse(content);
      return true;
    } catch {
      return false;
    }
  }

  async checkKGWinStatus() {
    if (!this.extensionDetected || !this.isRegistered) {
      this.kgwinConnected = false;
      return;
    }

    try {
      const result = await this.sendCtrl('PING');
      this.zone.run(() => {
        this.kgwinConnected = result?.ok || false;
        this.cdr.markForCheck();
      });
    } catch (error) {
      this.zone.run(() => {
        this.kgwinConnected = false;
        this.cdr.markForCheck();
      });
    }
  }

  private startKGWinStatusTimer() {
    if (this.kgwinStatusTimer) {
      clearInterval(this.kgwinStatusTimer);
    }
    this.kgwinStatusTimer = setInterval(() => {
      this.checkKGWinStatus();
    }, 3000); // Check every 3 seconds
  }
}



