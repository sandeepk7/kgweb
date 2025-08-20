using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.AspNetCore.SignalR;
using Microsoft.VisualBasic;
using System.Diagnostics;

namespace KGWin
{
    public partial class SignalRCommunicationPage : Page
    {
        private CommunicationHub? _hub;
        private IHubContext<CommunicationHub>? _hubContext;
        private DispatcherTimer? _updateTimer;
        private int _connectedClients = 0;
        private List<string> _connectedUserIds = new List<string>();
        private Dictionary<string, string> _connectedUserNames = new Dictionary<string, string>();
        private string _selectedUserId = "";
        private string _selectedUserName = "";
        private List<ChatMessage> _chatHistory = new List<ChatMessage>();
        private Random _random = new Random();

        public SignalRCommunicationPage()
        {
            InitializeComponent();
            InitializeHub();
            SetupTimer();
            AddLogEntry("SignalR Communication initialized", LogType.Info);

            // Subscribe to hub message events so WPF receives messages from KGWeb
            CommunicationHub.MessageReceived += OnHubMessageReceived;
            CommunicationHub.AssetLocationReceived += OnAssetLocationReceived;
            CommunicationHub.MapMarkerReceived += OnMapMarkerReceived;
        }

        private void OnHubMessageReceived(string user, string message)
        {
            // Treat messages not from WPF as incoming
            if (!string.Equals(user, "KGWin Monitor", StringComparison.OrdinalIgnoreCase))
            {
                Dispatcher.Invoke(() =>
                {
                    // Detect JSON payloads
                    var type = MessageType.Text;
                    var trimmed = (message ?? string.Empty).Trim();
                    if (trimmed.StartsWith("{") || trimmed.StartsWith("["))
                    {
                        try { using var _ = JsonDocument.Parse(trimmed); type = MessageType.Json; } catch { }
                    }

                    AddChatMessage(message, user, type);
                });
            }
        }

        private void OnAssetLocationReceived(AssetLocation asset)
        {
            Dispatcher.Invoke(() =>
            {
                var json = JsonSerializer.Serialize(asset, new JsonSerializerOptions { WriteIndented = true });
                var sender = string.IsNullOrEmpty(_selectedUserName) ? "KGWin" : _selectedUserName;
                AddChatMessage(json, sender, MessageType.Json);
            });
        }

        private void OnMapMarkerReceived(MapMarker marker)
        {
            Dispatcher.Invoke(() =>
            {
                var json = JsonSerializer.Serialize(marker, new JsonSerializerOptions { WriteIndented = true });
                var sender = string.IsNullOrEmpty(_selectedUserName) ? "KGWin" : _selectedUserName;
                AddChatMessage(json, sender, MessageType.Json);
            });
        }

