import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable } from 'rxjs';
import { MapService } from '../map/map.service';

export interface CommunicationMessage {
  id: string;
  text: string;
  sender: 'user' | 'wpf' | 'system';
  timestamp: Date;
  isJson?: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class CommunicationService {
  private messagesSubject = new BehaviorSubject<CommunicationMessage[]>([]);
  private isConnectedSubject = new BehaviorSubject<boolean>(false);

  // Base URL of the Angular app used inside the WPF WebView
  // Update this if your hosted URL changes
  public readonly appUrl = 'http://localhost:4200/';

  public messages$ = this.messagesSubject.asObservable();
  public isConnected$ = this.isConnectedSubject.asObservable();

  constructor(private mapService: MapService, private router: Router) {
    this.setupCommunication();
    // When route changes to /work-order and context exists, show modal and clear it
    try {
      this.router.events.subscribe(() => {
        const url = (this.router as any).url || window.location.hash || '';
        if (url && (url.includes('/work-order') || url.endsWith('#/work-order'))) {
          const ctx = (window as any).__KG_CONTEXT__;
          if (ctx && ctx.fields) {
            // this.showModalForContext(ctx);
            // (window as any).__KG_CONTEXT__ = undefined;
          }
        }
      });
    } catch {}
  }

  openNaperville(): void {
    const napervilleData = {
      type: 'NapervillePopupWindow',
      data: null,
    };

    const webview = (window as any).chrome?.webview;
    if (webview?.postMessage) {
      webview.postMessage(napervilleData);
      console.log('Requested WPF Naperville popup via WebView2');
    } else if ((window as any).communicationService && (window as any).communicationService.openWpfMapPopup) {
      (window as any).communicationService.openWpfMapPopup(JSON.stringify(napervilleData));
      console.log('Opening WPF Naperville popup...');
    }
  }

  login(username:string): void {
    const userLoginData = {
      type: 'UserInfo',
      data: username,
    };

    const webview = (window as any).chrome?.webview;
    if (webview?.postMessage) {
      webview.postMessage(userLoginData);
      console.log('Requested WPF Naperville popup via WebView2');
    } else if ((window as any).communicationService && (window as any).communicationService.openWpfMapPopup) {
      (window as any).communicationService.openWpfMapPopup(JSON.stringify(userLoginData));
      console.log('Opening WPF Naperville popup...');
    }
  }

  private setupCommunication(): void {
    // Check if we're running in CefSharp environment
    this.checkCefSharpConnection();

    // Listen for messages from WPF
    window.addEventListener('message', (event) => {
      if (event.data && event.data.type === 'communication') {
        // First: try to interpret structured commands; do NOT echo to chat for these
        let handledCommand = false;
        try {
          const payload = JSON.parse(event.data.data);
          if (payload && payload.command === 'navigate') {
            this.handleNavigateCommand(payload);
            handledCommand = true;
          } else if (payload && payload.command === 'switchToCreateWorkOrder') {
            this.switchToCreateWorkOrder(payload.context);
            handledCommand = true;
          }
        } catch {
          /* not JSON or no command */
        }

        // Also support simple string command without adding to chat
        if (!handledCommand && event.data.data === 'switchToCreateWorkOrder') {
          this.switchToCreateWorkOrder();
          handledCommand = true;
        }

        // If not a command, treat it as a normal chat message
        if (!handledCommand) {
          this.addMessage(event.data.data, 'wpf');
        }
      }
    });

    // Also listen for CefSharp to become available
    window.addEventListener('load', () => {
      setTimeout(() => this.checkCefSharpConnection(), 1000);
    });

    // Check periodically for CefSharp connection
    setInterval(() => this.checkCefSharpConnection(), 2000);
  }

  private checkCefSharpConnection(): void {
    debugger;
    const w = window as any;
    const hasCommunicationService = typeof w.communicationService !== 'undefined';
    const hasCefSharp = typeof w.CefSharp !== 'undefined';

    console.log('Checking CefSharp connection:', {
      hasCommunicationService,
      hasCefSharp,
      isConnected: this.isConnectedSubject.value,
    });

    if (
      !hasCommunicationService &&
      hasCefSharp &&
      typeof w.CefSharp.BindObjectAsync === 'function'
    ) {
      // Try to bind the object dynamically (post-load binding)
      w.CefSharp.BindObjectAsync('communicationService')
        .then(() => {
          this.isConnectedSubject.next(true);
          this.addSystemMessage('Connected to WPF via CefSharp (async bind)!');
          console.log('CefSharp BindObjectAsync resolved, communicationService available');
        })
        .catch((err: any) => {
          console.warn('BindObjectAsync failed:', err);
        });
    } else if (hasCommunicationService && !this.isConnectedSubject.value) {
      this.isConnectedSubject.next(true);
      this.addSystemMessage('Connected to WPF via CefSharp!');
      console.log('CefSharp communicationService detected');
    } else if (!hasCommunicationService && this.isConnectedSubject.value) {
      this.isConnectedSubject.next(false);
      this.addSystemMessage('Disconnected from WPF');
      console.log('CefSharp communicationService not available');
    }
  }

  sendMessage(messageText: string): void {
    if (!messageText.trim()) return;

    this.addMessage(messageText.trim(), 'user');

    const w = window as any;
    const hasCommunicationService = typeof w.communicationService !== 'undefined';
    console.log('Sending message:', {
      messageText,
      hasCommunicationService,
      isConnected: this.isConnectedSubject.value,
    });

    // Send message to WPF via CefSharp
    if (hasCommunicationService && this.isConnectedSubject.value) {
      try {
        const svc = w.communicationService;

        // Debug: Log what methods are available
        console.log('Available methods on communicationService:', Object.getOwnPropertyNames(svc));
        console.log('sendMessage type:', typeof svc.sendMessage);
        console.log('sendCommunicationMessage type:', typeof svc.sendCommunicationMessage);

        // Candidate call strategies (try all possible method names)
        const strategies: Array<() => any> = [
          () =>
            typeof svc.sendMessage === 'function' && svc.sendMessage('communication', messageText),
          () =>
            typeof svc.sendCommunicationMessage === 'function' &&
            svc.sendCommunicationMessage(messageText),
        ];

        let attempted = 0;
        let succeeded = false;
        for (const invoke of strategies) {
          attempted++;
          const res = invoke();
          if (res) {
            if (typeof res.then === 'function') {
              res
                .then(() => console.log('Message sent to WPF (async):', messageText))
                .catch((e: any) => console.error('Async send failed:', e));
            } else {
              console.log('Message sent to WPF:', messageText);
            }
            succeeded = true;
            break;
          }
        }

        if (!succeeded) {
          console.error('No compatible send method found on communicationService');
          this.addSystemMessage('Send method not found on communicationService');
        }
      } catch (error) {
        console.error('Error sending message to WPF:', error);
        this.addSystemMessage('Error sending message to WPF');
      }
    } else {
      this.addSystemMessage(
        `Message sent locally (communicationService: ${hasCommunicationService}, isConnected: ${this.isConnectedSubject.value})`
      );
    }
  }

  addMessage(text: string, sender: 'user' | 'wpf'): void {
    // Check if the text is JSON and format it
    let formattedText = text;
    let isJson = false;
    let jsonObj: any = null;

    try {
      // Try to parse as JSON
      jsonObj = JSON.parse(text);
      formattedText = JSON.stringify(jsonObj, null, 2);
      isJson = true;

      // Check if it's assets or markers data and handle it
      if (jsonObj && (jsonObj.assets || jsonObj.markers)) {
        this.handleMapData(jsonObj);
      }
    } catch (e) {
      // Not JSON, use original text
      formattedText = text;
      isJson = false;
    }

    const message: CommunicationMessage = {
      id: Date.now().toString(),
      text: formattedText,
      sender,
      timestamp: new Date(),
      isJson: isJson,
    };

    const currentMessages = this.messagesSubject.value;
    this.messagesSubject.next([...currentMessages, message]);
    this.scrollToBottom();
  }

  private handleMapData(data: any): void {
    // Use the injected map service
    if (data.assets && Array.isArray(data.assets)) {
      this.mapService.addAssetsFromMessage(data.assets);
      this.addSystemMessage(`Added ${data.assets.length} assets to the map`);
    }

    if (data.markers && Array.isArray(data.markers)) {
      this.mapService.addMarkersFromMessage(data.markers);
      this.addSystemMessage(`Added ${data.markers.length} markers to the map`);
    }
  }

  private handleNavigateCommand(payload: any): void {
    // Expect payload: { command: 'navigate', route: string, context?: any }
    if (!payload || !payload.route) {
      return;
    }
    // Route handling: rely on Angular router if present, otherwise set hash
    const route: string = payload.route;
    try {
      // Prefer DI router when service constructed in Angular context
      if (this.router && typeof this.router.navigateByUrl === 'function') {
        this.router.navigateByUrl(route, { state: payload.context || {} });
        return;
      }
    } catch {}
    // Fallback: update URL hash so app can react
    window.location.hash = route.startsWith('#') ? route : '#' + route.replace(/^\//, '');
  }

  // Public API to switch to Create Work Order page
  switchToCreateWorkOrder(context?: any): void {
    // Ensure app URL is loaded when running inside WPF shell
    try {
      if (window.location.href.indexOf(this.appUrl) === -1) {
        // If app not loaded, navigate main window to the app URL
        window.location.href = this.appUrl + '#/work-order';
        return;
      }
    } catch {}

    // If app already loaded, route to /work-order
    this.handleNavigateCommand({ route: '/work-order', context });

    // Also expose the passed context globally so the /work-order page can read it
    try {
      (window as any).__KG_CONTEXT__ = context || {};
    } catch {}

    // If already on work-order, show modal immediately and clear context
    try {
      const url = (this.router as any).url || window.location.hash || '';
      if (url && (url.includes('/work-order') || url.endsWith('#/work-order'))) {
        const ctx = (window as any).__KG_CONTEXT__;
        if (ctx && ctx.fields) {
          // this.showModalForContext(ctx);
          // (window as any).__KG_CONTEXT__ = undefined;
        }
      }
    } catch {}
  }

  // Utility: show a lightweight modal with fields, then clear on close
  private showModalForContext(ctx: any): void {
    try {
      const fields = Array.isArray(ctx?.fields) ? ctx.fields : [];
      const title = ctx?.title || 'Work Order Context';
      const modal = document.createElement('div');
      modal.style.position = 'fixed';
      modal.style.left = '0';
      modal.style.top = '0';
      modal.style.right = '0';
      modal.style.bottom = '0';
      modal.style.background = 'rgba(0,0,0,0.5)';
      modal.style.zIndex = '9999';

      const box = document.createElement('div');
      box.style.position = 'absolute';
      box.style.left = '50%';
      box.style.top = '50%';
      box.style.transform = 'translate(-50%, -50%)';
      box.style.background = '#fff';
      box.style.borderRadius = '8px';
      box.style.width = '600px';
      box.style.maxHeight = '70vh';
      box.style.overflow = 'auto';
      box.style.boxShadow = '0 10px 30px rgba(0,0,0,0.2)';
      box.style.padding = '16px 16px 8px 16px';

      const header = document.createElement('div');
      header.style.display = 'flex';
      header.style.justifyContent = 'space-between';
      header.style.alignItems = 'center';
      header.style.marginBottom = '8px';
      const h = document.createElement('h3');
      h.textContent = title;
      h.style.margin = '0';
      const close = document.createElement('button');
      close.textContent = '✕';
      close.style.border = 'none';
      close.style.background = 'transparent';
      close.style.fontSize = '18px';
      close.style.cursor = 'pointer';
      close.onclick = () => document.body.removeChild(modal);
      header.appendChild(h);
      header.appendChild(close);
      box.appendChild(header);

      const list = document.createElement('div');
      for (const f of fields) {
        const row = document.createElement('div');
        row.style.display = 'grid';
        row.style.gridTemplateColumns = '1fr 16px 2fr';
        row.style.gap = '8px';
        row.style.padding = '6px 0';
        const l = document.createElement('div');
        l.textContent = f.label ?? '';
        l.style.color = '#6B7280';
        l.style.fontSize = '12px';
        const v = document.createElement('div');
        v.textContent = f.value ?? '';
        v.style.textAlign = 'right';
        v.style.fontSize = '14px';
        v.style.color = '#111827';
        const spacer = document.createElement('div');
        row.appendChild(l);
        row.appendChild(spacer);
        row.appendChild(v);
        list.appendChild(row);
      }
      box.appendChild(list);

      modal.appendChild(box);
      modal.addEventListener('click', (e) => {
        if (e.target === modal) document.body.removeChild(modal);
      });
      document.body.appendChild(modal);
    } catch (e) {
      console.warn('Failed to show modal for context:', e);
    }
  }

  addSystemMessage(text: string): void {
    // Only log to console, don't add to communication messages
    console.log(`[System] ${text}`);
  }

  clearCommunication(): void {
    this.messagesSubject.next([]);
    this.addSystemMessage('Communication cleared');
  }

  addRandomAssets(): string {
    const randomAssets = {
      assets: [
        {
          id: 'asset_001',
          name: 'Excavator',
          type: 'Equipment',
          status: 'Active',
          location: {
            longitude: -118.2437 + (Math.random() - 0.5) * 0.8,
            latitude: 34.0522 + (Math.random() - 0.5) * 0.6,
          },
        },
        {
          id: 'asset_002',
          name: 'Dump Truck',
          type: 'Vehicle',
          status: 'Active',
          location: {
            longitude: -118.2437 + (Math.random() - 0.5) * 0.8,
            latitude: 34.0522 + (Math.random() - 0.5) * 0.6,
          },
        },
      ],
    };

    const jsonString = JSON.stringify(randomAssets, null, 2);
    this.addSystemMessage('Sample assets JSON added to input');
    return jsonString;
  }

  addRandomMarkers(): string {
    const randomMarkers = {
      markers: [
        {
          id: 'marker_001',
          title: 'Construction Site',
          type: 'POI',
          location: {
            longitude: -118.2437 + (Math.random() - 0.5) * 0.8,
            latitude: 34.0522 + (Math.random() - 0.5) * 0.6,
          },
        },
        {
          id: 'marker_002',
          title: 'Equipment Location',
          type: 'Task',
          location: {
            longitude: -118.2437 + (Math.random() - 0.5) * 0.8,
            latitude: 34.0522 + (Math.random() - 0.5) * 0.6,
          },
        },
      ],
    };

    const jsonString = JSON.stringify(randomMarkers, null, 2);
    this.addSystemMessage('Sample markers JSON added to input');
    return jsonString;
  }

  private scrollToBottom(): void {
    setTimeout(() => {
      const messagesArea = document.querySelector('.messages-area');
      if (messagesArea) {
        messagesArea.scrollTop = messagesArea.scrollHeight;
      }
    }, 100);
  }

  getMessages(): CommunicationMessage[] {
    return this.messagesSubject.value;
  }

  getIsConnected(): boolean {
    return this.isConnectedSubject.value;
  }
}
