import { Injectable } from '@angular/core';
import Map from '@arcgis/core/Map';
import MapView from '@arcgis/core/views/MapView';
import GraphicsLayer from '@arcgis/core/layers/GraphicsLayer';
import Graphic from '@arcgis/core/Graphic';
import Point from '@arcgis/core/geometry/Point';
import SimpleMarkerSymbol from '@arcgis/core/symbols/SimpleMarkerSymbol';
import PictureMarkerSymbol from '@arcgis/core/symbols/PictureMarkerSymbol';
import { BehaviorSubject } from 'rxjs';
import { MapPopupData } from './map-popup.component';

@Injectable({
  providedIn: 'root'
})
export class MapService {
  private map!: Map;
  private mapView!: MapView;
  private markersLayer!: GraphicsLayer;
  private assetsLayer!: GraphicsLayer;
  
  // Popup observable
  private popupDataSubject = new BehaviorSubject<MapPopupData | null>(null);
  public popupData$ = this.popupDataSubject.asObservable();

  initializeMap(container: HTMLDivElement): Promise<void> {
    return new Promise((resolve, reject) => {
      try {
        console.log('Initializing map with container:', container);
        console.log('Container dimensions:', container.offsetWidth, 'x', container.offsetHeight);
        
        // Create a new Map instance
        this.map = new Map({
          basemap: 'streets-vector'
        });

        // Create a new MapView instance
        this.mapView = new MapView({
          container: container,
          map: this.map,
          zoom: 10,
          center: [-118.2437, 34.0522] // Los Angeles coordinates
        });

        // Create graphics layers for markers and assets
        this.markersLayer = new GraphicsLayer({ id: 'markersLayer' });
        this.assetsLayer = new GraphicsLayer({ id: 'assetsLayer' });
        this.map.addMany([this.assetsLayer, this.markersLayer]);

        // Add click event handler for graphics
        this.mapView.on('click', (event) => {
          this.handleMapClick(event);
        });

        // Map is ready
        this.mapView.when(() => {
          console.log('MapView is ready');
        });

        console.log('Map initialized successfully');
        resolve();
      } catch (error) {
        console.error('Error initializing map:', error);
        reject(error);
      }
    });
  }

  // Utility function to format titles properly
  private formatTitle(title: string): string {
    return title
      .replace(/_/g, ' ') // Replace underscores with spaces
      .replace(/\b\w/g, (char) => char.toUpperCase()) // Capitalize first letter of each word
      .trim();
  }

  private addMarker(longitude: number, latitude: number, title: string, color: string = '#ff0000', layer: GraphicsLayer = this.markersLayer): void {
    const point = new Point({ longitude, latitude });

    // Create a pin marker symbol with SVG
    const svgContent = `
      <svg width="24" height="32" viewBox="0 0 24 32" xmlns="http://www.w3.org/2000/svg">
        <path d="M12 0C5.373 0 0 5.373 0 12c0 10.5 12 20 12 20s12-9.5 12-20c0-6.627-5.373-12-12-12z" fill="${color}"/>
        <circle cx="12" cy="12" r="6" fill="white"/>
      </svg>
    `;
    
    const markerSymbol = new PictureMarkerSymbol({
      url: "data:image/svg+xml;base64," + btoa(svgContent),
      width: "24px",
      height: "32px"
    });

    const pointGraphic = new Graphic({ 
      geometry: point, 
      symbol: markerSymbol,
      attributes: {
        id: title,
        type: layer === this.markersLayer ? 'marker' : 'asset',
        title: this.formatTitle(title),
        color: color,
        longitude: longitude,
        latitude: latitude
      }
    });
    
    layer.add(pointGraphic);
  }

  clearMap(): void {
    this.markersLayer.removeAll();
    this.assetsLayer.removeAll();
  }

  addRandomMapMarkers(): void {
    // Clear existing markers
    this.markersLayer.removeAll();

    // Add random markers around Los Angeles
    for (let i = 0; i < 8; i++) {
      const longitude = -118.2437 + (Math.random() - 0.5) * 0.8;
      const latitude = 34.0522 + (Math.random() - 0.5) * 0.6;
      const title = this.formatTitle(`marker_${i + 1}`);
      const colors = ['#2563EB', '#9333EA', '#EF4444', '#10B981', '#F59E0B'];
      const color = colors[Math.floor(Math.random() * colors.length)];
      this.addMarker(longitude, latitude, title, color, this.markersLayer);
    }
  }