        private void InitializeHub()
        {
            var app = Application.Current as App;
            if (app != null)
            {
                _hub = app.GetHubInstance();
                _hubContext = app.GetHubContext();
                if (_hub != null)
                {
                    AddLogEntry("Hub initialized successfully", LogType.Success);
                }
                else
                {
                    AddLogEntry("Hub not available yet, will retry...", LogType.Warning);
                }
            }
            else
            {
                AddLogEntry("Failed to get app instance", LogType.Error);
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
            UpdateConnectedUsers();
        }

        private void UpdateConnectionStatus()
        {
            if (_hub == null || _hubContext == null)
            {
                var app = Application.Current as App;
                if (app != null)
                {
                    _hub = app.GetHubInstance();
                    _hubContext = app.GetHubContext();
                }
            }

                    if (_hub != null)
                    {
                ServerStatusText.Text = "Connected";
                ServerStatusText.Foreground = Brushes.Green;
                ServerStatusIndicator.Fill = Brushes.Green;
            }
            else
            {
                ServerStatusText.Text = "Disconnected";
                ServerStatusText.Foreground = Brushes.Red;
                ServerStatusIndicator.Fill = Brushes.Red;
            }
        }

        private void UpdateConnectedUsers()
        {
            if (_hub == null) return;

            try
            {
                var connectedIds = _hub.GetConnectedClientIds();
                var connectedNames = _hub.GetConnectedClientNames();

                // Update connected clients count
                _connectedClients = connectedIds.Count;

                // Update user lists
                _connectedUserIds = connectedIds;
                _connectedUserNames = connectedNames;

                // Refresh UI
                RefreshConnectedUsersList();

                // Auto-select the first user if none selected yet
                if (string.IsNullOrEmpty(_selectedUserId) && _connectedUserIds.Count > 0)
                {
                    var firstId = _connectedUserIds[0];
                    var firstName = _connectedUserNames.TryGetValue(firstId, out var name) ? name : string.Empty;
                    OpenChatWithUser(firstId, firstName);
                }
            }
            catch (Exception ex)
            {
                AddLogEntry($"Error updating connected users: {ex.Message}", LogType.Error);
            }
        }

        private void RefreshConnectedUsersList()
        {
            ConnectedUsersContainer.Children.Clear();

            foreach (var userId in _connectedUserIds)
            {
                var userItem = CreateUserItem(userId);
                ConnectedUsersContainer.Children.Add(userItem);
            }
        }

        private UIElement CreateUserItem(string userId)
        {
            var border = new Border
            {
                Background = Brushes.White,
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                Margin = new Thickness(0, 2, 0, 2),
                Padding = new Thickness(8, 4, 8, 4),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Connection indicator
            var indicator = new System.Windows.Shapes.Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = Brushes.Green,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            // Get user name if available
            var userName = _connectedUserNames.TryGetValue(userId, out var name) ? name : string.Empty;
            
            // User info text (name + ID)
            var userInfoText = new TextBlock
            {
                FontFamily = new FontFamily("Consolas"),
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center
            };

            if (!string.IsNullOrEmpty(userName))
            {
                userInfoText.Text = userName; // show only name
                userInfoText.FontWeight = FontWeights.SemiBold;
            }
            else
            {
                userInfoText.Text = userId;
                userInfoText.FontStyle = FontStyles.Italic;
            }

            Grid.SetColumn(indicator, 0);
            Grid.SetColumn(userInfoText, 1);

            grid.Children.Add(indicator);
            grid.Children.Add(userInfoText);

            border.Child = grid;
            
            // Make entire border clickable
            border.MouseLeftButtonDown += (s, e) => OpenChatWithUser(userId, userName);
            
            return border;
        }

        private void AddLogEntry(string message, LogType type = LogType.Info)
        {
            // Keep logs out of the chat view; write to debug output instead.
            Debug.WriteLine($"[{DateTime.Now:HH:mm:ss}] {type}: {message}");
        }

        private Brush GetLogColor(LogType type)
        {
            return type switch
            {
                LogType.Success => Brushes.Green,
                LogType.Warning => Brushes.Orange,
                LogType.Error => Brushes.Red,
                LogType.Info => Brushes.Blue,
                _ => Brushes.Black
            };
        }

        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            ChatMessagesContainer.Children.Clear();
            AddLogEntry("Log cleared", LogType.Info);
        }

        private void OpenChatWithUser(string userId, string userName)
        {
            try
            {
                _selectedUserId = userId;
                _selectedUserName = userName;
                UpdateChatView();
                var displayName = string.IsNullOrEmpty(userName) ? userId : userName;
                AddLogEntry($"Selected chat with {displayName}", LogType.Info);
            }
            catch (Exception ex)
            {
                AddLogEntry($"Error selecting user chat: {ex.Message}", LogType.Error);
                MessageBox.Show($"Error selecting user chat: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateChatView()
        {
            if (string.IsNullOrEmpty(_selectedUserId))
            {
                // Clear chat view
                ChatTitleText.Text = "Chat Messages";
                ChatMessagesContainer.Children.Clear();
                return;
            }

            // Update chat title
            var title = string.IsNullOrEmpty(_selectedUserName) 
                ? $"Chat with KGWeb" 
                : $"Chat with {_selectedUserName}";
            ChatTitleText.Text = title;

            // Clear and reload chat messages for selected user
            ChatMessagesContainer.Children.Clear();
            var userMessages = _chatHistory.Where(m => m.Sender == _selectedUserName || m.Sender == "You" || m.Sender == "KGWin").ToList();
            
            foreach (var message in userMessages)
            {
                DisplayChatMessage(message);
            }
        }

        private void DisplayChatMessage(ChatMessage message)
        {
            var messageBorder = new Border
            {
                Margin = new Thickness(10, 5, 10, 5),
                Padding = new Thickness(12, 8, 12, 8),
                CornerRadius = new CornerRadius(8),
                MaxWidth = 500
            };

            // Header row (sender left, time right)
            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var headerSender = new TextBlock
            {
                Text = message.Sender,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Opacity = 0.8,
                Margin = new Thickness(2, 0, 8, 3)
            };
            var headerTime = new TextBlock
            {
                Text = message.Timestamp.ToString("HH:mm:ss"),
                FontSize = 11,
                Opacity = 0.8,
                Margin = new Thickness(8, 0, 2, 3)
            };
            Grid.SetColumn(headerTime, 1);
            headerGrid.Children.Add(headerSender);
            headerGrid.Children.Add(headerTime);

            var messageText = new TextBlock
            {
                Text = message.Content,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap
            };

            if (message.Type == MessageType.Json)
            {
                messageText.FontFamily = new FontFamily("Consolas");
            }

            if (message.Sender == "You")
            {
                messageBorder.Background = (Brush)new BrushConverter().ConvertFromString("#1976D2")!; // KGWin (blue)
                messageBorder.HorizontalAlignment = HorizontalAlignment.Right;
                messageText.Foreground = Brushes.White;
                headerSender.Foreground = Brushes.White;
                headerTime.Foreground = Brushes.White;
            }
            else if (message.Type == MessageType.System)
            {
                messageBorder.Background = (Brush)new BrushConverter().ConvertFromString("#FFF3CD")!;
                messageBorder.BorderBrush = (Brush)new BrushConverter().ConvertFromString("#FFEEBA")!;
                messageBorder.BorderThickness = new Thickness(1);
                messageBorder.HorizontalAlignment = HorizontalAlignment.Center;
                messageText.FontStyle = FontStyles.Italic;
                }
                else
                {
                messageBorder.Background = (Brush)new BrushConverter().ConvertFromString("#EEEEEE")!; // KGWeb (gray)
                messageBorder.BorderBrush = (Brush)new BrushConverter().ConvertFromString("#BDBDBD")!;
                messageBorder.BorderThickness = new Thickness(1);
                messageBorder.HorizontalAlignment = HorizontalAlignment.Left;
            }

            var stack = new StackPanel();
            stack.Children.Add(headerGrid);
            stack.Children.Add(messageText);
            messageBorder.Child = stack;
            ChatMessagesContainer.Children.Add(messageBorder);
        }

        private void SendChatMessage_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedUserId))
            {
                MessageBox.Show("Please select a user to chat with.", "No User Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var messageContent = ChatMessageInput.Text.Trim();
            if (string.IsNullOrEmpty(messageContent))
            {
                MessageBox.Show("Please enter a message to send.", "No Message", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Detect JSON and tag appropriately
                var contentToSend = messageContent;
                MessageType type = MessageType.Text;
                try
                {
                    using var doc = JsonDocument.Parse(messageContent);
                    contentToSend = doc.RootElement.GetRawText();
                    type = MessageType.Json;
                }
                catch { /* not JSON */ }

                SendMessageToUser(_selectedUserId, contentToSend);
                AddChatMessage(messageContent, "You", type);
                ChatMessageInput.Text = "";
                }
                catch (Exception ex)
                {
                MessageBox.Show($"Error sending message: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SendMessageToUser(string userId, string message)
        {
            if (_hubContext != null)
            {
                try
                {
                    // Keep sender name concise for web (KGWin)
                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", _selectedUserName == "KGWeb" ? "KGWin" : _selectedUserName, message);
                    AddLogEntry($"Sent message to user {userId}: {message}", LogType.Success);
                }
                catch (Exception ex)
                {
                    AddLogEntry($"Error sending message to user {userId}: {ex.Message}", LogType.Error);
                }
            }
            else
            {
                AddLogEntry("Hub not available for sending user message", LogType.Error);
            }
        }

        private async void SendTestAssetToUser_Click(object sender, RoutedEventArgs e)
        {
            // Insert example Asset JSON into the input instead of sending immediately
            var assetJson = JsonSerializer.Serialize(new
            {
                id = Guid.NewGuid().ToString(),
                name = $"Test Asset from KGWin",
                type = "pump",
                latitude = 40.7128,
                longitude = -74.0060,
                status = "active",
                lastUpdated = DateTime.Now
            }, new JsonSerializerOptions { WriteIndented = true });

            ChatMessageInput.Text = assetJson;
        }

        private async void SendTestMarkerToUser_Click(object sender, RoutedEventArgs e)
        {
            // Insert example Marker JSON into the input instead of sending immediately
            var markerJson = JsonSerializer.Serialize(new
            {
                id = Guid.NewGuid().ToString(),
                assetId = Guid.NewGuid().ToString(),
                title = "Test Marker from KGWin",
                description = "This is a test marker sent from KGWin",
                latitude = 40.7128,
                longitude = -74.0060,
                markerType = "custom",
                color = "#FF0000",
                icon = "📍",
                timestamp = DateTime.Now
            }, new JsonSerializerOptions { WriteIndented = true });

            ChatMessageInput.Text = markerJson;
        }

        private void AddChatMessage(string content, string sender, MessageType type = MessageType.Text)
        {
            var chatMessage = new ChatMessage
            {
                Content = content,
                Timestamp = DateTime.Now,
                Type = type,
                Sender = sender
            };

            _chatHistory.Add(chatMessage);
            
            if (sender == _selectedUserName || sender == "You" || sender == "KGWin")
            {
                DisplayChatMessage(chatMessage);
            }
        }

        private void SendToAllUsers_Click(object sender, RoutedEventArgs e)
        {
            if (_connectedUserIds.Count == 0)
            {
                MessageBox.Show("No users are currently connected.", "No Users", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var message = Interaction.InputBox(
                $"Enter message to send to all {_connectedUserIds.Count} connected users:",
                "Send Message to All Users",
                "Hello to all users from KGWin Monitor!");

            if (!string.IsNullOrEmpty(message))
            {
                foreach (var userId in _connectedUserIds)
                {
                    SendMessageToUser(userId, message);
                }
                AddLogEntry($"Sent message to all {_connectedUserIds.Count} users: {message}", LogType.Success);
            }
        }

        private async void RegisterTestClientName_Click(object sender, RoutedEventArgs e)
        {
            var clientName = Interaction.InputBox(
                "Enter a test client name to register:",
                "Register Test Client Name",
                $"TestUser_{DateTime.Now:HHmmss}");

            if (!string.IsNullOrEmpty(clientName) && _hub != null)
            {
                try
                {
                    await _hub.RegisterClientName(clientName);
                    AddLogEntry($"Registered test client name: {clientName}", LogType.Success);
                }
                catch (Exception ex)
                {
                    AddLogEntry($"Error registering client name: {ex.Message}", LogType.Error);
                }
            }
        }

        private void ClearChat_Click(object sender, RoutedEventArgs e)
        {
            ChatMessagesContainer.Children.Clear();
            _chatHistory.Clear();
            AddLogEntry("Chat history cleared", LogType.Info);
        }

        private void RefreshUsers_Click(object sender, RoutedEventArgs e)
        {
            UpdateConnectedUsers();
            AddLogEntry("Connected users list refreshed", LogType.Info);
        }
    }

    public class ChatMessage
    {
        public string Content { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public MessageType Type { get; set; }
        public string Sender { get; set; } = "";
    }

    public enum MessageType
    {
        Text,
        Json,
        System
    }

    public enum LogType
    {
        Info,
        Success,
        Warning,
        Error
    }
}
