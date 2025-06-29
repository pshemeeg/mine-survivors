using Godot;
using MineSurvivors.scripts.managers;
using System.Collections.Generic;

namespace MineSurvivors.scripts.ui
{
    /// <summary>
    /// LevelUpMenu - proste menu wyboru ulepszeń po awansie.
    /// KISS: 3 przyciski, gracz klika jeden, koniec.
    /// 
    /// Zasady OOP:
    /// - Hermetyzacja: Wszystkie elementy UI są prywatne
    /// - Enkapsulacja: Logika menu oddzielona od logiki ulepszeń
    /// - Separacja odpowiedzialności: Tylko prezentacja opcji, nie logika gry
    /// - Kompozycja: Używa UpgradeOption jako dane do wyświetlenia
    /// </summary>
    public partial class LevelUpMenu : Control
    {
        #region Signals - Komunikacja z LevelUpManager
        
        [Signal]
        public delegate void UpgradeSelectedEventHandler(int optionIndex);
        
        #endregion

        #region Private UI Components - Hermetyzacja
        
        private Label _titleLabel;
        private Button _option1Button;
        private Button _option2Button;
        private Button _option3Button;
        
        // Przechowywanie aktualnych opcji
        private List<LevelUpManager.UpgradeOption> _currentOptions = new();
        
        #endregion

        #region Initialization
        
        public override void _Ready()
        {
            GD.Print("Inicjalizacja LevelUpMenu...");
            
            // Znajdź komponenty UI
            FindUiComponents();
            
            // Skonfiguruj przyciski
            SetupButtons();
            
            // Ukryj menu na start
            Hide();
            
            // Dodaj do grupy dla łatwego znajdowania
            AddToGroup("levelup_menu");
            
            // Ustaw process mode żeby działało podczas pauzy
            ProcessMode = ProcessModeEnum.WhenPaused;
            
            GD.Print("LevelUpMenu gotowe!");
        }

        /// <summary>
        /// Hermetyzacja: Znajdowanie komponentów UI
        /// </summary>
        private void FindUiComponents()
        {
            // Struktura: PanelContainer/VBoxContainer/...
            var container = GetNode("PanelContainer/VBoxContainer");
            
            _titleLabel = container.GetNode<Label>("TitleLabel");
            
            var buttonsContainer = container.GetNode("ButtonsContainer");
            _option1Button = buttonsContainer.GetNode<Button>("Option1Button");
            _option2Button = buttonsContainer.GetNode<Button>("Option2Button");
            _option3Button = buttonsContainer.GetNode<Button>("Option3Button");
            
            ValidateComponents();
        }

        private void ValidateComponents()
        {
            if (_option1Button == null) GD.PrintErr("BŁĄD: Brak Option1Button!");
            if (_option2Button == null) GD.PrintErr("BŁĄD: Brak Option2Button!");
            if (_option3Button == null) GD.PrintErr("BŁĄD: Brak Option3Button!");
        }

        /// <summary>
        /// Hermetyzacja: Konfiguracja przycisków
        /// </summary>
        private void SetupButtons()
        {
            if (_option1Button != null)
                _option1Button.Pressed += () => OnOptionSelected(0);
                
            if (_option2Button != null)
                _option2Button.Pressed += () => OnOptionSelected(1);
                
            if (_option3Button != null)
                _option3Button.Pressed += () => OnOptionSelected(2);
        }
        
        #endregion

        #region Public API - Interface dla LevelUpManager
        
        /// <summary>
        /// Publiczny interfejs: Pokaż menu z opcjami ulepszeń
        /// Enkapsulacja: LevelUpManager nie musi znać szczegółów UI
        /// </summary>
        public void ShowUpgradeOptions(List<LevelUpManager.UpgradeOption> options)
        {
            // Zapisz opcje
            _currentOptions = new List<LevelUpManager.UpgradeOption>(options);
            
            // Zaktualizuj tekst przycisków
            UpdateButtonTexts();
            
            // Pokaż menu
            Show();
            
            // Focus na pierwszym przycisku
            _option1Button?.GrabFocus();
            
            GD.Print($"LevelUpMenu pokazane z {options.Count} opcjami");
        }
        
        #endregion

        #region Private Methods - Hermetyzacja logiki UI
        
        /// <summary>
        /// Hermetyzacja: Aktualizacja tekstów przycisków na podstawie opcji
        /// </summary>
        private void UpdateButtonTexts()
        {
            // Ustaw tekst dla każdego przycisku
            UpdateButtonText(_option1Button, 0);
            UpdateButtonText(_option2Button, 1);
            UpdateButtonText(_option3Button, 2);
            
            // Ustaw tytuł
            if (_titleLabel != null)
            {
                _titleLabel.Text = "🎉 LEVEL UP! 🎉\nWybierz ulepszenie:";
            }
        }

        /// <summary>
        /// Helper: Aktualizacja tekstu pojedynczego przycisku
        /// </summary>
        private void UpdateButtonText(Button button, int optionIndex)
        {
            if (button == null || optionIndex >= _currentOptions.Count) 
            {
                if (button != null)
                {
                    button.Text = "Brak opcji";
                    button.Disabled = true;
                }
                return;
            }
            
            var option = _currentOptions[optionIndex];
            
            // Format: Nazwa + opis + wartość
            string buttonText = $"{option.Name}\n{option.Description}";
            
            // Dodaj emoji na podstawie typu
            string emoji = GetUpgradeEmoji(option.Type);
            buttonText = $"{emoji} {buttonText}";
            
            button.Text = buttonText;
            button.Disabled = false;
        }

