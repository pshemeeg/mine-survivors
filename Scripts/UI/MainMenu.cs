using Godot;

namespace MineSurvivors.scripts.ui
{
    /// <summary>
    /// Klasa głównego menu gry — demonstracja hermetyzacji i odpowiedzialności.
    /// 
    /// Zasady OOP w działaniu:
    /// - Hermetyzacja: Wszystkie elementy UI są prywatne, dostęp tylko przez publiczne metody
    /// - Enkapsulacja: Logika menu jest oddzielona od logiki gry
    /// - Separacja odpowiedzialności: Menu zajmuje się tylko interfejsem, nie logiką gry
    /// 
    /// </summary>
    public partial class MainMenu : Control
    {
        #region Private UI Components - Hermetyzacja
        
        // Wszystkie komponenty UI są prywatne — hermetyzacja w działaniu
        private Button _startButton;
        private Button _optionsButton;
        private Button _highScoresButton;
        private Button _quitButton;
        private Label _titleLabel;

        // Ścieżki do scen — łatwo modyfikowalne, ale ukryte przed zewnętrzem
        private const string GameScenePath = "res://Scenes/Game/GameScene.tscn";
        private const string OptionsScenePath = "res://scenes/UI/OptionsMenu.tscn";
        private const string HighScoresScenePath = "res://Scenes/UI/HighScoresMenu.tscn";
        
        #endregion

        #region Initialization - Configuracja menu
        
        public override void _Ready()
        {
            GD.Print("Inicjalizacja głównego menu...");
            
            // Znajdź wszystkie komponenty UI
            FindUiComponents();
            
            // Skonfiguruj przyciski
            SetupButtons();
            
            // Ustaw focus na pierwszym przycisku dla łatwiejszej nawigacji
            _startButton?.GrabFocus();
            
            GD.Print("Główne menu gotowe!");
        }

        /// <summary>
        /// Hermetyzacja: Prywatna metoda do znajdowania komponentów UI
        /// Całą logikę znajdowania elementów ukrywamy w tej metodzie
        /// </summary>
        private void FindUiComponents()
        {
            // Użyj Godot's node paths — bezpieczne nawet jak struktura się zmieni
            _titleLabel = GetNode<Label>("VBoxContainer/TitleLabel");
            _startButton = GetNode<Button>("VBoxContainer/ButtonContainer/StartButton");
            _optionsButton = GetNode<Button>("VBoxContainer/ButtonContainer/OptionsButton");
            _highScoresButton = GetNode<Button>("VBoxContainer/ButtonContainer/HighScoresButton");
            _quitButton = GetNode<Button>("VBoxContainer/ButtonContainer/QuitButton");
            
            
            // Sprawdź, czy znaleziono kluczowe komponenty
            ValidateUiComponents();
        }

        /// <summary>
        /// Hermetyzacja: Walidacja czy wszystkie potrzebne komponenty zostały znalezione
        /// </summary>
        private void ValidateUiComponents()
        {
            if (_startButton == null) GD.PrintErr("BŁĄD: Nie znaleziono StartButton!");
            if (_optionsButton == null) GD.PrintErr("BŁĄD: Nie znaleziono OptionsButton!");
            if (_highScoresButton == null) GD.PrintErr("BŁĄD: Nie znaleziono HighScoresButton!");
            if (_quitButton == null) GD.PrintErr("BŁĄD: Nie znaleziono QuitButton!");
            if (_titleLabel == null) GD.PrintErr("BŁĄD: Nie znaleziono TitleLabel!");
            
        }

        /// <summary>
        /// Hermetyzacja: Konfiguracja przycisków w jednym miejscu
        /// Separacja odpowiedzialności — setup oddzielony od logiki
        /// </summary>
        private void SetupButtons()
        {
            // Podłącz sygnały do metod obsługi
            if (_startButton != null)
            {
                _startButton.Pressed += OnStartButtonPressed;
            }
            
            if (_optionsButton != null)
            {
                _optionsButton.Pressed += OnOptionsButtonPressed;
            }
            
            if (_highScoresButton != null)
            {
                _highScoresButton.Pressed += OnHighScoresButtonPressed;
            }
            
            if (_quitButton != null)
            {
                _quitButton.Pressed += OnQuitButtonPressed;
            }
        }

