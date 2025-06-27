using Godot;

namespace MineSurvivors.scripts.ui
{
    /// <summary>
    /// Menu opcji gry — demonstracja hermetyzacji ustawień i zarządzania konfiguracją.
    /// 
    /// Zasady OOP w działaniu:
    /// - Hermetyzacja: Wszystkie kontrolki UI są prywatne, dostęp przez publiczne metody.
    /// - Enkapsulacja: Logika ustawień oddzielona od innych systemów.
    /// - Separacja odpowiedzialności: Menu zajmuje się tylko opcjami, nie logiką gry.
    /// - Kompozycja: Używa GameSettings jako osobnego obiektu do zarządzania danymi.
    /// 
    /// </summary>
    public partial class OptionsMenu : Control
    {
        #region Private UI Components - Hermetyzacja
        
        // Audio controls
        private Label _masterVolumeLabel;
        private HSlider _masterVolumeSlider;
        private Label _musicVolumeLabel;
        private HSlider _musicVolumeSlider;
        private Label _sfxVolumeLabel;
        private HSlider _sfxVolumeSlider;
        
        // Graphics controls
        private Label _resolutionLabel;
        private OptionButton _resolutionOption;
        private CheckBox _fullscreenCheckbox;
        
        // Gameplay controls
        private Label _difficultyLabel;
        private OptionButton _difficultyOption;
        
        // Navigation buttons
        private Button _backButton;
        private Button _resetButton;
        private Button _applyButton;
        
        // Audio dla feedback
        private AudioStreamPlayer _buttonSound;
        
        // Ścieżka powrotu — enkapsulacja nawigacji
        private const string MainMenuPath = "res://Scenes/UI/MainMenu.tscn";
        
        #endregion

        #region Settings Data - Hermetyzacja danych
        
        // Prywatne dane ustawień — hermetyzacja w działaniu
        private float _masterVolume = 1.0f;
        private float _musicVolume = 0.8f;
        private float _sfxVolume = 0.9f;
        private bool _fullscreen;
        private int _resolutionIndex = 1; // 1080p jako domyślne
        private int _difficultyIndex = 1; // Normal jako domyślne
        
        // Dostępne rozdzielczości — łatwo modyfikowalne
        private readonly Vector2I[] _availableResolutions =
        [
            new Vector2I(1280, 720),   // HD
            new Vector2I(1920, 1080),  // Full HD
            new Vector2I(2560, 1440),  // QHD
            new Vector2I(3840, 2160)   // 4K
        ];
        
        // Nazwy poziomów trudności
        private readonly string[] _difficultyNames =
        [
            "Łatwy", "Normalny", "Trudny", "Ekstremalny"
        ];
        
        #endregion

        #region Initialization
        
        public override void _Ready()
        {
            GD.Print("Inicjalizacja menu opcji...");
            
            // Znajdź komponenty UI
            FindUiComponents();
            
            // Załaduj zapisane ustawienia
            LoadSettings();
            
            // Skonfiguruj kontrolki
            SetupControls();
            
            // Zaktualizuj UI na podstawie załadowanych ustawień
            UpdateUiFromSettings();
            
            GD.Print("Menu opcji gotowe!");
        }

