import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class SwitchWpfService {

  constructor() { }

  switchToWpfMap(): void {
    try {
      // Check if the WPF communication service is available
      if ((window as any).communicationService && (window as any).communicationService.switchToWpfMap) {
        (window as any).communicationService.switchToWpfMap();
        console.log('Switching to WPF Map page...');
      } else {
        // Fallback: try to open WPF map popup as an alternative
        if ((window as any).communicationService && (window as any).communicationService.openWpfMapPopup) {
          const defaultData = {
            title: 'Switch to WPF Map',
            type: 'navigation',
            longitude: 0,
            latitude: 0,
            description: 'Switched from Angular application'
          };
          (window as any).communicationService.openWpfMapPopup(JSON.stringify(defaultData));
          console.log('Opening WPF map popup as fallback...');
        } else {
          console.error('WPF communication service not available');
          alert('WPF communication service not available. Please ensure the WPF application is running.');
        }
      }
    } catch (error) {
      console.error('Error switching to WPF Map:', error);
      alert('Error switching to WPF Map. Please try again.');
    }
  }

  closeAllWpfPopups(): void {
    try {
      if ((window as any).communicationService && (window as any).communicationService.closeAllPopups) {
        (window as any).communicationService.closeAllPopups();
        console.log('Requested WPF to close all popups.');
      } else {
        console.error('WPF communication service not available for closeAllPopups');
        alert('WPF communication service not available.');
      }
    } catch (error) {
      console.error('Error closing WPF popups:', error);
      alert('Error closing WPF popups. Please try again.');
    }
  }

  minimizeLastWpfPopup(): void {
    try {
      const cs = (window as any).communicationService;
      if (cs && cs.minimizeLastPopup) {
        cs.minimizeLastPopup();
        console.log('Requested WPF to minimize last popup.');
      } else {
        console.error('WPF communication service not available for minimizeLastPopup');
        alert('WPF communication service not available.');
      }
    } catch (error) {
      console.error('Error minimizing WPF popup:', error);
      alert('Error minimizing WPF popup. Please try again.');
    }
  }

  maximizeLastWpfPopup(): void {
    try {
      const cs = (window as any).communicationService;
      if (cs && cs.maximizeLastPopup) {
        cs.maximizeLastPopup();
        console.log('Requested WPF to maximize last popup.');
      } else {
        console.error('WPF communication service not available for maximizeLastPopup');
        alert('WPF communication service not available.');
      }
    } catch (error) {
      console.error('Error maximizing WPF popup:', error);
      alert('Error maximizing WPF popup. Please try again.');
    }
  }

  restoreLastWpfPopup(): void {
    try {
      const cs = (window as any).communicationService;
      if (cs && cs.restoreLastPopup) {
        cs.restoreLastPopup();
        console.log('Requested WPF to restore last popup.');
      } else {
        console.error('WPF communication service not available for restoreLastPopup');
        alert('WPF communication service not available.');
      }
    } catch (error) {
      console.error('Error restoring WPF popup:', error);
      alert('Error restoring WPF popup. Please try again.');
    }
  }
}
