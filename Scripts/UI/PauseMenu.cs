using Godot;

namespace MineSurvivors.scripts.ui
{
    /// <summary>
    /// Menu pauzy - demonstracja hermetyzacji UI i zarządzania stanem gry.
    /// 
    /// Zasady OOP:
    /// - Hermetyzacja: Wszystkie elementy UI są prywatne
    /// - Enkapsulacja: Logika pauzy jest oddzielona od logiki gry
    /// - Separacja odpowiedzialności: Menu zajmuje się tylko UI, nie logiką gry
    /// - Kompozycja: Używa GameManager do zarządzania stanem
    /// </summary>
    public partial class PauseMenu : Control
    {
        #region Private UI Components - Hermetyzacja
        
        private Label _titleLabel;
        private Button _resumeButton;
        private Button _optionsButton;
        private Button _mainMenuButton;
        private Button _quitButton;
        
        // Audio feedback
        private AudioStreamPlayer _buttonSound;
        
        // Ścieżki scen - hermetyzacja konfiguracji
        private const string MainMenuPath = "res://scenes/UI/MainMenu.tscn";
        private const string OptionsPath = "res://scenes/UI/OptionsMenu.tscn";
        
        // Stan pauzy - kontrolowany dostęp
        private bool _isPaused = false;
        
        #endregion

        #region Initialization
        
        public override void _Ready()
        {
            GD.Print("Inicjalizacja menu pauzy...");
            
            // Znajdź komponenty UI
            FindUiComponents();
            
            // Skonfiguruj przyciski
            SetupButtons();
            
            // Ukryj menu na start
            Hide();
            SetPaused(false);
            
            GD.Print("Menu pauzy gotowe!");
        }

        /// <summary>
        /// Hermetyzacja: Znajdowanie komponentów UI
        /// </summary>
        private void FindUiComponents()
        {
            var container = GetNode("PanelContainer/VBoxContainer");
            
            _titleLabel = container.GetNode<Label>("TitleLabel");
            _resumeButton = container.GetNode<Button>("ButtonContainer/ResumeButton");
            _optionsButton = container.GetNode<Button>("ButtonContainer/OptionsButton");
            _mainMenuButton = container.GetNode<Button>("ButtonContainer/MainMenuButton");
            _quitButton = container.GetNode<Button>("ButtonContainer/QuitButton");
            
            // Walidacja komponentów
            ValidateComponents();
        }

        private void ValidateComponents()
        {
            if (_resumeButton == null) GD.PrintErr("BŁĄD: Brak ResumeButton!");
            if (_mainMenuButton == null) GD.PrintErr("BŁĄD: Brak MainMenuButton!");
            if (_quitButton == null) GD.PrintErr("BŁĄD: Brak QuitButton!");
        }

        /// <summary>
        /// Hermetyzacja: Konfiguracja przycisków
        /// </summary>
        private void SetupButtons()
        {
            if (_resumeButton != null)
                _resumeButton.Pressed += OnResumePressed;
                
            if (_optionsButton != null)
                _optionsButton.Pressed += OnOptionsPressed;
                
            if (_mainMenuButton != null)
                _mainMenuButton.Pressed += OnMainMenuPressed;
                
            if (_quitButton != null)
                _quitButton.Pressed += OnQuitPressed;
        }

        #endregion

        #region Public API - Kontrolowany dostęp do funkcji pauzy
        
        /// <summary>
        /// Publiczny interfejs: Przełącz pauzę
        /// Enkapsulacja: Zewnętrzne systemy nie muszą znać szczegółów implementacji
        /// </summary>
        public void TogglePause()
        {
            if (_isPaused)
                Resume();
            else
                Pause();
        }

        /// <summary>
        /// Publiczny interfejs: Zapauzuj grę
        /// </summary>
        public void Pause()
        {
            SetPaused(true);
            Show();
            _resumeButton?.GrabFocus();
            GD.Print("Gra zapauzowana");
        }

        /// <summary>
        /// Publiczny interfejs: Wznów grę
        /// </summary>
        public void Resume()
        {
            SetPaused(false);
            Hide();
            GD.Print("Gra wznowiona");
        }

        /// <summary>
        /// Getter dla stanu pauzy - kontrolowany dostęp do prywatnej zmiennej
        /// </summary>
        public bool IsPaused => _isPaused;

        #endregion

        #region Private Methods - Hermetyzacja logiki
        
        /// <summary>
        /// Hermetyzacja: Centralne zarządzanie stanem pauzy
        /// </summary>
        private void SetPaused(bool paused)
        {
            _isPaused = paused;
            
            // Zatrzymaj/wznów cały tree (except UI)
            GetTree().Paused = paused;
            
            // Ustaw process mode żeby UI działało podczas pauzy
            ProcessMode = paused ? ProcessModeEnum.WhenPaused : ProcessModeEnum.Pausable;
        }

        #endregion

        #region Event Handlers - Obsługa przycisków
        
        /// <summary>
        /// Wznów grę
        /// </summary>
        private void OnResumePressed()
        {
            Resume();
        }

        /// <summary>
        /// Otwórz opcje - zachowaj stan pauzy
        /// </summary>
        private void OnOptionsPressed()
        {
            GD.Print("Otwieranie opcji z menu pauzy...");
            
            // TODO: Otwórz options overlay lub przejdź do opcji
            // Zachowaj stan pauzy
            GD.Print("Opcje - do implementacji");
        }

        /// <summary>
        /// Powrót do głównego menu
        /// </summary>
        private void OnMainMenuPressed()
        {
            GD.Print("Powrót do głównego menu...");
            
            // Wznów grę przed zmianą sceny
            SetPaused(false);
            
            // Przejdź do głównego menu
            GetTree().ChangeSceneToFile(MainMenuPath);
        }

        /// <summary>
        /// Zamknij grę
        /// </summary>
        private void OnQuitPressed()
        {
            GD.Print("Zamykanie gry z menu pauzy...");
            
            // Wznów grę przed zamknięciem
            SetPaused(false);
            
            // Zamknij grę
            GetTree().Quit();
        }

        #endregion

        #region Input Handling
        
        /// <summary>
        /// Obsługa klawiatury - ESC toggles pause
        /// </summary>
        public override void _Input(InputEvent @event)
        {
            // Tylko jeśli menu jest aktywne
            if (!Visible) return;
            
            if (@event.IsActionPressed("ui_cancel") || @event.IsActionPressed("pause"))
            {
                Resume();
            }
        }

        #endregion
    }
}