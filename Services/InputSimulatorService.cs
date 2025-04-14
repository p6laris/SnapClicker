namespace SnapClicker.Services
{
    /// <summary>
    /// Simulates user input actions (mouse and keyboard).
    /// </summary>
    public class InputSimulatorService : IInputSimulatorService, IDisposable
    {
        private readonly Stopwatch _stopwatch;
        private double _interval;
        private bool _isPreciseDelaysEnabled;

        public InputSimulatorService()
        {
            _stopwatch = new Stopwatch();
            
            _interval = AppConfig.ActionInterval;
            _isPreciseDelaysEnabled = AppConfig.IsPreciseDelaysEnabled;
            
            WeakReferenceMessenger.Default.Register<ActionIntervalMessage>(this, (r,m) 
                => _interval = m.Value );
            
            WeakReferenceMessenger.Default.Register<PreciseDelayMessage>(this, (r,m) 
                => _isPreciseDelaysEnabled = m.Value );
        }
        
        /// <inheritdoc />
        public async ValueTask Simulate(List<RecordedAction> actions, CancellationToken cancellationToken)
        {
            var baseTime = actions.FirstOrDefault(a => !a.IsBurstMode)?.Timestamp ?? TimeSpan.Zero;
            _stopwatch.Restart();

            foreach (var action in actions)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var targetDelayMs = action.IsBurstMode
                    ? action.Timestamp.TotalMilliseconds
                    : (action.Timestamp - baseTime).TotalMilliseconds;

                await WaitUntilAsync(targetDelayMs, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    break;

                ExecuteAction(action);

                if (_interval > 0)
                    await WaitUntilAsync(_interval, cancellationToken, isInterval: true);
            }

            _stopwatch.Stop();
        }
        private async ValueTask WaitUntilAsync(double milliseconds, CancellationToken cancellationToken, bool isInterval = false)
        {
            if (!_isPreciseDelaysEnabled)
            {
                // Coarse delay (original fallback)
                var remaining = milliseconds - (isInterval ? 0 : _stopwatch.Elapsed.TotalMilliseconds);
                if (remaining > 0)
                    await Task.Delay((int)remaining, cancellationToken);
                return;
            }

            // PRECISE DELAY MODE (high accuracy)
            if (isInterval)
            {
                // For intervals, use a separate Stopwatch (since they're relative to action completion)
                var sw = Stopwatch.StartNew();
                while (sw.Elapsed.TotalMilliseconds < milliseconds)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    var remaining = milliseconds - sw.Elapsed.TotalMilliseconds;
                    if (remaining > 2) 
                        await Task.Delay(1, cancellationToken);
                    else 
                        Thread.SpinWait(20);
                }
            }
            else
            {
                // For action timing, sync with the main Stopwatch
                while (_stopwatch.Elapsed.TotalMilliseconds < milliseconds)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    var remaining = milliseconds - _stopwatch.Elapsed.TotalMilliseconds;
                    if (remaining > 2)
                        await Task.Delay((int)Math.Min(remaining, 16), cancellationToken);
                    else
                        Thread.SpinWait(20);
                }
            }
        }
        private void ExecuteAction(RecordedAction action)
        {
            switch (action.Type)
            {
                case ActionType.LeftMouseClick:
                    MouseHook.SimulateLeftClick(action.X, action.Y);
                    break;
                case ActionType.RightMouseClick:
                    MouseHook.SimulateRightClick(action.X, action.Y);
                    break;
                case ActionType.MiddleMouseClick:
                    MouseHook.SimulateMiddleClick(action.X, action.Y);
                    break;
                case ActionType.MouseMove:
                    Methods.SetCursorPos(action.X, action.Y);
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
        }
    }
    
}
