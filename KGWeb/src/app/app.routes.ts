import { Routes } from '@angular/router';
import { Home } from './home/home';
import { SignalrComponent } from './signalr/signalr.component';
import { ExtCommComponent } from './ext/ext.component';

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'signalr', component: SignalrComponent },
  { path: 'ext', component: ExtCommComponent },
  { path: 'chat', redirectTo: 'signalr' },
  { path: '**', redirectTo: '' }
];