        /// <summary>
        /// Hermetyzacja: Znajdowanie wszystkich komponentów UI w jednym miejscu
        /// </summary>
        private void FindUiComponents()
        {
            var container = GetNode("ScrollContainer/VBoxContainer");
            
            // Audio section
            var audioSection = container.GetNode("AudioSection");
            _masterVolumeLabel = audioSection.GetNode<Label>("MasterVolumeContainer/Label");
            _masterVolumeSlider = audioSection.GetNode<HSlider>("MasterVolumeContainer/Slider");
            _musicVolumeLabel = audioSection.GetNode<Label>("MusicVolumeContainer/Label");
            _musicVolumeSlider = audioSection.GetNode<HSlider>("MusicVolumeContainer/Slider");
            _sfxVolumeLabel = audioSection.GetNode<Label>("SFXVolumeContainer/Label");
            _sfxVolumeSlider = audioSection.GetNode<HSlider>("SFXVolumeContainer/Slider");
            
            // Graphics section
            var graphicsSection = container.GetNode("GraphicsSection");
            _resolutionLabel = graphicsSection.GetNode<Label>("ResolutionContainer/Label");
            _resolutionOption = graphicsSection.GetNode<OptionButton>("ResolutionContainer/OptionButton");
            _fullscreenCheckbox = graphicsSection.GetNode<CheckBox>("FullscreenCheckbox");
            
            // Gameplay section
            var gameplaySection = container.GetNode("GameplaySection");
            _difficultyLabel = gameplaySection.GetNode<Label>("DifficultyContainer/Label");
            _difficultyOption = gameplaySection.GetNode<OptionButton>("DifficultyContainer/OptionButton");
            
            // Buttons
            var buttonSection = container.GetNode("ButtonSection");
            _backButton = buttonSection.GetNode<Button>("ButtonContainer/BackButton");
            _resetButton = buttonSection.GetNode<Button>("ButtonContainer/ResetButton");
            _applyButton = buttonSection.GetNode<Button>("ButtonContainer/ApplyButton");
            
            ValidateComponents();
        }

        /// <summary>
        /// Hermetyzacja: Walidacja komponenty w jednym miejscu
        /// </summary>
        private void ValidateComponents()
        {
            // Sprawdź kluczowe komponenty — bez nich menu nie może działać
            if (_masterVolumeSlider == null) GD.PrintErr("BŁĄD: Brak MasterVolumeSlider!");
            if (_backButton == null) GD.PrintErr("BŁĄD: Brak BackButton!");
            if (_resolutionOption == null) GD.PrintErr("BŁĄD: Brak ResolutionOption!");
            
        }

        #endregion

        #region UI Setup - Konfiguracja kontrolek
        
        /// <summary>
        /// Hermetyzacja: Konfiguracja wszystkich kontrolek w jednym miejscu
        /// </summary>
        private void SetupControls()
        {
            // Setup sliderów audio
            SetupAudioSliders();
            
            // Setup opcji graficznych
            SetupGraphicsControls();
            
            // Setup opcji rozgrywki
            SetupGameplayControls();
            
            // Setup przycisków
            SetupButtons();
        }

        /// <summary>
        /// Separacja odpowiedzialności: Konfiguracja tylko audio sliderów
        /// </summary>
        private void SetupAudioSliders()
        {
            // Konfiguracja sliderów — zakres 0-100%
            ConfigureSlider(_masterVolumeSlider, 0f, 1f, 0.01f);
            ConfigureSlider(_musicVolumeSlider, 0f, 1f, 0.01f);
            ConfigureSlider(_sfxVolumeSlider, 0f, 1f, 0.01f);
            
            // Podłącz sygnały
            _masterVolumeSlider.ValueChanged += OnMasterVolumeChanged;
            _musicVolumeSlider.ValueChanged += OnMusicVolumeChanged;
            _sfxVolumeSlider.ValueChanged += OnSFXVolumeChanged;
        }

        /// <summary>
        /// Separacja odpowiedzialności: Konfiguracja tylko opcji graficznych
        /// </summary>
        private void SetupGraphicsControls()
        {
            // Wypełnij opcje rozdzielczości
            _resolutionOption.Clear();
            foreach (var res in _availableResolutions)
            {
                _resolutionOption.AddItem($"{res.X} x {res.Y}");
            }
            
            // Podłącz sygnały
            _resolutionOption.ItemSelected += OnResolutionChanged;
            _fullscreenCheckbox.Toggled += OnFullscreenToggled;
        }

