namespace SnapClicker.Services.Interfaces;

public interface ISystemTrayService : IDisposable
{
    void Initialize(Window mainWindow);
    void ShowNotification(string title, string message);
    void RemoveTrayIcon();
}
