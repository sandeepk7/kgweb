import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface MapPopupData {
  id: string;
  type: string;
  title: string;
  color: string;
  longitude: number;
  latitude: number;
  description: string;
  status: string;
  notes: string;
}

@Component({
  selector: 'app-map-popup',
  templateUrl: './map-popup.component.html',
  styleUrls: ['./map-popup.component.css'],
  standalone: true,
  imports: [CommonModule, FormsModule]
})
export class MapPopupComponent {
  @Input() isVisible: boolean = false;
  @Input() popupData: MapPopupData = {
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

  @Output() close = new EventEmitter<void>();
  @Output() save = new EventEmitter<MapPopupData>();

  // Local copy for editing
  editedData: MapPopupData = { ...this.popupData };

  ngOnChanges(): void {
    if (this.popupData) {
      this.editedData = { ...this.popupData };
    }
  }

  onClose(): void {
    this.close.emit();
  }

  onSave(): void {
    this.save.emit(this.editedData);
  }

  onCancel(): void {
    this.editedData = { ...this.popupData };
    this.close.emit();
  }

    onOpenWpfMap(): void {
    try {
      // Create the popup data to pass to the WPF application
      const popupData = {
        id: this.editedData.id,
        type: this.editedData.type,
        title: this.editedData.title,
        color: this.editedData.color,
        longitude: this.editedData.longitude,
        latitude: this.editedData.latitude,
        description: this.editedData.description,
        status: this.editedData.status,
        notes: this.editedData.notes
      };

      // Convert to JSON
      const jsonData = JSON.stringify(popupData);
      
      // Call the WPF method
      if (typeof (window as any).communicationService !== 'undefined') {
        (window as any).communicationService.openWpfMapPopup(jsonData);
      } else {
        alert('WPF communication service not available.');
      }
    } catch (error) {
      console.error('Error opening WPF map:', error);
      alert('Error opening WPF map. Please make sure the WPF application is installed and running.');
    }
  }

  
}