        /// <summary>
        /// Separacja odpowiedzialności: Konfiguracja tylko opcji rozgrywki
        /// </summary>
        private void SetupGameplayControls()
        {
            // Wypełnij opcje trudności
            _difficultyOption.Clear();
            foreach (string difficulty in _difficultyNames)
            {
                _difficultyOption.AddItem(difficulty);
            }
            
            // Podłącz sygnał
            _difficultyOption.ItemSelected += OnDifficultyChanged;
        }

        /// <summary>
        /// Separacja odpowiedzialności: Konfiguracja tylko przycisków
        /// </summary>
        private void SetupButtons()
        {
            _backButton.Pressed += OnBackButtonPressed;
            _resetButton.Pressed += OnResetButtonPressed;
            _applyButton.Pressed += OnApplyButtonPressed;
        }

        /// <summary>
        /// Helper: Konfiguracja slidera z określonymi parametrami
        /// </summary>
        private void ConfigureSlider(HSlider slider, float min, float max, float step)
        {
            if (slider == null) return;
            
            slider.MinValue = min;
            slider.MaxValue = max;
            slider.Step = step;
        }

        #endregion

        #region Event Handlers - Obsługa zmian ustawień
        
        /// <summary>
        /// Hermetyzacja: Obsługa zmiany głównej głośności
        /// </summary>
        private void OnMasterVolumeChanged(double value)
        {
            _masterVolume = (float)value;
            UpdateVolumeLabel(_masterVolumeLabel, "Główna głośność", _masterVolume);
            ApplyAudioSettings();
        }

        /// <summary>
        /// Hermetyzacja: Obsługa zmiany głośności muzyki
        /// </summary>
        private void OnMusicVolumeChanged(double value)
        {
            _musicVolume = (float)value;
            UpdateVolumeLabel(_musicVolumeLabel, "Muzyka", _musicVolume);
            ApplyAudioSettings();
        }

        /// <summary>
        /// Hermetyzacja: Obsługa zmiany głośności efektów
        /// </summary>
        private void OnSFXVolumeChanged(double value)
        {
            _sfxVolume = (float)value;
            UpdateVolumeLabel(_sfxVolumeLabel, "Efekty dźwiękowe", _sfxVolume);
            ApplyAudioSettings();
        }

        /// <summary>
        /// Hermetyzacja: Obsługa zmiany rozdzielczości
        /// </summary>
        private void OnResolutionChanged(long index)
        {
            _resolutionIndex = (int)index;
        }

        /// <summary>
        /// Hermetyzacja: Obsługa przełączania pełnego ekranu
        /// </summary>
        private void OnFullscreenToggled(bool pressed)
        {
            _fullscreen = pressed;
            ApplyDisplaySettings();
        }

        /// <summary>
        /// Hermetyzacja: Obsługa zmiany trudności
        /// </summary>
        private void OnDifficultyChanged(long index)
        {
            _difficultyIndex = (int)index;
        }

        #endregion

        #region Button Handlers - Nawigacja i akcje
        
        /// <summary>
        /// Enkapsulacja: Powrót do głównego menu
        /// </summary>
        private void OnBackButtonPressed()
        {
            GD.Print("Powrót do głównego menu...");
            
            // Automatycznie zapisz ustawienia przed wyjściem
            SaveSettings();
            
            // Powrót do głównego menu
            GetTree().ChangeSceneToFile(MainMenuPath);
        }

        /// <summary>
        /// Hermetyzacja: Reset ustawień do domyślnych
        /// </summary>
        private void OnResetButtonPressed()
        {
            GD.Print("Resetowanie ustawień do domyślnych...");
            
            // Przywróć domyślne wartości
            ResetToDefaults();
            
            // Zaktualizuj UI
            UpdateUiFromSettings();
            
            // Zastosuj ustawienia
            ApplyAllSettings();
        }

