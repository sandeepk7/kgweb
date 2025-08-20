using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Linq; // Added for FirstOrDefault

namespace KGWin
{
    public partial class BrowserExtPage : Page
    {
        private ObservableCollection<ChatUser> connectedUsers;
        private ChatUser? selectedUser;
        private List<BrowserExtChatMessage> chatHistory;
        private string clientId;
        private bool isRegistered = false;
        private bool kgwebConnected = false;
        private DispatcherTimer statusTimer;

        public BrowserExtPage()
        {
            InitializeComponent();
            InitializeChat();
            InitializeStatusTimer();
            
            // Auto-register on load
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (!isRegistered)
                    {
                        Register_Click(this, new RoutedEventArgs());
                    }
                }
                catch {}
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void InitializeChat()
        {
            connectedUsers = new ObservableCollection<ChatUser>();
            chatHistory = new List<BrowserExtChatMessage>();
            clientId = $"kgwin-{Guid.NewGuid():N}";
            UsersListBox.ItemsSource = connectedUsers;
        }

        private void InitializeStatusTimer()
        {
            statusTimer = new DispatcherTimer();
            statusTimer.Interval = TimeSpan.FromSeconds(2);
            statusTimer.Tick += StatusTimer_Tick;
            statusTimer.Start();
        }

        private void StatusTimer_Tick(object? sender, EventArgs e)
        {
            // Check if we can communicate with the native messaging host
            UpdateExtensionStatus();
        }

        private void UpdateExtensionStatus()
        {
            // Check if native messaging is actually working by testing the connection
            bool isNativeConnected = TestNativeMessagingConnection();
            
            // Check if KGWeb is connected by looking for registered users
            kgwebConnected = connectedUsers.Any(u => u.Name == "KGWeb");
            
            if (isNativeConnected)
            {
                ExtensionStatusIndicator.Fill = Brushes.Green;
                ExtensionStatusText.Text = "Connected";
                ExtensionStatusText.Foreground = Brushes.Green;
            }
            else
            {
                ExtensionStatusIndicator.Fill = Brushes.Red;
                ExtensionStatusText.Text = "Not Connected";
                ExtensionStatusText.Foreground = Brushes.Red;
            }
            
            // Update KGWeb status
            if (kgwebConnected)
            {
                KGWebStatusIndicator.Fill = Brushes.Green;
                KGWebStatusText.Text = "Connected";
                KGWebStatusText.Foreground = Brushes.Green;
            }
            else
            {
                KGWebStatusIndicator.Fill = Brushes.Gray;
                KGWebStatusText.Text = "Not Connected";
                KGWebStatusText.Foreground = Brushes.Gray;
            }
        }

        private bool TestNativeMessagingConnection()
        {
            try
            {
                // Check if we have received any native messages recently
                // This indicates that the browser extension is actually connecting
                bool hasReceived = App.HasReceivedNativeMessages;
                
                // Add debugging output
                System.Diagnostics.Debug.WriteLine($"Native messaging test: HasReceived={hasReceived}, LastReceived={App.LastNativeMessageReceived}");
                
                return hasReceived;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Native messaging test error: {ex.Message}");
                return false;
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // KGWin is the receiver, not a user - so don't add it to users list
                connectedUsers.Clear();
                isRegistered = true;
                
                UpdateExtensionStatus();
                
                // Don't simulate KGWeb connection - let it happen through actual native messaging
                // Only add KGWeb user when we actually receive a message from it
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Registration failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshUsers_Click(object sender, RoutedEventArgs e)
        {
            // Simulate refreshing users list
            // In real implementation, this would query the native messaging host
            UpdateExtensionStatus();
        }

        private void ClearChat_Click(object sender, RoutedEventArgs e)
        {
            ChatMessagesPanel.Children.Clear();
        }

        private void UsersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedUser = UsersListBox.SelectedItem as ChatUser;
            if (selectedUser != null)
            {
                ChatTitleText.Text = $"Chat with {selectedUser.Name}";
                LoadChatHistory();
            }
        }

        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift)
            {
                e.Handled = true;
                SendMessage();
            }
        }

