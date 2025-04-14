using System.Text.Json;

namespace SnapClicker.Services
{
    public static class AppConfig
    {
        private static readonly Lock _lock = new Lock();
        private static readonly string ConfigDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SnapClicker");
        private static readonly string ConfigFilePath = Path.Combine(ConfigDirectory, "settings.json");

        private static Dictionary<string, string> _cache = new();
        private static bool _isCacheLoaded = false;

        static AppConfig()
        {
            if (!Directory.Exists(ConfigDirectory))
            {
                Directory.CreateDirectory(ConfigDirectory);
            }

            if (!File.Exists(ConfigFilePath))
            {
                File.WriteAllText(ConfigFilePath, JsonSerializer.Serialize(new Dictionary<string, string>()));
            }
            
            LoadCache();
        }
        
        public static bool IsPreciseDelaysEnabled 
        {
            get => bool.TryParse(GetAppSetting("IsPreciseDelaysEnabled"), out var value) && value;
            set => UpdateAppSetting("IsPreciseDelaysEnabled", value.ToString());
        }
        public static string ToUpdateVersion 
        {
            get => $"SnapClicker v{GetAppSetting("ToUpdateVersion") ?? string.Empty}";
            set => UpdateAppSetting("ToUpdateVersion", value);
        }
        public static string ReleaseNotesLink
        {
            get
            {
                var link = GetAppSetting("ReleaseNotesLink");
                var baseUrl = $"https://github.com/p6laris/SnapClicker/";

                if (string.IsNullOrEmpty(link))
                    return baseUrl;
                
                return $"{baseUrl}releases/tag/v{link}";
            } 
            set => UpdateAppSetting("ReleaseNotesLink", value);
        }
        public static bool IsUpdateAvailable 
        {
            get => bool.TryParse(GetAppSetting("IsUpdateAvailable"), out var value) && value;
            set => UpdateAppSetting("IsUpdateAvailable", value.ToString());
        }
        public static bool IsReleaseNotesAvailable 
        {
            get => bool.TryParse(GetAppSetting("IsReleaseNotesAvailable"), out var value) && value;
            set => UpdateAppSetting("IsReleaseNotesAvailable", value.ToString());
        }
        public static DateTime LastCheckedUpdate
        {
            get => DateTime.TryParse(GetAppSetting("LastCheckedUpdate"), out var value) ? value : DateTime.Now;
            set => UpdateAppSetting("LastCheckedUpdate", value.ToString("o"));
        }
        public static ApplicationTheme Theme 
        {
            get
            {
                var theme = GetAppSetting("Theme");
                if (string.IsNullOrEmpty(theme))
                    return ApplicationTheme.Dark;

                return Enum.TryParse(theme, true, out ApplicationTheme th) ? th : ApplicationTheme.Dark;
            }
            set => UpdateAppSetting("Theme", value.ToString());
        }

        public static double ActionInterval
        {
            get => double.TryParse(GetAppSetting("ActionInterval"), out var value) ? value : 1;
            set => UpdateAppSetting("ActionInterval", value.ToString());
        }
        public static bool IsMouseMoveRecordingSet
        {
            get => bool.TryParse(GetAppSetting("IsMouseMoveRecordingSet"), out var value) && value;
            set => UpdateAppSetting("IsMouseMoveRecordingSet", value.ToString());
        }
        public static KeyBindingModel StartAndStopKeyBinding
        {
            get
            {
                var shortcut = GetAppSetting("StartAndStopKeyBinding");
                if (string.IsNullOrEmpty(shortcut))
                    return new KeyBindingModel(Key.S, ModifierKeys.Control | ModifierKeys.Shift);

                return ParseKeyBinding(shortcut);

            }
            set => UpdateAppSetting("StartAndStopKeyBinding", value.ToString());
        }
        
        public static KeyBindingModel PlayAndStopKeyBinding
        {
            get
            {
                var shortcut = GetAppSetting("PlayAndStopKeyBinding");
                
                if (string.IsNullOrEmpty(shortcut))
                    return new KeyBindingModel(Key.P, ModifierKeys.Control);

                return ParseKeyBinding(shortcut);
            }
            set => UpdateAppSetting("PlayAndStopKeyBinding", value.ToString());
            
        }
        private static void LoadCache()
        {
            lock (_lock)
            {
                try
                {
                    var json = File.ReadAllText(ConfigFilePath);
                    _cache = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
                    _isCacheLoaded = true;
                }
                catch
                {
                }
            }
        }

        private static string? GetAppSetting(string key)
        {
            lock (_lock)
            {
                if (!_isCacheLoaded)
                {
                    LoadCache();
                }

                return _cache.TryGetValue(key, out var value) ? value : null;
            }
        }

        private static void UpdateAppSetting(string key, string value)
        {
            lock (_lock)
            {
                try
                {
                    _cache[key] = value;
                    var json = JsonSerializer.Serialize(_cache, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(ConfigFilePath, json);
                }
                catch 
                {
                }
            }
        }

        private static KeyBindingModel ParseKeyBinding(ReadOnlySpan<char> keyBinding)
        {
            var keys = keyBinding.Split(',');
                
            Key key = Key.None;
            ModifierKeys modifierKeys = ModifierKeys.None;
                
            int i = 0;
            foreach (var k in keys)
            {
                var keyToken = keyBinding[k].TrimStart();
                    
                if (i == 0)
                    key = Enum.Parse<Key>(keyToken, true);
                else
                    modifierKeys |= Enum.Parse<ModifierKeys>(keyToken, true);

                i++;
            }

            return new KeyBindingModel(key, modifierKeys);
        }
    }
}