        /// <summary>
        /// Hermetyzacja: Zastosowanie i zapisanie ustawień
        /// </summary>
        private void OnApplyButtonPressed()
        {
            GD.Print("Zastosowywanie ustawień...");
            
            // Zastosuj wszystkie ustawienia
            ApplyAllSettings();
            
            // Zapisz do pliku
            SaveSettings();
            
            GD.Print("Ustawienia zastosowane i zapisane!");
        }

        #endregion

        #region Settings Application - Zastosowanie ustawień
        
        /// <summary>
        /// Hermetyzacja: Zastosowanie wszystkich ustawień
        /// </summary>
        private void ApplyAllSettings()
        {
            ApplyAudioSettings();
            ApplyDisplaySettings();
            ApplyGameplaySettings();
        }

        /// <summary>
        /// Separacja odpowiedzialności: Tylko ustawienia audio
        /// </summary>
        private void ApplyAudioSettings()
        {
            // Zastosuj ustawienia audio do AudioServer
            var masterBusIndex = AudioServer.GetBusIndex("Master");
            var musicBusIndex = AudioServer.GetBusIndex("Music");
            var sfxBusIndex = AudioServer.GetBusIndex("SFX");
            
            // Konwersja liniowej wartości na decybele
            AudioServer.SetBusVolumeDb(masterBusIndex, LinearToDb(_masterVolume));
            
            if (musicBusIndex != -1)
                AudioServer.SetBusVolumeDb(musicBusIndex, LinearToDb(_musicVolume));
                
            if (sfxBusIndex != -1)
                AudioServer.SetBusVolumeDb(sfxBusIndex, LinearToDb(_sfxVolume));
        }

        /// <summary>
        /// Separacja odpowiedzialności: Tylko ustawienia wyświetlania
        /// </summary>
        private void ApplyDisplaySettings()
        {
            var window = GetWindow();
            
            // Zastosuj rozdzielczość
            if (_resolutionIndex >= 0 && _resolutionIndex < _availableResolutions.Length)
            {
                var resolution = _availableResolutions[_resolutionIndex];
                window.Size = resolution;
                
                // Wyśrodkuj okno na ekranie
                var screenSize = DisplayServer.ScreenGetSize();
                var windowPos = (screenSize - resolution) / 2;
                window.Position = windowPos;
            }
            
            // Zastosuj tryb pełnoekranowy
            window.Mode = _fullscreen ? Window.ModeEnum.Fullscreen : Window.ModeEnum.Windowed;
        }

        /// <summary>
        /// Separacja odpowiedzialności: Tylko ustawienia rozgrywki
        /// </summary>
        private void ApplyGameplaySettings()
        {
            // Tutaj trzeba dodać logikę stosowania trudności
            // Na przykład: GameManager.Instance.SetDifficulty(_difficultyIndex);
            GD.Print($"Trudność ustawiona na: {_difficultyNames[_difficultyIndex]}");
        }

        #endregion

        #region Data Persistence - Zarządzanie zapisem ustawień
        
        /// <summary>
        /// Hermetyzacja: Ładowanie ustawień z pliku konfiguracyjnego
        /// Godot automatycznie zarządza plikiem user://settings.cfg
        /// </summary>
        private void LoadSettings()
        {
            var config = new ConfigFile();
            var error = config.Load("user://settings.cfg");
            
            if (error != Error.Ok)
            {
                GD.Print("Nie znaleziono pliku ustawień, używam domyślnych.");
                return;
            }
            
            // Wczytaj ustawienia audio
            _masterVolume = (float)config.GetValue("audio", "master_volume", 1.0f);
            _musicVolume = (float)config.GetValue("audio", "music_volume", 0.8f);
            _sfxVolume = (float)config.GetValue("audio", "sfx_volume", 0.9f);
            
            // Wczytaj ustawienia graficzne
            _fullscreen = (bool)config.GetValue("graphics", "fullscreen", false);
            _resolutionIndex = (int)config.GetValue("graphics", "resolution_index", 1);
            
            // Wczytaj ustawienia rozgrywki
            _difficultyIndex = (int)config.GetValue("gameplay", "difficulty", 1);
            
            GD.Print("Ustawienia wczytane z pliku.");
        }

