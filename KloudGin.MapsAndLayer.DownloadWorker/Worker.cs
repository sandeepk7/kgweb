using KGWin.WorkerProcess.Models;
using RestSharp;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _config;
    private readonly RestClient _restClient;
    /// <summary>
    /// Worker
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="config"></param>
    public Worker(ILogger<Worker> logger, IConfiguration config, RestClient restClient)
    {
        _logger = logger;
        _config = config;
        _restClient = restClient;
    }
    /// <summary>
    /// SLog
    /// </summary>
    private static readonly Serilog.ILogger SLog = Serilog.Log.ForContext<Worker>();
    /// <summary>
    /// _localAppData
    /// </summary>
    private static readonly string _localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    /// <summary>
    /// ExecuteAsync
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var jobConfig = _config.GetSection("KGGISDownloadJob");
        int intervalMinutes = int.Parse(jobConfig["IntervalMinutes"] ?? "1440");

        DateTime dt = DateTime.UtcNow; // or any DateTime value
        long timestampMs = new DateTimeOffset(dt).ToUnixTimeMilliseconds();


        while (!stoppingToken.IsCancellationRequested)
        {
            List<TenantConfig?> tenants = jobConfig.GetSection("Tenants").Get<List<TenantConfig?>>();

            foreach (var tenant in tenants)
            {
                try
                {
                    await ProcessTenantAsync(tenant, stoppingToken);
                }
                catch (Exception ex)
                {
                    SLog.Error(ex, "Error processing tenant {Tenant}", tenant.TenantName);
                }
            }

            SLog.Information("Waiting {Minutes} minutes until next run...", intervalMinutes);
            await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
        }
    }


    /// <summary>
    /// GetLocalIPAddress
    /// </summary>
    /// <returns></returns>
    private string? GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return null;
    }

    /// <summary>
    /// ProcessTenantAsync
    /// </summary>
    /// <param name="tenant"></param>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>

    private async Task ProcessTenantAsync(TenantConfig tenant, CancellationToken stoppingToken)
    {
        SLog.Information("Starting job for tenant: {Tenant}", tenant.TenantName);

        try
        {
            // 1️ Build authentication URL
            string fullAuthUrl = BuildAuthUrl(tenant);

            // 2️ Authenticate and retrieve token
            var authResponse = await AuthenticateTenantAsync(tenant, fullAuthUrl, stoppingToken);

            if (authResponse == null || string.IsNullOrWhiteSpace(authResponse.access_token))
            {
                SLog.Error("Authentication failed or token missing for tenant {Tenant}", tenant.TenantName);
                return;
            }

            // 3️ Prepare directories
            var tenantFolder = PrepareTenantFolders(tenant);

            // 4 dummy data for testing
            var newFiles = new List<FileMetadata>
            {
                new() {
                    FileName = "IlliniosWater.mmpk",
                    LastModified = default
                },
                //new() {
                //    FileName = "Naperville.vtpk",
                //    LastModified = default
                //}
            };

            // var newFiles = await GetNewFilesFromApiAsync(tenant.NewFileListUrl ?? string.Empty, authResponse.access_token ?? string.Empty);

            // 5 In real scenario, fetch this list from an API endpoint
            await TestParallelDownloadSpeedAsync(newFiles, tenantFolder, tenant.TenantName, authResponse, tenant.FileListUrl, stoppingToken);

            SLog.Information("Completed job for tenant: {Tenant}", tenant.TenantName);
        }
        catch (Exception ex)
        {
            SLog.Error(ex, "Error processing tenant {Tenant}", tenant.TenantName);
        }
    }

    /// <summary>
    /// GetNewFilesFromApiAsync
    /// </summary>
    /// <param name="apiUrl"></param>
    /// <param name="lastDownload"></param>
    /// <param name="accessToken"></param>
    /// <returns></returns>
    private async Task<List<FileMetadata>> GetNewFilesFromApiAsync(string apiUrl, string accessToken)
    {
        using var client = new HttpClient();

        // ✅ Add Bearer token if required
        if (!string.IsNullOrEmpty(accessToken))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        try
        {
            var response = await client.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var allFiles = JsonSerializer.Deserialize<List<FileMetadata>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<FileMetadata>();

            return allFiles.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching files from API: {ApiUrl}", apiUrl);
            return new List<FileMetadata>();
        }
    }



    // ----------------- Helper Methods -----------------

    /// <summary>
    /// BuildAuthUrl
    /// </summary>
    /// <param name="tenant"></param>
    /// <returns></returns>
    private string BuildAuthUrl(TenantConfig tenant)
    {
        var builder = new UriBuilder(tenant.AuthUrl);
        var query = System.Web.HttpUtility.ParseQueryString(builder.Query);

        query["deviceModel"] = Environment.MachineName;
        query["appVersion"] = "KG.2025.T3";
        query["deviceToken"] = "";
        query["timeZone"] = TimeZoneInfo.Local.Id;
        query["osVersion"] = RuntimeInformation.OSDescription;
        query["latitude"] = "17.3707";
        query["longitude"] = "78.4050";
        query["deviceType"] = "5";
        query["deviceLang"] = CultureInfo.CurrentCulture.Name;
        query["deviceId"] = "";
        query["ipAddress"] = GetLocalIPAddress() ?? "0.0.0.0";
        query["loginFrom"] = "Web3.0";
        query["orgName"] = tenant.TenantName;
        query["isCompressedToken"] = "Y";

        builder.Query = query.ToString();
        return builder.ToString();
    }

    /// <summary>
    /// AuthenticateTenantAsync
    /// </summary>
    /// <param name="tenant"></param>
    /// <param name="authUrl"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private async Task<AuthResponse?> AuthenticateTenantAsync(TenantConfig tenant, string authUrl, CancellationToken token)
    {
        var request = new RestRequest(authUrl, Method.Post);

        // Add Basic Auth header manually
        var byteArray = Encoding.ASCII.GetBytes($"{tenant.Username}:{tenant.Password}");
        var base64Auth = Convert.ToBase64String(byteArray);
        request.AddHeader("Authorization", $"Basic {base64Auth}");

        // Optionally add body if needed (your original code sends null)
        // request.AddJsonBody(new { username = tenant.Username, password = tenant.Password });

        var response = await _restClient.ExecuteAsync(request, token);
        var content = response.Content;

        SLog.Information("Auth response for {Tenant}: {Response}", tenant.TenantName, content);
        if (!response.IsSuccessful || string.IsNullOrWhiteSpace(content)) return null;

        return JsonSerializer.Deserialize<AuthResponse?>(content);
    }

    /// <summary>
    /// PrepareTenantFolders
    /// </summary>
    private string PrepareTenantFolders(TenantConfig tenant)
    {
        string? MapsPath = _config["KGGISDownloadJob:Maps"];
        if (string.IsNullOrWhiteSpace(MapsPath))
        {
            throw new InvalidOperationException("Cache path is not configured.");
        }

        // Combine base app data + cache path + tenant name to isolate each tenant
        string tenantDownloadFileFolder = Path.Combine(_localAppData, MapsPath, tenant.TenantName);

        Directory.CreateDirectory(tenantDownloadFileFolder);
        return tenantDownloadFileFolder;
    }

    /// <summary>
    /// TestParallelDownloadSpeedAsync
    /// </summary>
    /// <param name="files"></param>
    /// <param name="tenantDownloadFolder"></param>
    /// <param name="tenantName"></param>
    /// <param name="authResponse"></param>
    /// <param name="fileListUrl"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task TestParallelDownloadSpeedAsync(
    List<FileMetadata> files,
    string tenantDownloadFolder,
    string tenantName,
    AuthResponse authResponse,
    string fileListUrl,
    CancellationToken cancellationToken)
    {
        // 1️⃣ Detect system cores
        int processorCount = Environment.ProcessorCount;

        // 2️⃣ Measure network speed asynchronously
        double networkSpeedMbps = await GetApproxNetworkSpeedAsync(cancellationToken);

        SLog.Information("Detected {Cores} CPU cores, network speed ≈ {Mbps:F2} Mbps", processorCount, networkSpeedMbps);

        bool canUseParallel = processorCount > 1 && networkSpeedMbps >= 10;

        // 3️⃣ Limit parallel downloads to 2–3 files max
        int maxParallelDownloads = canUseParallel ? Math.Min(files.Count, 3) : 1;
        double throttleBytesPerSec =  ((networkSpeedMbps * 0.7)/maxParallelDownloads) * 1024;  



        if (canUseParallel)
        {
            SLog.Information("✅ Using parallel downloading with {Count} concurrent tasks (2 Mbps per file)...", maxParallelDownloads);

            using var semaphore = new SemaphoreSlim(maxParallelDownloads);

            var downloadTasks = files.Select(async file =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    await DownloadFileAsync(
                        file.FileName,
                        file.LastModified,
                        tenantDownloadFolder,
                        tenantName,
                        authResponse,
                        fileListUrl,
                        throttleBytesPerSec,
                        cancellationToken);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(downloadTasks);
        }
        else
        {
            SLog.Information("⚠️ Using sequential download mode (slow link or single-core system)...");

            foreach (var file in files)
            {
                await DownloadFileAsync(
                    file.FileName,
                    file.LastModified,
                    tenantDownloadFolder,
                    tenantName,
                    authResponse,
                    fileListUrl,
                    throttleBytesPerSec,
                    cancellationToken);
            }
        }
    }

    //  Measure approximate network speed (simple)
    private async Task<double> GetApproxNetworkSpeedAsync(CancellationToken token)
    {
        var ni = NetworkInterface.GetAllNetworkInterfaces()
         .FirstOrDefault(n => n.OperationalStatus == OperationalStatus.Up);
        if (ni == null) return 0;

        var start = ni.GetIPv4Statistics().BytesReceived;
        var sw = Stopwatch.StartNew();
        Thread.Sleep(1000);
        var end = ni.GetIPv4Statistics().BytesReceived;

        double bps = (end - start) * 8.0 / sw.Elapsed.TotalSeconds;
        return Math.Round(bps / (1024 * 1024), 2);
    }

    // Throttled download (limit bandwidth per file)

    private async Task DownloadFileAsync(string fileName, long lastModified, string tenantDownloadFolder, string tenantName, AuthResponse authResponse, string fileListUrl, double maxSpeedMbps, CancellationToken cancellationToken)
    {
        string url = lastModified > 0
            ? $"{fileListUrl}?lastModifiedTime={lastModified}&fileName={Uri.EscapeDataString(fileName)}"
            : $"{fileListUrl}?fileName={Uri.EscapeDataString(fileName)}";

        string targetPath = Path.Combine(tenantDownloadFolder, fileName);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);

            bool isComplete = false;
            int resumeCount = 0;
            long existingLength;


            //will remove resumecount limit after testing
            while (!isComplete && resumeCount < 10)
            {
                existingLength = File.Exists(targetPath) ? new FileInfo(targetPath).Length : 0;

                var request = new RestRequest(url, Method.Get);
                request.AddHeader("Authorization", $"Bearer {authResponse.access_token}");

                if (existingLength > 0)
                    request.AddHeader("Range", $"bytes={existingLength}-");

                var response = await _restClient.ExecuteAsync(request, cancellationToken);

                if (!response.IsSuccessful || response.RawBytes == null)
                {
                    SLog.Warning("Failed to download {FileName}. Status: {Status}", fileName, response.StatusCode);
                    return;
                }

                // Parse headers
                string? eTag = response.Headers
                    .FirstOrDefault(h => h.Name.Equals("ETag", StringComparison.OrdinalIgnoreCase))?
                    .Value?.ToString()?.Trim('"');

                string? lastModifiedHeader = response.Headers
                    .FirstOrDefault(h => h.Name.Equals("lastModifiedTime", StringComparison.OrdinalIgnoreCase))?
                    .Value?.ToString();

                long lastModifiedTime = long.TryParse(lastModifiedHeader, out var parsedLmt) ? parsedLmt : 0;
                DateTime? serverLastModified = ConvertTimestampToDateTime(lastModifiedTime);

                if (serverLastModified.HasValue && File.Exists(targetPath))
                {
                    if (IsFileLocked(targetPath))
                    {
                        SLog.Warning("File {FileName} is currently in use. Skipping download.", fileName);
                        return;
                    }

                    if (IsLocalFileUpToDate(targetPath, serverLastModified.Value))
                    {
                        SLog.Information("File {FileName} is already up to date. Skipping download.", fileName);
                        return;
                    }
                }

                // Throttled write
                var fileMode = existingLength > 0 ? FileMode.Append : FileMode.Create;

                using (var fileStream = new FileStream(targetPath, fileMode, FileAccess.Write, FileShare.None))
                {
                    int bufferSize = 8192;//willl depend on network speed
                    double maxBytesPerSecond = (maxSpeedMbps * 1024 * 1024) / 8.0;
                    byte[] buffer = new byte[bufferSize];
                    var rawData = response.RawBytes;
                    int offset = 0;
                    var stopwatch = new Stopwatch();

                    while (offset < rawData.Length)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        int bytesToWrite = Math.Min(bufferSize, rawData.Length - offset);
                        Array.Copy(rawData, offset, buffer, 0, bytesToWrite);

                        stopwatch.Restart();
                        await fileStream.WriteAsync(buffer, 0, bytesToWrite, cancellationToken);
                        offset += bytesToWrite;

                        // Throttle logic
                        double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                        double expectedSeconds = bytesToWrite / maxBytesPerSecond;
                        if (expectedSeconds > elapsedSeconds)
                        {
                            int delayMs = (int)((expectedSeconds - elapsedSeconds) * 1000);
                            if (delayMs > 0)
                                await Task.Delay(delayMs, cancellationToken);
                        }
                    }
                }

                if (await IsFileFullyDownloadedAsync(response, targetPath))
                {
                    isComplete = true;
                }
                else
                {
                    resumeCount++;
                    long newLength = new FileInfo(targetPath).Length;
                    SLog.Warning("File {FileName} incomplete. Resuming... ({Bytes} bytes so far)", fileName, newLength);
                    await Task.Delay(1000 * resumeCount, cancellationToken);
                }

                if (isComplete)
                {
                    var fileInfo = new FileInfo(targetPath);
                    SLog.Information("✅ Downloaded {FileName} (ETag: {ETag}, Modified: {Modified}, Size: {Size} bytes)",
                        fileName, eTag, File.GetLastWriteTimeUtc(targetPath), fileInfo.Length);

                    await SaveDownloadMetadataAsync(fileName, tenantDownloadFolder, tenantName, eTag, lastModifiedTime);

                    if (serverLastModified.HasValue)
                        File.SetLastWriteTimeUtc(targetPath, serverLastModified.Value);
                }
            }

            if (!isComplete)
                SLog.Error("File {FileName} could not be fully downloaded even after retries.", fileName);
        }
        catch (OperationCanceledException)
        {
            SLog.Warning("Download of {FileName} for tenant {Tenant} was canceled.", fileName, tenantName);
            throw;
        }
        catch (Exception ex)
        {
            SLog.Error(ex, "Error downloading file {FileName} for tenant {Tenant}.", fileName, tenantName);
            throw;
        }
    }

    /// <summary>
    /// ConvertTimestampToDateTime
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    private DateTime ConvertTimestampToDateTime(long timestamp)
    {
        // If timestamp is in milliseconds (13 digits), use FromUnixTimeMilliseconds
        if (timestamp > 9999999999) // greater than 10 digits → milliseconds
            return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;

        // Otherwise, assume it’s in seconds
        return DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;
    }

    /// <summary>
    /// IsFileFullyDownloadedAsync
    /// </summary>
    /// <param name="response"></param>
    /// <param name="targetPath"></param>
    /// <returns></returns>
    private async Task<bool> IsFileFullyDownloadedAsync(RestResponse response, string targetPath)
    {
        // Try to get Content-Length header from response
        var contentLengthHeader = response.Headers
            .FirstOrDefault(h => h.Name.Equals("Content-Length", StringComparison.OrdinalIgnoreCase));

        if (contentLengthHeader?.Value is string contentLengthStr &&
            long.TryParse(contentLengthStr, out long expectedLength))
        {
            long actualLength = new FileInfo(targetPath).Length;
            return actualLength >= expectedLength;
        }

        // If Content-Length is not provided, fallback to other checks
        return true; // assume downloaded if stream completed
    }

    /// <summary>
    /// Example: persist the ETag and lastModifiedTime for next run (could be a JSON file, DB, etc.)
    /// </summary>
    private async Task SaveDownloadMetadataAsync(string fileName, string tenantFolder, string tenantName, string? eTag, long? lastModifiedTime)
    {
        if (string.IsNullOrEmpty(eTag) && !lastModifiedTime.HasValue)
            return;

        var metadataPath = Path.Combine(_localAppData, tenantFolder, Path.ChangeExtension(fileName, ".json"));

        var entry = new
        {
            FileName = fileName,
            Tenant = tenantName,
            ETag = eTag,
            LastModifiedTime = lastModifiedTime,
            SavedAt = DateTime.UtcNow
        };

        string json = System.Text.Json.JsonSerializer.Serialize(entry, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(metadataPath, json);
    }
    private bool IsFileLocked(string filePath)
    {
        if (!File.Exists(filePath)) return false;

        try
        {
            using FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            return false; // File is not locked
        }
        catch (IOException)
        {
            return true; // File is in use
        }
    }

    /// <summary>
    /// IsLocalFileUpToDate
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="serverLastModifiedUtc"></param>
    /// <returns></returns>
    private bool IsLocalFileUpToDate(string filePath, DateTime serverLastModifiedUtc)
    {
        if (!File.Exists(filePath)) return false;

        DateTime localLastModified = File.GetLastWriteTimeUtc(filePath);
        return localLastModified >= serverLastModifiedUtc;
    }
}