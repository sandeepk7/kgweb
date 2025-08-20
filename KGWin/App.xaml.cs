using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using System.Text.Json;
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace KGWin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private WebApplication? _webApp;
        private CommunicationHub? _hubInstance;
        private IHubContext<CommunicationHub>? _hubContext;
        private Task? _nativeMessagingTask;
        
        // Static property to track native messaging activity
        public static DateTime LastNativeMessageReceived { get; private set; } = DateTime.MinValue;
        public static bool HasReceivedNativeMessages => (DateTime.Now - LastNativeMessageReceived).TotalSeconds < 30;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            InitSingalRServer();

            // Start the Native Messaging loop in a background task
            System.Diagnostics.Debug.WriteLine("=== KGWin STARTUP ===");
            System.Diagnostics.Debug.WriteLine("Starting native messaging task...");
            _nativeMessagingTask = Task.Run(() => StartNativeMessagingLoop());
            System.Diagnostics.Debug.WriteLine("Native messaging task started");

            if (e.Args.Length > 0)
            {
                string protocolUrl = e.Args[0]; // e.g., kgwin://launch?assetId=123&layerId=456

                Uri uri = new Uri(protocolUrl);
                string assetId = System.Web.HttpUtility.ParseQueryString(uri.Query).Get("assetId");
                string layerId = System.Web.HttpUtility.ParseQueryString(uri.Query).Get("layerId");

                MessageBox.Show($"Asset: {assetId}, Layer: {layerId}", "Incoming data from opener");
            }
        }

        private void InitSingalRServer()
        {
            Task.Run(async () =>
            {
                var builder = WebApplication.CreateBuilder();

                builder.WebHost.UseUrls("http://localhost:5000");

                builder.Services.AddSignalR();

                // Enable CORS for Angular dev server and GitHub Pages
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowAngularDevClient",
                        policy =>
                        {
                            policy.WithOrigins(
                                    "http://localhost:4200", // Angular dev server
                                    "https://sandeepk7.github.io" // GitHub Pages
                                  )
                                  .AllowAnyHeader()
                                  .AllowAnyMethod()
                                  .AllowCredentials();
                        });
                });

                _webApp = builder.Build();

                _webApp.UseRouting();
                _webApp.UseCors("AllowAngularDevClient");

                _webApp.MapHub<CommunicationHub>("/communicationHub");

                await _webApp.StartAsync();

                // Resolve and retain a hub context for server-side sends
                _hubContext = _webApp.Services.GetRequiredService<IHubContext<CommunicationHub>>();
            });
        }

        private void StartNativeMessagingLoop()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== NATIVE MESSAGING LOOP STARTED ===");
                System.Diagnostics.Debug.WriteLine("Opening stdin/stdout for native messaging...");
                
                // Check if stdin/stdout are available
                try
                {
                    var stdin = Console.OpenStandardInput();
                    var stdout = Console.OpenStandardOutput();
                    
                    System.Diagnostics.Debug.WriteLine("Stdin/stdout opened successfully");
                    System.Diagnostics.Debug.WriteLine("Stdin can read: " + stdin.CanRead);
                    System.Diagnostics.Debug.WriteLine("Stdout can write: " + stdout.CanWrite);
                    
                    using (stdin)
                    using (stdout)
                    {
                        System.Diagnostics.Debug.WriteLine("Waiting for messages from browser extension...");

                        while (true)
                        {
                            System.Diagnostics.Debug.WriteLine("Reading message from stdin...");
                            string? input = ReadMessage(stdin);
                            if (input == null) 
                            {
                                System.Diagnostics.Debug.WriteLine("Received null input, browser closed pipe");
                                break; // Browser closed pipe
                            }

                            System.Diagnostics.Debug.WriteLine($"Received raw input: {input}");

                            try
                            {
                                var request = JsonSerializer.Deserialize<Dictionary<string, object>>(input);
                                System.Diagnostics.Debug.WriteLine($"Deserialized request: {JsonSerializer.Serialize(request)}");
                                
                                var response = ProcessNativeMessage(request);
                                System.Diagnostics.Debug.WriteLine($"Sending response: {JsonSerializer.Serialize(response)}");
                                
                                WriteMessage(stdout, response);
                                System.Diagnostics.Debug.WriteLine("Response sent successfully");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error processing message: {ex.Message}");
                                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                                WriteMessage(stdout, new { error = ex.Message });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Stdin/stdout error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
            catch (Exception ex)
            {
                // Log error but don't crash the app
                System.Diagnostics.Debug.WriteLine($"Native messaging loop error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private object ProcessNativeMessage(Dictionary<string, object>? request)
        {
            // Track that we received a native message
            LastNativeMessageReceived = DateTime.Now;
            
            if (request == null || !request.ContainsKey("action"))
            {
                return new { error = "Invalid message format" };
            }

            string action = request["action"].ToString() ?? "";
            
            switch (action)
            {
                case "PING":
                    return new { type = "PING_RESPONSE", payload = new { ok = true } };
                
                case "REGISTER":
                    if (request.ContainsKey("payload"))
                    {
                        var payload = request["payload"] as Dictionary<string, object>;
                        if (payload != null && payload.ContainsKey("clientName"))
                        {
                            string clientName = payload["clientName"].ToString() ?? "";
                            string clientId = $"tab-{DateTime.Now.Ticks}";
                            
                            // Store client registration (in real implementation, this would be persistent)
                            System.Diagnostics.Debug.WriteLine($"Client registered: {clientName} ({clientId})");
                            
                            return new { 
                                type = "REGISTER_RESPONSE", 
                                payload = new { 
                                    ok = true, 
                                    clientId,
                                    users = new List<object> {
                                        new { id = clientId, name = clientName, clientId, isSelf = true }
                                        // Don't include KGWin in users list - it's the receiver
                                    }
                                } 
                            };
                        }
                    }
                    return new { error = "Missing clientName in payload" };
                
                case "GET_USERS":
                    // Return list of registered users (excluding KGWin)
                    return new { 
                        type = "GET_USERS_RESPONSE", 
                        payload = new { 
                            users = new List<object>() 
                            // Empty list since KGWin is the receiver, not a user
                        } 
                    };
                
                case "SEND_MESSAGE":
                    if (request.ContainsKey("payload"))
                    {
                        var payload = request["payload"] as Dictionary<string, object>;
                        if (payload != null && payload.ContainsKey("content"))
                        {
                            string content = payload["content"].ToString() ?? "";
                            string fromId = payload.ContainsKey("fromId") ? payload["fromId"].ToString() ?? "" : "unknown";
                            string toId = payload.ContainsKey("toClientId") ? payload["toClientId"].ToString() ?? "" : "kgwin-client";
                            bool isJson = payload.ContainsKey("isJson") && (bool)payload["isJson"];
                            
                            // Log the message (in real implementation, this would be stored and relayed)
                            System.Diagnostics.Debug.WriteLine($"Message from {fromId} to {toId}: {content}");
                            
                            // If message is to KGWin, we could trigger UI updates here
                            if (toId == "kgwin-client")
                            {
                                // TODO: Update KGWin UI with the received message
                                System.Diagnostics.Debug.WriteLine($"Message received in KGWin: {content}");
                            }
                            
                            return new { 
                                type = "SEND_MESSAGE_RESPONSE", 
                                payload = new { 
                                    ok = true,
                                    message = new {
                                        id = DateTime.Now.Ticks.ToString(),
                                        fromClientId = fromId,
                                        fromName = "KGWeb",
                                        toClientId = toId,
                                        content = content,
                                        isJson = isJson,
                                        timestamp = DateTime.Now.ToString("O")
                                    }
                                } 
                            };
                        }
                    }
                    return new { error = "Missing content in payload" };
                
                case "GET_HISTORY":
                    // Return chat history (in real implementation, this would be from persistent storage)
                    return new { 
                        type = "GET_HISTORY_RESPONSE", 
                        payload = new { 
                            messages = new List<object>() 
                        } 
                    };
                
                default:
                    return new { error = $"Unknown action: {action}" };
            }
        }

        private string? ReadMessage(Stream input)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("ReadMessage: Reading length bytes...");
                byte[] lengthBytes = new byte[4];
                int bytesRead = input.Read(lengthBytes, 0, 4);
                if (bytesRead == 0)
                {
                    System.Diagnostics.Debug.WriteLine("ReadMessage: No bytes read, end of stream");
                    return null; // End of stream
                }

                int length = BitConverter.ToInt32(lengthBytes, 0);
                System.Diagnostics.Debug.WriteLine($"ReadMessage: Message length = {length} bytes");

                byte[] buffer = new byte[length];
                int read = 0;
                while (read < length)
                {
                    int chunk = input.Read(buffer, read, length - read);
                    if (chunk == 0) break;
                    read += chunk;
                }

                string result = Encoding.UTF8.GetString(buffer);
                System.Diagnostics.Debug.WriteLine($"ReadMessage: Successfully read message: {result}");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ReadMessage error: {ex.Message}");
                throw;
            }
        }

        private void WriteMessage(Stream output, object message)
        {
            try
            {
                string json = JsonSerializer.Serialize(message);
                System.Diagnostics.Debug.WriteLine($"WriteMessage: Serialized message: {json}");
                
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                byte[] length = BitConverter.GetBytes(bytes.Length);
                
                System.Diagnostics.Debug.WriteLine($"WriteMessage: Writing {bytes.Length} bytes");

                output.Write(length, 0, length.Length);
                output.Write(bytes, 0, bytes.Length);
                output.Flush();
                
                System.Diagnostics.Debug.WriteLine("WriteMessage: Message written and flushed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WriteMessage error: {ex.Message}");
                throw;
            }
        }

        public CommunicationHub? GetHubInstance()
        {
            return _hubInstance;
        }

        public void SetHubInstance(CommunicationHub hub)
        {
            _hubInstance = hub;
        }

        public IHubContext<CommunicationHub>? GetHubContext()
        {
            return _hubContext;
        }
    }
}
