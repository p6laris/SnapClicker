namespace SnapClicker.ViewModels.Controls
{
    public partial class KeyBindingDialogViewModel : ObservableObject, IDisposable
    {
        private readonly IKeyboardTrackerService _keyboardTrackerService;
        
        public readonly ObservableHashSet<Key> PressedKeys = new();
        public INotifyCollectionChangedSynchronizedViewList<Key> KeysView { get; set; }

        [ObservableProperty] private bool _isValid = true;
        public KeyBindingDialogViewModel(IKeyboardTrackerService keyboardTrackerService)
        {
            _keyboardTrackerService = keyboardTrackerService;
            KeysView = PressedKeys.ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);
            
            _keyboardTrackerService.OnKeyDownOrUp += KeyboardTrackerOnKeyPress;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void KeyboardTrackerOnKeyPress(Key key) => RegisterKeys(key);
        
        public void StartTracking() => _keyboardTrackerService.StartTracking();
        public void StopTracking() => _keyboardTrackerService.StopTracking();
        
        // Keys that should NOT be part of a shortcut
        private static readonly HashSet<Key> InvalidKeys = new()
        {
            Key.Space, Key.Enter, Key.Tab, Key.Escape, Key.Apps, 
            Key.CapsLock, Key.NumLock, Key.Scroll, Key.PrintScreen, 
            Key.Pause, Key.Clear, Key.OemClear
        };

        public void RegisterKeys(Key key)
        {
            if(key == Key.Space)
                PressedKeys.Clear();
            
            if (PressedKeys.Count > 3)
                PressedKeys.Clear();
            
            if (IsInvalidKey(key) || PressedKeys.Contains(key))
                return;
            PressedKeys.Add(key);

            if (!IsValidCombination())
            {
                IsValid = false;
                return;
            }

            IsValid = true;
        }

        private bool IsValidCombination()
        {
            bool allModifiers = PressedKeys.All(IsModifierKey);
            bool firstIsModifier = IsModifierKey(PressedKeys.First());
            int modifierCount = PressedKeys.Count(k => IsModifierKey(k));
            int nonModifierCount = PressedKeys.Count - modifierCount;

            // Starts with non-modifier
            if (!firstIsModifier)
                return false;

            // Only modifiers, e.g. Ctrl+Shift
            if (allModifiers)
                return false;

            // Has more than one non-modifier
            if (nonModifierCount > 1)
                return false;

            // Valid only if starts with modifier and includes exactly one non-modifier
            return PressedKeys.Count > 1 &&
                   firstIsModifier &&
                   nonModifierCount == 1;
        }
        private bool IsModifierKey(Key key)
        {
            return key == Key.LeftShift || key == Key.RightShift
                || key == Key.LeftCtrl || key == Key.RightCtrl
                || key == Key.LeftAlt || key == Key.RightAlt
                || key == Key.LWin || key == Key.RWin;
        }
        public void Reset()
        {
            PressedKeys.Clear();
        }
        private bool IsInvalidKey(Key key)
        {
            return InvalidKeys.Contains(key);
        }

        public void Dispose()
        {
            KeysView.Dispose();
            _keyboardTrackerService.OnKeyDownOrUp -= KeyboardTrackerOnKeyPress;
        }
    }
}
