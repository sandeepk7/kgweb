# KGWin - ESRI ArcGIS WPF Application

This WPF application demonstrates integration with ESRI ArcGIS Runtime SDK for .NET, featuring a modern navigation system with Home and Map pages.

## Features

- **Modern Navigation**: Top navigation bar with Home and Map pages
- **ESRI Map Integration**: Full-featured map with zoom, pan, and point marking capabilities
- **API Key Authentication**: Secure authentication with ESRI ArcGIS Online services
- **Real-time Coordinates**: Live latitude, longitude, and zoom level display
- **Interactive Controls**: Zoom in/out, reset view, and add custom points

## Setup Instructions

### 1. Get ESRI API Key

To use the ESRI map services, you need to obtain a free API key:

1. Go to [ESRI Developers](https://developers.arcgis.com/)
2. Click "Sign Up" to create a free account
3. Once registered, go to your dashboard
4. Create a new project or use an existing one
5. Navigate to the "API Keys" section in your project
6. Create a new API key with the following permissions:
   - **Basemaps**: Read access to ArcGIS Online basemaps
   - **Geocoding**: Basic geocoding services
   - **Routing**: Basic routing services

### 2. Configure API Key

1. Open the `appsettings.json` file in the project root
2. Replace the placeholder API key with your actual key:

```json
{
  "EsriSettings": {
    "ApiKey": "YOUR_ACTUAL_API_KEY_HERE",
    "PortalUrl": "https://www.arcgis.com"
  }
}
```

### 3. Build and Run

```bash
dotnet build
dotnet run --project KGWin
```

## API Key Security

- **Never commit API keys to version control**
- **Use environment variables in production**
- **Rotate keys regularly**
- **Monitor usage in ESRI Developer Dashboard**

## Free Tier Limits

The ESRI free tier includes:
- 20,000 map tile requests per month
- Access to ArcGIS Online services
- Basic geocoding and routing
- Suitable for development and small applications

## Troubleshooting

### "Token Required" Error
- Ensure your API key is correctly configured in `appsettings.json`
- Verify the API key has the necessary permissions
- Check your internet connection

### Map Not Loading
- Verify ESRI services are accessible
- Check API key validity in ESRI Developer Dashboard
- Ensure proper network connectivity

### Authentication Failures
- Double-check API key format (should start with "AAPK")
- Verify API key permissions include basemap access
- Check ESRI service status at [ESRI Status Page](https://status.arcgis.com/)

## Project Structure

```
KGWin/
├── MainWindow.xaml          # Main application window with navigation
├── HomePage.xaml           # Home page with welcome content
├── MapPage.xaml            # Map page with ESRI integration
├── LoginDialog.xaml        # Authentication dialog
├── EsriConfiguration.cs    # API key configuration service
├── appsettings.json        # Configuration file for API key
└── README.md              # This file
```

## Dependencies

- **.NET 8.0**: Target framework
- **ESRI ArcGIS Runtime WPF**: Version 200.8.0
- **Microsoft.Extensions.Configuration**: Version 9.0.8 for secure configuration management

## License

This project is for demonstration purposes. Please ensure compliance with ESRI's terms of service when using their APIs.
