import { Component, OnInit, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MapService } from './map.service';
import { MapPopupComponent, MapPopupData } from './map-popup.component';
import { CommunicationComponent } from '../communication/communication.component';

@Component({
  selector: 'app-map',
  templateUrl: './map.component.html',
  styleUrls: ['./map.component.css'],
  standalone: true,
  imports: [CommonModule, MapPopupComponent, CommunicationComponent]
})
export class MapComponent implements OnInit, AfterViewInit {
  @ViewChild('mapViewDiv', { static: true }) private mapViewEl!: ElementRef;

  // Popup properties
  isPopupVisible = false;
  popupData: MapPopupData = {
    id: '',
    type: '',
    title: '',
    color: '',
    longitude: 0,
    latitude: 0,
    description: '',
    status: '',
    notes: ''
  };

  constructor(private mapService: MapService) {}

  ngOnInit(): void {
    // Component initialization
  }

  ngAfterViewInit(): void {
    this.initializeMap();
    this.setupPopupSubscription();
  }

  private async initializeMap(): Promise<void> {
    try {
      // Wait a bit for the container to be properly rendered
      await new Promise(resolve => setTimeout(resolve, 100));
      console.log('Map container element:', this.mapViewEl.nativeElement);
      await this.mapService.initializeMap(this.mapViewEl.nativeElement);
    } catch (error) {
      console.error('Failed to initialize map:', error);
    }
  }

  openNaperville(): void {
    try {
      // Check if the WPF communication service is available
      if ((window as any).communicationService && (window as any).communicationService.openWpfMapPopup) {
        const napervilleData = {
          title: 'Naperville Map',
          type: 'naperville',
          longitude: -88.1473,
          latitude: 41.7508,
          description: 'Naperville utility and infrastructure data'
        };
        (window as any).communicationService.openWpfMapPopup(JSON.stringify(napervilleData));
        console.log('Opening WPF Naperville popup...');
      } else {
        console.error('WPF communication service not available');
        alert('WPF communication service not available. Please ensure the WPF application is running.');
      }
    } catch (error) {
      console.error('Error opening WPF Naperville popup:', error);
      alert('Error opening WPF Naperville popup. Please try again.');
    }
  }

  showAssets(): void {
    this.mapService.showAssets();
  }

  showMarkers(): void {
    this.mapService.addRandomMapMarkers();
  }

  clearMap(): void {
    this.mapService.clearMap();
  }


  private setupPopupSubscription(): void {
    this.mapService.popupData$.subscribe(data => {
      if (data) {
        this.popupData = data;
        this.isPopupVisible = true;
      } else {
        this.isPopupVisible = false;
      }
    });
  }

  onPopupClose(): void {
    this.mapService.hidePopup();
  }

  onPopupSave(data: MapPopupData): void {
    console.log('Saving popup data:', data);
    // Here you would typically save the data to a service or API
    alert('Data saved successfully!');
    this.mapService.hidePopup();
  }
}
