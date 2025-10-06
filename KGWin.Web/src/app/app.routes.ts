import { Routes } from '@angular/router';
import { WorkOrderComponent } from './modules/work-order/work-order.component';
import { MapComponent } from './modules/map/map.component';
import { SwitchWpfComponent } from './modules/switch-wpf/switch-wpf.component';

export const routes: Routes = [
  { path: '', component: MapComponent },
  { path: 'work-order', component: WorkOrderComponent },
  { path: 'switch-wpf', component: SwitchWpfComponent },
  { path: '**', redirectTo: '/' }
];
