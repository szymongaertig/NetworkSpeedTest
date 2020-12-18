using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NetworkSpeedTest
{
    public class Worker : IHostedService, IDisposable
    {
        private readonly ILogger<Worker> _logger;
        private readonly CronExpression _cronExpression;
        private readonly TimeZoneInfo _timeZoneInfo;
        private Timer _timer;
        private DateTime _lastRunTime;
        private TelemetryClient _telemetryClient;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, TelemetryClient telemetryClient)
        {
            _cronExpression = CronExpression.Parse(configuration.GetValue<string>("CronExpression"));
            _speedTestCmdBasePath = configuration.GetValue<string>("SpeedTestCmdBasePath");
            _speedTestCmdPath = string.IsNullOrEmpty(_speedTestCmdBasePath)
                ? "speedtest"
                : $"{_speedTestCmdBasePath}/speedtest";

            _logger = logger;
            _telemetryClient = telemetryClient;
            _timeZoneInfo = TimeZoneInfo.Local;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!SpeedTestCmdToolTest.Exists(_speedTestCmdPath))
                throw new Exception($"path {_speedTestCmdPath} does not apply to speedtest tool");
            _cancellationToken = new CancellationTokenSource();
            await ScheduleJob(cancellationToken);
        }

        public async Task ScheduleJob(CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.Now;
            var nextOccurence = _cronExpression.GetNextOccurrence(now, _timeZoneInfo);
            if (!nextOccurence.HasValue)
                return;
            var delay = nextOccurence.Value - now;
            _logger.LogInformation($"New job scheduled in {delay.Milliseconds} ms");
            _timer = new Timer(DoWork, null, delay, TimeSpan.FromMilliseconds(-1));
        }

        private CancellationTokenSource _cancellationToken;

        private void DoWork(object state)
        {
            _timer.Change(Timeout.Infinite, 0);
            _currentJob = DoWork(_cancellationToken.Token);
        }

        private Task _currentJob;
        private string _speedTestCmdBasePath;
        private string _speedTestCmdPath;

        private async Task DoWork(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
                await ExecuteTest(cancellationToken);
            if (!cancellationToken.IsCancellationRequested)
                await ScheduleJob(cancellationToken);
        }

        private async Task ExecuteTest(CancellationToken cancellationToken)
        {
            Console.WriteLine("Starting test");
            await ExecuteSpeedTest((result) =>
                {
                    _telemetryClient.TrackEvent("speed-measurement-executed", new Dictionary<string, string>
                    {
                        {nameof(result.Isp), result.Isp},
                        {nameof(result.Result.Url), result.Result.Url},
                        {nameof(result.Result.Id), result.Result.Id}
                    }, new Dictionary<string, double>
                    {
                        {nameof(result.Ping.Jitter), result.Ping.Jitter},
                        {nameof(result.Ping.Latency), result.Ping.Latency},
                        {"DownloadBandwidth", result.Download.Bandwidth},
                        {"UploadBandwidth", result.Upload.Bandwidth}
                    });
                },
                errorCallback: (error) =>
                {
                    _telemetryClient.TrackEvent("speed-measurement-interrupted", new Dictionary<string, string>()
                    {
                        {"msg", error}
                    });
                });
            Console.WriteLine("done");
        }

        async Task ExecuteSpeedTest(Action<SpeedTestResult> successCallback, Action<string> errorCallback)
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                FileName = $"{_speedTestCmdBasePath}speedtest",
                Arguments = "--format=json",
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.ASCII
            };
            process.OutputDataReceived += delegate(object sender, DataReceivedEventArgs eventArgs)
            {
                if (!string.IsNullOrEmpty(eventArgs.Data))
                {
                    Console.WriteLine($"Response received: {eventArgs.Data}");
                    try
                    {
                        var jsonResult = JsonSerializer.Deserialize<SpeedTestResult>(eventArgs.Data);
                        successCallback.Invoke(jsonResult);
                    }
                    catch (Exception ex)
                    {
                        errorCallback.Invoke($"Error during standard output deserialization. {ex.Message}");
                    }

                    Console.WriteLine("Done");
                }
            };
            process.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs eventArgs)
            {
                if (!string.IsNullOrEmpty(eventArgs.Data))
                {
                    errorCallback.Invoke(eventArgs.Data);
                }
            };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_currentJob != null)
            {
                try
                {
                    _cancellationToken.Cancel();
                }
                finally
                {
                    await Task.WhenAny(_currentJob, Task.Delay(Timeout.Infinite, cancellationToken));
                }
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}