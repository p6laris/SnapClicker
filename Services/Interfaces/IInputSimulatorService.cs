namespace SnapClicker.Services;

public interface IInputSimulatorService
{
    /// <summary>
    /// Executes a sequence of recorded input actions with precise timing.
    /// </summary>
    /// <param name="actions">The sequence of input actions to simulate</param>
    /// <param name="cancellationToken">Token to cancel simulation mid-execution</param>
    /// <returns>A ValueTask representing the asynchronous operation</returns>
    /// <remarks>
    /// <para>Behavior details:</para>
    /// <list type="bullet">
    /// <item>Resets cursor to screen center before starting</item>
    /// <item>Maintains exact timing between actions</item>
    /// <item>Respects cancellation requests immediately</item>
    /// <item>Applies configured action interval between inputs</item>
    /// </list>
    /// <para>Supported action types:</para>
    /// <list type="bullet">
    /// <item>All mouse button clicks (left/right/middle)</item>
    /// <item>Mouse movement</item>
    /// <item>Key presses (down/up)</item>
    /// </list>
    /// </remarks>
    ValueTask Simulate(List<RecordedAction> actions, CancellationToken cancellationToken);
}