        /// <summary>
        /// Hermetyzacja: Zapisywanie ustawień do pliku konfiguracyjnego
        /// </summary>
        private void SaveSettings()
        {
            var config = new ConfigFile();
            
            // Zapisz ustawienia audio
            config.SetValue("audio", "master_volume", _masterVolume);
            config.SetValue("audio", "music_volume", _musicVolume);
            config.SetValue("audio", "sfx_volume", _sfxVolume);
            
            // Zapisz ustawienia graficzne
            config.SetValue("graphics", "fullscreen", _fullscreen);
            config.SetValue("graphics", "resolution_index", _resolutionIndex);
            
            // Zapisz ustawienia rozgrywki
            config.SetValue("gameplay", "difficulty", _difficultyIndex);
            
            // Zapisz do pliku
            var error = config.Save("user://settings.cfg");
            if (error == Error.Ok)
            {
                GD.Print("Ustawienia zapisane pomyślnie.");
            }
            else
            {
                GD.PrintErr($"Błąd zapisu ustawień: {error}");
            }
        }

        /// <summary>
        /// Hermetyzacja: Reset do wartości domyślnych
        /// </summary>
        private void ResetToDefaults()
        {
            _masterVolume = 1.0f;
            _musicVolume = 0.8f;
            _sfxVolume = 0.9f;
            _fullscreen = false;
            _resolutionIndex = 1; // 1080p
            _difficultyIndex = 1; // Normal
        }

        #endregion

        #region UI Updates - Aktualizacja interfejsu
        
        /// <summary>
        /// Hermetyzacja: Aktualizacja UI na podstawie aktualnych ustawień
        /// </summary>
        private void UpdateUiFromSettings()
        {
            // Aktualizuj slidery audio
            _masterVolumeSlider.Value = _masterVolume;
            _musicVolumeSlider.Value = _musicVolume;
            _sfxVolumeSlider.Value = _sfxVolume;
            
            // Aktualizuj etykiety audio
            UpdateVolumeLabel(_masterVolumeLabel, "Główna głośność", _masterVolume);
            UpdateVolumeLabel(_musicVolumeLabel, "Muzyka", _musicVolume);
            UpdateVolumeLabel(_sfxVolumeLabel, "Efekty dźwiękowe", _sfxVolume);
            
            // Aktualizuj opcje graficzne
            _resolutionOption.Selected = _resolutionIndex;
            _fullscreenCheckbox.ButtonPressed = _fullscreen;
            
            // Aktualizuj opcje rozgrywki
            _difficultyOption.Selected = _difficultyIndex;
        }

        /// <summary>
        /// Helper: Aktualizacja etykiety głośności z procentem
        /// </summary>
        private void UpdateVolumeLabel(Label label, string name, float volume)
        {
            if (label != null)
            {
                int percentage = Mathf.RoundToInt(volume * 100);
                label.Text = $"{name}: {percentage}%";
            }
        }

        #endregion

        #region Helpers and Utilities
        
        /// <summary>
        /// Helper: Konwersja wartości liniowej na decybele
        /// Godot AudioServer używa decybeli, ale użytkownik preferuje procenty
        /// </summary>
        private float LinearToDb(float linear)
        {
            if (linear <= 0.0f)
                return -80.0f; // Praktycznie wyciszenie
            
            return 20.0f * Mathf.Log(linear) / Mathf.Log(10.0f);
        }
        
        #endregion

        #region Input Handling
        
        /// <summary>
        /// Obsługa klawiatury — szybki powrót przez Escape
        /// </summary>
        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel"))
            {
                OnBackButtonPressed();
            }
        }

        #endregion

        #region Cleanup
        
        public override void _ExitTree()
        {
            // Automatyczny zapis przy wyjściu
            SaveSettings();
            base._ExitTree();
        }

        #endregion
    }
}