        private void SendMessage()
        {
            if (!isRegistered)
            {
                MessageBox.Show("Please register first", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(MessageTextBox.Text))
                return;

            try
            {
                var message = new BrowserExtChatMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    FromId = clientId,
                    FromName = "KGWin",
                    ToId = selectedUser?.Id,
                    Content = MessageTextBox.Text,
                    IsJson = IsJsonContent(MessageTextBox.Text),
                    Timestamp = DateTime.Now,
                    IsFromSelf = true
                };

                AddMessageToChat(message);
                MessageTextBox.Clear();

                // In real implementation, this would send through native messaging
                // For now, simulate sending
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    // Simulate response from KGWeb
                    if (selectedUser?.Name == "KGWeb")
                    {
                        ReceiveMessage("KGWeb", $"Received: {message.Content}");
                    }
                }), DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Send failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InsertTestJson_Click(object sender, RoutedEventArgs e)
        {
            var testJson = new
            {
                type = "asset",
                id = Guid.NewGuid().ToString(),
                name = "Test Asset",
                location = new { lat = 40.7128, lng = -74.0060 },
                properties = new { status = "active", priority = "high" }
            };

            MessageTextBox.Text = JsonSerializer.Serialize(testJson, new JsonSerializerOptions { WriteIndented = true });
        }

        private void GetHistory_Click(object sender, RoutedEventArgs e)
        {
            LoadChatHistory();
        }

        private void LoadChatHistory()
        {
            ChatMessagesPanel.Children.Clear();
            
            if (selectedUser == null) return;

            var messages = chatHistory.FindAll(m => 
                (m.FromId == selectedUser.Id && m.ToId == clientId) ||
                (m.FromId == clientId && m.ToId == selectedUser.Id) ||
                (m.ToId == null && m.FromId != clientId) // Broadcast messages
            );

            foreach (var message in messages)
            {
                AddMessageToChat(message);
            }
        }

        private void AddMessageToChat(BrowserExtChatMessage message)
        {
            var messageBorder = new Border
            {
                Margin = new Thickness(0, 5, 0, 5),
                Padding = new Thickness(12, 8, 12, 8),
                CornerRadius = new CornerRadius(8),
                HorizontalAlignment = message.IsFromSelf ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                MaxWidth = 400
            };

            var messageText = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                FontSize = message.IsJson ? 11 : 14,
                FontFamily = message.IsJson ? new FontFamily("Consolas") : new FontFamily("Segoe UI")
            };

            if (message.IsJson)
            {
                messageText.Text = message.Content;
            }
            else
            {
                messageText.Text = message.Content;
            }

            messageBorder.Child = messageText;

            // Style based on sender
            if (message.IsFromSelf)
            {
                messageBorder.Background = new SolidColorBrush(Color.FromRgb(0, 122, 204));
                messageText.Foreground = Brushes.White;
            }
            else
            {
                messageBorder.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
                messageText.Foreground = Brushes.Black;
            }

            // Add sender label
            var senderLabel = new TextBlock
            {
                Text = message.IsFromSelf ? "You" : message.FromName,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                Margin = new Thickness(0, 0, 0, 2),
                HorizontalAlignment = message.IsFromSelf ? HorizontalAlignment.Right : HorizontalAlignment.Left
            };

            var container = new StackPanel();
            container.Children.Add(senderLabel);
            container.Children.Add(messageBorder);

            ChatMessagesPanel.Children.Add(container);

            // Scroll to bottom
            var scrollViewer = ChatMessagesPanel.Parent as ScrollViewer;
            scrollViewer?.ScrollToBottom();
        }

        private bool IsJsonContent(string content)
        {
            try
            {
                JsonDocument.Parse(content);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Simulate receiving a message from KGWeb
        public void ReceiveMessage(string fromName, string content, bool isJson = false)
        {
            var message = new BrowserExtChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                FromId = $"kgweb-{Guid.NewGuid():N}",
                FromName = fromName,
                ToId = clientId,
                Content = content,
                IsJson = isJson,
                Timestamp = DateTime.Now,
                IsFromSelf = false
            };

            // Add to users if not exists
            var existingUser = connectedUsers.FirstOrDefault(u => u.Name == fromName);
            if (existingUser == null)
            {
                var newUser = new ChatUser
                {
                    Id = message.FromId,
                    Name = fromName,
                    IsSelf = false
                };
                connectedUsers.Add(newUser);
            }

            chatHistory.Add(message);
            
            // Add to chat if this user is selected
            if (selectedUser?.Name == fromName)
            {
                AddMessageToChat(message);
            }
        }
    }

    public class ChatUser : INotifyPropertyChanged
    {
        private string id = "";
        private string name = "";
        private bool isSelf;

        public string Id
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public bool IsSelf
        {
            get => isSelf;
            set
            {
                isSelf = value;
                OnPropertyChanged(nameof(IsSelf));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class BrowserExtChatMessage
    {
        public string Id { get; set; } = "";
        public string FromId { get; set; } = "";
        public string FromName { get; set; } = "";
        public string? ToId { get; set; }
        public string Content { get; set; } = "";
        public bool IsJson { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsFromSelf { get; set; }
    }
}