        /// <summary>
        /// Helper: Emoji dla różnych typów ulepszeń
        /// </summary>
        private string GetUpgradeEmoji(LevelUpManager.UpgradeType type)
        {
            return type switch
            {
                LevelUpManager.UpgradeType.Speed => "⚡",
                LevelUpManager.UpgradeType.Damage => "⚔️",
                LevelUpManager.UpgradeType.Defense => "🛡️",
                LevelUpManager.UpgradeType.Health => "❤️",
                _ => "💫"
            };
        }
        
        #endregion

        #region Event Handlers - Obsługa wyboru gracza
        
        /// <summary>
        /// Event Handler: Gracz wybrał opcję
        /// </summary>
        private void OnOptionSelected(int optionIndex)
        {
            if (optionIndex < 0 || optionIndex >= _currentOptions.Count)
            {
                GD.PrintErr($"BŁĄD: Nieprawidłowy indeks opcji: {optionIndex}");
                return;
            }
            
            var selectedOption = _currentOptions[optionIndex];
            
            GD.Print($"Gracz wybrał opcję {optionIndex + 1}: {selectedOption.Name}");
            
            // Wyślij sygnał z indeksem do LevelUpManager
            EmitSignal(SignalName.UpgradeSelected, optionIndex);
            
            // Ukryj menu
            Hide();
        }
        
        #endregion

        #region Input Handling - Obsługa klawiatury
        
        /// <summary>
        /// Obsługa klawiatury - numery 1, 2, 3 dla szybkiego wyboru
        /// </summary>
        public override void _Input(InputEvent @event)
        {
            if (!Visible) return;
            
            // Sprawdź naciśnięcia klawiszy numerycznych
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                switch (keyEvent.Keycode)
                {
                    case Key.Key1:
                        OnOptionSelected(0);
                        break;
                        
                    case Key.Key2:
                        OnOptionSelected(1);
                        break;
                        
                    case Key.Key3:
                        OnOptionSelected(2);
                        break;
                }
            }
        }
        
        #endregion

        #region Visual Effects - Opcjonalne ulepszenia
        
        /// <summary>
        /// Opcjonalna animacja pojawiania się menu
        /// </summary>
        private void AnimateMenuAppearance()
        {
            // Rozpocznij ze scale 0
            Scale = Vector2.Zero;
            
            // Animuj do normalnego rozmiaru
            var tween = CreateTween();
            tween.TweenProperty(this, "scale", Vector2.One, 0.3f);
            tween.TweenCallback(Callable.From(() => _option1Button?.GrabFocus()));
        }

        /// <summary>
        /// Opcjonalna animacja hover na przyciskach
        /// </summary>
        private void SetupButtonHoverEffects()
        {
            SetupButtonHover(_option1Button);
            SetupButtonHover(_option2Button);
            SetupButtonHover(_option3Button);
        }

        private void SetupButtonHover(Button button)
        {
            if (button == null) return;
            
            button.MouseEntered += () => {
                var tween = CreateTween();
                tween.TweenProperty(button, "scale", Vector2.One * 1.05f, 0.1f);
            };
            
            button.MouseExited += () => {
                var tween = CreateTween();
                tween.TweenProperty(button, "scale", Vector2.One, 0.1f);
            };
        }
        
        #endregion

        #region Enhanced Display - Rozszerzone formatowanie
        
        /// <summary>
        /// Rozszerzona wersja aktualizacji przycisku z lepszym formatowaniem
        /// </summary>
        private void UpdateButtonTextEnhanced(Button button, int optionIndex)
        {
            if (button == null || optionIndex >= _currentOptions.Count) 
            {
                if (button != null)
                {
                    button.Text = "❌ Brak opcji";
                    button.Disabled = true;
                }
                return;
            }
            
            var option = _currentOptions[optionIndex];
            
            // Utwórz rich text jeśli przycisk to wspiera
            string emoji = GetUpgradeEmoji(option.Type);
            string typeColor = GetUpgradeColor(option.Type);
            
            // Prosty format dla zwykłego Button
            string buttonText = $"{emoji} {option.Name}\n";
            buttonText += $"{option.Description}\n";
            buttonText += $"[+{option.Value}]";
            
            button.Text = buttonText;
            button.Disabled = false;
        }

        /// <summary>
        /// Helper: Kolor dla różnych typów ulepszeń
        /// </summary>
        private string GetUpgradeColor(LevelUpManager.UpgradeType type)
        {
            return type switch
            {
                LevelUpManager.UpgradeType.Speed => "#FFD700", // Złoty
                LevelUpManager.UpgradeType.Damage => "#FF4444", // Czerwony  
                LevelUpManager.UpgradeType.Defense => "#4488FF", // Niebieski
                LevelUpManager.UpgradeType.Health => "#44FF44", // Zielony
                _ => "#FFFFFF"
            };
        }
        
        #endregion

        #region Cleanup
        
        public override void _ExitTree()
        {
            GD.Print("LevelUpMenu cleanup");
            base._ExitTree();
        }
        
        #endregion
    }
}