  showAssets(): void {
    // Clear existing assets
    this.assetsLayer.removeAll();

    // Add random green assets around LA
    for (let i = 0; i < 8; i++) {
      const longitude = -118.2437 + (Math.random() - 0.5) * 0.8;
      const latitude = 34.0522 + (Math.random() - 0.5) * 0.6;
      this.addMarker(longitude, latitude, this.formatTitle(`asset_${i + 1}`), '#10B981', this.assetsLayer);
    }
  }



  // Method to add assets from message
  addAssetsFromMessage(assets: any[]): void {
    console.log('Adding assets from message:', assets);
    
    // Clear existing assets first
    this.assetsLayer.removeAll();
    
    assets.forEach((asset, index) => {
      let longitude = -118.2437 + (Math.random() - 0.5) * 0.8;
      let latitude = 34.0522 + (Math.random() - 0.5) * 0.6;
      
      // Use location from asset if available
      if (asset.location) {
        longitude = asset.location.longitude;
        latitude = asset.location.latitude;
      }
      
      const title = asset.name || asset.id || this.formatTitle(`asset_${index + 1}`);
      
      // Use a highlighted color for message assets
      this.addMarker(longitude, latitude, title, '#FF6B35', this.assetsLayer);
    });
  }

  // Method to add markers from message
  addMarkersFromMessage(markers: any[]): void {
    console.log('Adding markers from message:', markers);
    
    // Clear existing markers first
    this.markersLayer.removeAll();
    
    markers.forEach((marker, index) => {
      let longitude = -118.2437 + (Math.random() - 0.5) * 0.8;
      let latitude = 34.0522 + (Math.random() - 0.5) * 0.6;
      
      // Use location from marker if available
      if (marker.location) {
        longitude = marker.location.longitude;
        latitude = marker.location.latitude;
      }
      
      const title = marker.title || marker.id || this.formatTitle(`marker_${index + 1}`);
      
      // Use a highlighted color for message markers
      this.addMarker(longitude, latitude, title, '#FFD700', this.markersLayer);
    });
  }

  getMapView(): MapView { return this.mapView; }
  getMap(): Map { return this.map; }

  // Popup methods
  showPopup(data: MapPopupData): void {
    this.popupDataSubject.next(data);
  }

  hidePopup(): void {
    this.popupDataSubject.next(null);
  }

  private handleMapClick(event: any): void {
    console.log('Map clicked at:', event.mapPoint);
    
    // Check if we clicked on a graphic
    this.mapView.hitTest(event).then((response) => {
      console.log('Hit test results:', response.results);
      console.log('Hit test response object:', response);
      
      if (response.results && response.results.length > 0) {
        const result = response.results[0];
        console.log('Hit test result:', result);
        console.log('Result type:', typeof result);
        console.log('Result keys:', Object.keys(result));
        
        // ArcGIS hit test results can have different types, we need to check for graphics specifically
        if (result && typeof result === 'object' && 'graphic' in result) {
          const graphic = (result as any).graphic;
          if (graphic && graphic.attributes) {
            const attributes = graphic.attributes;
            console.log('Graphic attributes:', attributes);
            
            // Only show popup if we have valid attributes AND the graphic is from our layers
            if (attributes.id && attributes.type && attributes.title && 
                (attributes.type === 'marker' || attributes.type === 'asset')) {
              const popupData: MapPopupData = {
                id: attributes.id || 'Unknown',
                type: attributes.type || 'Unknown',
                title: this.formatTitle(attributes.title) || 'Unknown',
                color: attributes.color || '#000000',
                longitude: attributes.longitude || 0,
                latitude: attributes.latitude || 0,
                description: `This is a ${attributes.type} located at coordinates ${attributes.longitude}, ${attributes.latitude}`,
                status: 'Active',
                notes: ''
              };
              console.log('Showing popup with data:', popupData);
              this.showPopup(popupData);
            } else {
              console.log('Invalid graphic attributes or wrong type, not showing popup');
              console.log('Type:', attributes.type, 'Expected: marker or asset');
            }
          } else {
            console.log('No valid graphic or attributes found in hit test result');
          }
        } else {
          console.log('No graphic found in hit test result');
          console.log('Result:', result);
        }
      } else {
        console.log('No hit test results - clicked on empty map area');
        console.log('Response results:', response.results);
        // Don't show popup when clicking on empty areas
      }
    }).catch((error) => {
      console.error('Error in hit test:', error);
    });
  }

}
