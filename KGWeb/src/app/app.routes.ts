import { Routes } from '@angular/router';
import { Home } from './home/home';
import { ChatComponent } from './chat/chat.component';
import { ExtCommComponent } from './ext/ext.component';

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'signalr', component: ChatComponent },
  { path: 'ext', component: ExtCommComponent },
  { path: 'chat', redirectTo: 'signalr' },
  { path: '**', redirectTo: '' }
];
