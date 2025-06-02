using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace TP.ConcurrentProgramming.Data
{
    internal class DiagnosticLogger
    {
        private static readonly Lazy<DiagnosticLogger> instance = new(() => new DiagnosticLogger());
        private readonly ConcurrentQueue<BallState> logBuffer = new();
        private readonly CancellationTokenSource cancellationTokenSource = new();
        private readonly string logFilePath;
        private Task? writeTask;
        private readonly object lockObject = new();
        private const int MAX_BUFFER_SIZE = 1024;
        private bool isDisposed;
        private bool isFirstWrite = true;

        private DiagnosticLogger()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string diagnosticsPath = Path.Combine(baseDirectory, "..", "..", "..", "..", "Diagnostics");
            Directory.CreateDirectory(diagnosticsPath);
            logFilePath = Path.Combine(diagnosticsPath, "diagnostic_log.json");
            
            if (!File.Exists(logFilePath))
            {
                File.WriteAllText(logFilePath, "[\n");
            }
            else
            {
                string content = File.ReadAllText(logFilePath).Trim();
                if (string.IsNullOrWhiteSpace(content))
                {
                    File.WriteAllText(logFilePath, "[\n");
                }
                else if (!content.EndsWith("]"))
                {
                    isFirstWrite = false;
                }
                else
                {
                    File.WriteAllText(logFilePath, content.TrimEnd(']') + "\n");
                    isFirstWrite = false;
                }
            }
            
            StartWriteTask();
        }

        public static DiagnosticLogger Instance => instance.Value;

        public void LogBallState(IBall ball, string message = "")
        {
            if (isDisposed) return;

            var ballState = new BallState(
                DateTime.Now,
                ball.Id,
                ball.Position,
                ball.Velocity,
                message
            );

            logBuffer.Enqueue(ballState);

            if (logBuffer.Count > MAX_BUFFER_SIZE)
            {
                FlushBuffer();
            }
        }

        private void StartWriteTask()
        {
            writeTask = Task.Run(async () =>
            {
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    await Task.Delay(1000, cancellationTokenSource.Token);
                    FlushBuffer();
                }
            }, cancellationTokenSource.Token);
        }

        private void FlushBuffer()
        {
            if (logBuffer.IsEmpty) return;

            lock (lockObject)
            {
                var states = new List<BallState>();

                while (logBuffer.TryDequeue(out BallState? state))
                {
                    states.Add(state);
                }

                if (states.Count == 0) return;

                try
                {
                    var json = string.Join(",\n", states.Select(s => s.ToString()));
                    
                    if (isFirstWrite)
                    {
                        File.AppendAllText(logFilePath, json + "\n");
                        isFirstWrite = false;
                    }
                    else
                    {
                        File.AppendAllText(logFilePath, "," + json + "\n");
                    }
                }
                catch (IOException)
                {
                    foreach (var state in states)
                    {
                        logBuffer.Enqueue(state);
                    }
                }
            }
        }

        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;

            cancellationTokenSource.Cancel();
            FlushBuffer();
            writeTask?.Wait(1000);

            try
            {
                File.AppendAllText(logFilePath, "]");
            }
            catch 
            {
                Console.WriteLine($"An error with closing the file");
            }

            cancellationTokenSource.Dispose();
        }
    }
} 