import { Component, OnInit, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MapService } from './map.service';
import { MapPopupComponent, MapPopupData } from './map-popup.component';
import { CommunicationComponent } from '../communication/communication.component';
import { CommunicationService } from '../communication/communication.service';

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

  constructor(private mapService: MapService, private communicationService: CommunicationService) { }

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
    this.communicationService.openNaperville();
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
