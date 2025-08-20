using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace KGWin
{
    public partial class UserChatWindow : Window
    {
        private CommunicationHub? _hub;
        private string _targetUserId;
        private string _targetUserName;
        private List<ChatMessage> _chatHistory = new List<ChatMessage>();
        private DispatcherTimer? _updateTimer;

        public UserChatWindow(string userId, string userName = "")
        {
            InitializeComponent();
            _targetUserId = userId;
            _targetUserName = userName;
            
            InitializeHub();
            SetupTimer();
            UpdateChatTitle();
            AddSystemMessage($"Chat started with {_targetUserName} ({_targetUserId})");

            // Ensure initial UI state after all elements are loaded
            Loaded += UserChatWindow_Loaded;
        }

        private void UserChatWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Set default selection without relying on XAML IsSelected
                if (MessageTypeComboBox != null && MessageTypeComboBox.SelectedIndex < 0)
                {
                    MessageTypeComboBox.SelectedIndex = 0; // Text Message
                }

                if (MessageInput != null)
                {
                    MessageInput.Text = "Hello! How can I help you today?";
                }
            }
            catch
            {
                // No-op: defensive initialization
            }
        }

        private void InitializeHub()
        {
            var app = Application.Current as App;
            if (app != null)
            {
                _hub = app.GetHubInstance();
                if (_hub != null)
                {
                    AddSystemMessage("Connected to SignalR Hub");
                }
                else
                {
                    AddSystemMessage("Hub not available yet, will retry...");
                }
            }
        }

        private void SetupTimer()
        {
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromSeconds(2);
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            UpdateConnectionStatus();
        }

        private void UpdateConnectionStatus()
        {
            if (_hub == null)
            {
                var app = Application.Current as App;
                if (app != null)
                {
                    _hub = app.GetHubInstance();
                }
            }

            // Check if user is still connected
            var connectedUsers = _hub?.GetConnectedClientIds() ?? new List<string>();
            var isConnected = connectedUsers.Contains(_targetUserId);

            Dispatcher.Invoke(() =>
            {
                if (isConnected)
                {
                    ConnectionIndicator.Fill = Brushes.Green;
                    ConnectionStatusText.Text = "Connected";
                    ConnectionStatusText.Foreground = Brushes.Green;
                }
                else
                {
                    ConnectionIndicator.Fill = Brushes.Red;
                    ConnectionStatusText.Text = "Disconnected";
                    ConnectionStatusText.Foreground = Brushes.Red;
                }
            });
        }

        private void UpdateChatTitle()
        {
            var title = string.IsNullOrEmpty(_targetUserName) 
                ? $"Chat with {_targetUserId}" 
                : $"Chat with {_targetUserName} ({_targetUserId})";
            
            ChatTitleText.Text = title;
            Title = title;
        }

        private void AddSystemMessage(string message)
        {
            var chatMessage = new ChatMessage
            {
                Content = message,
                Timestamp = DateTime.Now,
                Type = MessageType.System,
                Sender = "System"
            };

            _chatHistory.Add(chatMessage);
            DisplayMessage(chatMessage);
        }

        private void AddSentMessage(string content, MessageType type = MessageType.Text)
        {
            var chatMessage = new ChatMessage
            {
                Content = content,
                Timestamp = DateTime.Now,
                Type = type,
                Sender = "You"
            };

            _chatHistory.Add(chatMessage);
            DisplayMessage(chatMessage);
        }

        private void AddReceivedMessage(string content, string sender, MessageType type = MessageType.Text)
        {
            var chatMessage = new ChatMessage
            {
                Content = content,
                Timestamp = DateTime.Now,
                Type = type,
                Sender = sender
            };

            _chatHistory.Add(chatMessage);
            DisplayMessage(chatMessage);
        }

        private void DisplayMessage(ChatMessage message)
        {
            Dispatcher.Invoke(() =>
            {
                var messageBorder = new Border();
                var messageText = new TextBlock
                {
                    Text = $"{message.Content}\n{message.Timestamp:HH:mm:ss}",
                    FontSize = 13,
                    TextWrapping = TextWrapping.Wrap
                };

                switch (message.Type)
                {
                    case MessageType.Text:
                    case MessageType.Json:
                        if (message.Sender == "You")
                        {
                            messageBorder.Style = FindResource("SentMessageStyle") as Style;
                            messageText.Foreground = Brushes.White;
                        }
                        else
                        {
                            messageBorder.Style = FindResource("ReceivedMessageStyle") as Style;
                            messageText.Foreground = Brushes.Black;
                        }
                        break;

                    case MessageType.System:
                        messageBorder.Style = FindResource("SystemMessageStyle") as Style;
                        messageText.Foreground = Brushes.Black;
                        messageText.FontStyle = FontStyles.Italic;
                        break;
                }

                messageBorder.Child = messageText;
                MessagesContainer.Children.Add(messageBorder);

                // Auto-scroll to bottom
                MessagesScrollViewer.ScrollToBottom();
            });
        }

        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            var messageContent = MessageInput.Text.Trim();
            if (string.IsNullOrEmpty(messageContent))
            {
                MessageBox.Show("Please enter a message to send.", "No Message", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedItem = MessageTypeComboBox.SelectedItem as ComboBoxItem;
            var messageType = selectedItem?.Content.ToString();

            try
            {
                switch (messageType)
                {
                    case "Text Message":
                        SendTextMessage(messageContent);
                        break;
                    case "JSON Data":
                        SendJsonMessage(messageContent);
                        break;
                    case "Asset Location":
                        SendAssetLocation(messageContent);
                        break;
                    case "Map Marker":
                        SendMapMarker(messageContent);
                        break;
                }

                MessageInput.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending message: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SendTextMessage(string message)
        {
            if (_hub != null)
            {
                // For now, we'll broadcast with target info since direct messaging requires hub method
                await _hub.SendMessage("KGWin Monitor", $"To {_targetUserName}: {message}");
                AddSentMessage(message, MessageType.Text);
            }
        }

        private async void SendJsonMessage(string jsonContent)
        {
            if (_hub != null)
            {
                try
                {
                    // Validate JSON
                    JsonDocument.Parse(jsonContent);
                    await _hub.SendMessage("KGWin Monitor", $"To {_targetUserName} (JSON): {jsonContent}");
                    AddSentMessage(jsonContent, MessageType.Json);
                }
                catch (JsonException)
                {
                    MessageBox.Show("Invalid JSON format.", "JSON Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void SendAssetLocation(string jsonContent)
        {
            try
            {
                var asset = JsonSerializer.Deserialize<AssetLocation>(jsonContent);
                if (asset != null && _hub != null)
                {
                    await _hub.SendAssetLocation(asset);
                    AddSentMessage($"Asset Location: {asset.Name}", MessageType.Json);
                }
                else
                {
                    MessageBox.Show("Invalid asset location format.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (JsonException)
            {
                MessageBox.Show("Invalid JSON format for asset location.", "JSON Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SendMapMarker(string jsonContent)
        {
            try
            {
                var marker = JsonSerializer.Deserialize<MapMarker>(jsonContent);
                if (marker != null && _hub != null)
                {
                    await _hub.SendMapMarker(marker);
                    AddSentMessage($"Map Marker: {marker.Title}", MessageType.Json);
                }
                else
                {
                    MessageBox.Show("Invalid map marker format.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (JsonException)
            {
                MessageBox.Show("Invalid JSON format for map marker.", "JSON Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SendTestAsset_Click(object sender, RoutedEventArgs e)
        {
            var testAsset = new AssetLocation
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"Test Asset for {_targetUserName}",
                Latitude = 40.7128,
                Longitude = -74.0060,
                Status = "Active",
                LastUpdated = DateTime.Now
            };

            if (_hub != null)
            {
                await _hub.SendAssetLocation(testAsset);
                AddSentMessage($"Test Asset: {testAsset.Name}", MessageType.Json);
            }
        }

        private async void SendTestMarker_Click(object sender, RoutedEventArgs e)
        {
            var testMarker = new MapMarker
            {
                Id = Guid.NewGuid().ToString(),
                Title = $"Test Marker for {_targetUserName}",
                Description = "This is a test marker sent from the chat window",
                Latitude = 40.7128,
                Longitude = -74.0060,
                Color = "#FF0000",
                Icon = "📍"
            };

            if (_hub != null)
            {
                await _hub.SendMapMarker(testMarker);
                AddSentMessage($"Test Marker: {testMarker.Title}", MessageType.Json);
            }
        }

        private void ClearChat_Click(object sender, RoutedEventArgs e)
        {
            MessagesContainer.Children.Clear();
            _chatHistory.Clear();
            AddSystemMessage("Chat history cleared");
        }

        private void RefreshChat_Click(object sender, RoutedEventArgs e)
        {
            UpdateConnectionStatus();
            AddSystemMessage("Chat refreshed");
        }

        private void MessageTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Guard: controls may not be available during early initialization
            if (MessageInput == null || MessageTypeComboBox == null)
            {
                return;
            }

            var selectedItem = MessageTypeComboBox.SelectedItem as ComboBoxItem;
            var messageType = selectedItem?.Content?.ToString() ?? "Text Message";

            switch (messageType)
            {
                case "JSON Data":
                    MessageInput.Text = "{\n  \"key\": \"value\",\n  \"number\": 123,\n  \"array\": [1, 2, 3]\n}";
                    break;
                case "Asset Location":
                    MessageInput.Text = "{\n  \"id\": \"asset-123\",\n  \"name\": \"Test Asset\",\n  \"latitude\": 40.7128,\n  \"longitude\": -74.0060,\n  \"status\": \"Active\",\n  \"lastUpdated\": \"2024-01-01T12:00:00Z\"\n}";
                    break;
                case "Map Marker":
                    MessageInput.Text = "{\n  \"id\": \"marker-123\",\n  \"title\": \"Test Marker\",\n  \"description\": \"Test description\",\n  \"latitude\": 40.7128,\n  \"longitude\": -74.0060,\n  \"color\": \"#FF0000\",\n  \"icon\": \"📍\"\n}";
                    break;
                default:
                    MessageInput.Text = "Hello! How can I help you today?";
                    break;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _updateTimer?.Stop();
            base.OnClosed(e);
        }
    }


}