        #endregion

        #region Button Event Handlers - Publiczne API menu
        
        /// <summary>
        /// Publiczny interfejs: Obsługa rozpoczęcia gry
        /// Enkapsulacja: Menu wie, jak przejść do gry, ale nie zna szczegółów rozgrywki
        /// </summary>
        private void OnStartButtonPressed()
        {
            GD.Print("Rozpoczynanie nowej gry...");
            
            // Transition do sceny gry
            TransitionToScene(GameScenePath);
        }
        
        /// <summary>
        /// Publiczny interfejs: Obsługa wyników
        /// Enkapsulacja: Menu wie, jak przejść do wyników, ale nie zna szczegółów ich implementacji
        /// </summary>
        private void OnHighScoresButtonPressed()
        {
            GD.Print("Otwieranie najlepszych wyników...");
            
            // Transition do sceny wyników
            TransitionToScene(HighScoresScenePath);
        }

        /// <summary>
        /// Publiczny interfejs: Obsługa opcji
        /// W przyszłości można dodać ekran ustawień
        /// </summary>
        private void OnOptionsButtonPressed()
        {
            GD.Print("Otwieranie opcji...");
            
            // Transition do opcji:
            TransitionToScene(OptionsScenePath);
        }

        /// <summary>
        /// Publiczny interfejs: Obsługa wyjścia z gry
        /// Enkapsulacja: Menu wie, jak zamknąć grę, ale nie zna szczegółów systemu
        /// </summary>
        private void OnQuitButtonPressed()
        {
            GD.Print("Zamykanie gry...");
            
            // Graceful shutdown-daj czas na odtworzenie dźwięku
            GetTree().CreateTimer(0.1).Timeout += () => GetTree().Quit();
        }

        #endregion

        #region Scene Management - Hermetyzacja przejść między scenami
        
        /// <summary>
        /// Hermetyzacja: Bezpieczne przejście do innej sceny
        /// Enkapsulacja: Menu wie, jak zmieniać sceny, ale nie zna ich implementacji
        /// </summary>
        private void TransitionToScene(string scenePath)
        {
            // Sprawdź, czy scena istnieje
            if (!ResourceLoader.Exists(scenePath))
            {
                GD.PrintErr($"BŁĄD: Nie znaleziono sceny: {scenePath}");
                return;
            }
            
            // Wczytaj i zmień scenę
            var packedScene = GD.Load<PackedScene>(scenePath);
            if (packedScene != null)
            {
                GetTree().ChangeSceneToPacked(packedScene);
            }
            else
            {
                GD.PrintErr($"BŁĄD: Nie udało się wczytać sceny: {scenePath}");
            }
        }

        #endregion

        #region Input Handling - Obsługa klawiatury
        
        /// <summary>
        /// Obsługa input'u z klawiatury-accessibility i wygoda
        /// KISS: Proste mapowanie klawiszy na akcje przycisków
        /// </summary>
        public override void _Input(InputEvent @event)
        {
            // Obsługa Escape — szybkie wyjście
            if (@event.IsActionPressed("ui_cancel") || @event.IsActionPressed("quit"))
            {
                OnQuitButtonPressed();
            }
            
            // Enter/Space — rozpocznij grę
            if (@event.IsActionPressed("ui_accept") || @event.IsActionPressed("start_game"))
            {
                OnStartButtonPressed();
            }
        }

        #endregion

        #region Cleanup - Zarządzanie zasobami
        
        /// <summary>
        /// Czyszczenie zasobów — dobra praktyka w Godot
        /// Hermetyzacja: Menu samo zarządza swoimi zasobami
        /// </summary>
        public override void _ExitTree()
        {
            GD.Print("Zamykanie głównego menu...");
            
            base._ExitTree();
        }

        #endregion
    }
}