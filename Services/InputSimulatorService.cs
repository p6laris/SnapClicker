namespace SnapClicker.Services
{
    /// <summary>
    /// Simulates user input actions (mouse and keyboard).
    /// </summary>
    public class InputSimulatorService : IInputSimulatorService, IDisposable
    {
        private readonly ILogger<InputSimulatorService> _logger;
        private readonly Stopwatch _stopwatch;
        private double _interval;
        private bool _isPreciseDelaysEnabled;
        private bool _isTimingJitterEnabled;
        private int _timingJitterRangeMs;
        private bool _isCoordinateJitterEnabled;
        private int _coordinateJitterRadiusPx;

        public InputSimulatorService(ILogger<InputSimulatorService> logger)
        {
            _logger = logger;
            _stopwatch = new Stopwatch();
            
            _interval = AppConfig.ActionInterval;
            _isPreciseDelaysEnabled = AppConfig.IsPreciseDelaysEnabled;
            _isTimingJitterEnabled = AppConfig.IsTimingJitterEnabled;
            _timingJitterRangeMs = AppConfig.TimingJitterRangeMs;
            _isCoordinateJitterEnabled = AppConfig.IsCoordinateJitterEnabled;
            _coordinateJitterRadiusPx = AppConfig.CoordinateJitterRadiusPx;
            
            WeakReferenceMessenger.Default.Register<ActionIntervalMessage>(this, (r,m) 
                => _interval = m.Value );
            
            WeakReferenceMessenger.Default.Register<PreciseDelayMessage>(this, (r,m) 
                => _isPreciseDelaysEnabled = m.Value );

            WeakReferenceMessenger.Default.Register<TimingJitterMessage>(this, (r, m) =>
            {
                _isTimingJitterEnabled = m.Value.Enabled;
                _timingJitterRangeMs = m.Value.RangeMs;
            });

            WeakReferenceMessenger.Default.Register<CoordinateJitterMessage>(this, (r, m) =>
            {
                _isCoordinateJitterEnabled = m.Value.Enabled;
                _coordinateJitterRadiusPx = m.Value.RadiusPx;
            });
        }
        
        /// <inheritdoc />
        public async ValueTask Simulate(List<RecordedAction> actions, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting input simulation for {Count} actions (TimingJitter={TimingJitter}, CoordJitter={CoordJitter}).", 
                actions.Count, _isTimingJitterEnabled, _isCoordinateJitterEnabled);
            var baseTime = actions.FirstOrDefault(a => !a.IsBurstMode)?.Timestamp ?? TimeSpan.Zero;
            _stopwatch.Restart();

            foreach (var action in actions)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Simulation canceled by user.");
                    break;
                }

                if (IsPanicFailsafeTriggered())
                {
                    _logger.LogWarning("Simulation aborted by emergency panic failsafe (Escape key / Corner).");
                    break;
                }

                if (action.IsBurstMode)
                {
                    var delayMs = action.Timestamp.TotalMilliseconds;
                    if (_isTimingJitterEnabled && delayMs > 0)
                    {
                        int jitter = Random.Shared.Next(-_timingJitterRangeMs, _timingJitterRangeMs + 1);
                        delayMs = Math.Max(1, delayMs + jitter);
                    }
                    if (delayMs > 0)
                        await WaitDurationAsync(delayMs, cancellationToken);
                }
                else
                {
                    var targetDelayMs = (action.Timestamp - baseTime).TotalMilliseconds;
                    if (_isTimingJitterEnabled && targetDelayMs > 0)
                    {
                        int jitter = Random.Shared.Next(-_timingJitterRangeMs, _timingJitterRangeMs + 1);
                        targetDelayMs = Math.Max(1, targetDelayMs + jitter);
                    }
                    await WaitAbsoluteAsync(targetDelayMs, cancellationToken);
                }

                if (cancellationToken.IsCancellationRequested || IsPanicFailsafeTriggered())
                    break;

                ExecuteAction(action);

                if (_interval > 0)
                {
                    var intervalMs = _interval;
                    if (_isTimingJitterEnabled)
                    {
                        int jitter = Random.Shared.Next(-_timingJitterRangeMs, _timingJitterRangeMs + 1);
                        intervalMs = Math.Max(1, intervalMs + jitter);
                    }
                    await WaitDurationAsync(intervalMs, cancellationToken);
                }
            }

            _stopwatch.Stop();
            _logger.LogInformation("Simulation finished. Total elapsed: {ElapsedMs} ms.", _stopwatch.ElapsedMilliseconds);
        }

        private const int VkEscape = 0x1B;

        private static bool IsPanicFailsafeTriggered()
        {
            // 1. Check if Escape key is pressed
            if ((Methods.GetAsyncKeyState(VkEscape) & 0x8000) != 0)
                return true;

            // 2. Check top-left corner failsafe (X <= 5 && Y <= 5)
            if (Methods.GetCursorPos(out var pt))
            {
                return pt.X <= 5 && pt.Y <= 5;
            }

            return false;
        }

        private async ValueTask WaitAbsoluteAsync(double targetElapsedMs, CancellationToken cancellationToken)
        {
            while (_stopwatch.Elapsed.TotalMilliseconds < targetElapsedMs)
            {
                if (cancellationToken.IsCancellationRequested || IsPanicFailsafeTriggered())
                    break;

                var remaining = targetElapsedMs - _stopwatch.Elapsed.TotalMilliseconds;
                if (!_isPreciseDelaysEnabled)
                {
                    if (remaining > 20)
                        await Task.Delay((int)Math.Min(remaining, 50), cancellationToken);
                    else if (remaining > 0)
                        await Task.Delay((int)remaining, cancellationToken);
                }
                else
                {
                    if (remaining > 2)
                        await Task.Delay((int)Math.Min(remaining, 16), cancellationToken);
                    else
                        Thread.SpinWait(20);
                }
            }
        }

        private async ValueTask WaitDurationAsync(double milliseconds, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed.TotalMilliseconds < milliseconds)
            {
                if (cancellationToken.IsCancellationRequested || IsPanicFailsafeTriggered())
                    break;

                var remaining = milliseconds - sw.Elapsed.TotalMilliseconds;
                if (!_isPreciseDelaysEnabled)
                {
                    if (remaining > 20)
                        await Task.Delay((int)Math.Min(remaining, 50), cancellationToken);
                    else if (remaining > 0)
                        await Task.Delay((int)remaining, cancellationToken);
                }
                else
                {
                    if (remaining > 2)
                        await Task.Delay(1, cancellationToken);
                    else
                        Thread.SpinWait(20);
                }
            }
        }
        private void ExecuteAction(RecordedAction action)
        {
            int x = action.X;
            int y = action.Y;

            if (_isCoordinateJitterEnabled && action.Type is not (ActionType.KeyDown or ActionType.KeyUp))
            {
                int offsetX = Random.Shared.Next(-_coordinateJitterRadiusPx, _coordinateJitterRadiusPx + 1);
                int offsetY = Random.Shared.Next(-_coordinateJitterRadiusPx, _coordinateJitterRadiusPx + 1);
                x = Math.Clamp(x + offsetX, 0, (int)SystemParameters.VirtualScreenWidth);
                y = Math.Clamp(y + offsetY, 0, (int)SystemParameters.VirtualScreenHeight);
            }

            switch (action.Type)
            {
                case ActionType.LeftMouseClick:
                    MouseHook.SimulateLeftClick(x, y);
                    break;
                case ActionType.LeftMouseDown:
                    MouseHook.SimulateLeftDown(x, y);
                    break;
                case ActionType.LeftMouseUp:
                    MouseHook.SimulateLeftUp(x, y);
                    break;
                case ActionType.RightMouseClick:
                    MouseHook.SimulateRightClick(x, y);
                    break;
                case ActionType.RightMouseDown:
                    MouseHook.SimulateRightDown(x, y);
                    break;
                case ActionType.RightMouseUp:
                    MouseHook.SimulateRightUp(x, y);
                    break;
                case ActionType.MiddleMouseClick:
                    MouseHook.SimulateMiddleClick(x, y);
                    break;
                case ActionType.MiddleMouseDown:
                    MouseHook.SimulateMiddleDown(x, y);
                    break;
                case ActionType.MiddleMouseUp:
                    MouseHook.SimulateMiddleUp(x, y);
                    break;
                case ActionType.MouseMove:
                    Methods.SetCursorPos(x, y);
                    break;
                case ActionType.KeyDown:
                    KeyboardHook.SimulateKeyDown(action.Key);
                    break;
                case ActionType.KeyUp:
                    KeyboardHook.SimulateKeyUp(action.Key);
                    break;
            }
        }
        private void SetCursorPositionToCenter()
        {
            var (screenWidth, screenHeight) = (SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight);
            Methods.SetCursorPos((int)(screenWidth / 2), (int)(screenHeight / 2));
        }

        public void Dispose()
        {
            WeakReferenceMessenger.Default.Unregister<ActionIntervalMessage>(this);
            WeakReferenceMessenger.Default.Unregister<PreciseDelayMessage>(this);
            WeakReferenceMessenger.Default.Unregister<TimingJitterMessage>(this);
            WeakReferenceMessenger.Default.Unregister<CoordinateJitterMessage>(this);
        }
    }
    
}
