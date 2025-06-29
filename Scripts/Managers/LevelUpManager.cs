using Godot;
using MineSurvivors.scripts.player;
using MineSurvivors.scripts.ui;
using System.Collections.Generic;
using System.Linq;

namespace MineSurvivors.scripts.managers
{
    /// <summary>
    /// LevelUpManager - zarządza systemem awansów i ulepszeń gracza.
    /// Prostota KISS: 3 losowe opcje, gracz wybiera jedną.
    /// 
    /// Zasady OOP:
    /// - Hermetyzacja: Logika ulepszeń ukryta w tej klasie
    /// - Enkapsulacja: Zewnętrzne systemy nie znają szczegółów implementacji
    /// - Kompozycja: Używa Player i LevelUpMenu jako komponenty
    /// - Separacja odpowiedzialności: Tylko level-up logic, nic więcej
    /// </summary>
    public partial class LevelUpManager : Node
    {
        #region Upgrade Definitions - Proste struktury danych
        
        /// <summary>
        /// Struktura opisująca pojedyncze ulepszenie
        /// KISS: Wszystko co potrzebne w jednym miejscu
        /// </summary>
        public struct UpgradeOption
        {
            public string Name;
            public string Description;
            public UpgradeType Type;
            public float Value;
            public string IconPath;

            public UpgradeOption(string name, string description, UpgradeType type, float value, string iconPath = "")
            {
                Name = name;
                Description = description;
                Type = type;
                Value = value;
                IconPath = iconPath;
            }
        }

        /// <summary>
        /// Typy ulepszeń - mapują się na metody Player
        /// </summary>
        public enum UpgradeType
        {
            Speed,
            Damage, 
            Defense,
            Health
        }
        
        #endregion

        #region Available Upgrades - Konfiguracja wszystkich możliwych ulepszeń
        
        /// <summary>
        /// Wszystkie możliwe ulepszenia w grze
        /// KISS: Hardcoded ale łatwe do modyfikacji
        /// </summary>
        private readonly List<UpgradeOption> _allUpgrades = new()
        {
            // Speed Upgrades
            new("Szybkie Nogi", "Zwiększa prędkość ruchu o 20", UpgradeType.Speed, 20f),
            new("Buty Sprinta", "Znacznie zwiększa prędkość ruchu o 40", UpgradeType.Speed, 40f),
            new("Wiatr w Plecach", "Umiarkowanie zwiększa prędkość o 15", UpgradeType.Speed, 15f),
            new("Błyskawiczne Kroki", "Drastycznie zwiększa prędkość o 60", UpgradeType.Speed, 60f),
            
            // Damage Upgrades  
            new("Ostre Ostrze", "Zwiększa obrażenia o 5", UpgradeType.Damage, 5f),
            new("Moc Ataku", "Znacznie zwiększa obrażenia o 10", UpgradeType.Damage, 10f),
            new("Krytyczne Uderzenie", "Umiarkowanie zwiększa obrażenia o 7", UpgradeType.Damage, 7f),
            new("Niszczycielska Siła", "Drastycznie zwiększa obrażenia o 15", UpgradeType.Damage, 15f),
            
            // Defense Upgrades
            new("Twarda Skóra", "Zwiększa obronę o 2", UpgradeType.Defense, 2f),
            new("Stalowa Zbroja", "Znacznie zwiększa obronę o 4", UpgradeType.Defense, 4f),
            new("Kamienna Skóra", "Umiarkowanie zwiększa obronę o 3", UpgradeType.Defense, 3f),
            new("Niezniszczalność", "Drastycznie zwiększa obronę o 6", UpgradeType.Defense, 6f),
            
            // Health Upgrades
            new("Więcej Zdrowia", "Zwiększa maksymalne HP o 25", UpgradeType.Health, 25f),
            new("Regeneracja", "Znacznie zwiększa maksymalne HP o 50", UpgradeType.Health, 50f),
            new("Wytrzymałość", "Umiarkowanie zwiększa maksymalne HP o 35", UpgradeType.Health, 35f),
            new("Żelazne Zdrowie", "Drastycznie zwiększa maksymalne HP o 75", UpgradeType.Health, 75f),
        };
        
        #endregion

        #region Private State - Hermetyzacja
        
        private Player _player;
        private LevelUpMenu _levelUpMenu;
        private bool _isUpgradeMenuOpen = false;
        
        // Przechowywanie aktualnych opcji dla menu
        private List<UpgradeOption> _currentUpgradeOptions = new();
        
        #endregion

        #region Initialization
        
        public override void _Ready()
        {
            GD.Print("Inicjalizacja LevelUpManager...");
            
            // Znajdź gracza
            ConnectToPlayer();
            
            // Znajdź/stwórz menu level up
            SetupLevelUpMenu();
            
            GD.Print($"LevelUpManager gotowy! Dostępnych ulepszeń: {_allUpgrades.Count}");
        }

        /// <summary>
        /// Połącz się z graczem i słuchaj jego level up
        /// </summary>
        private void ConnectToPlayer()
        {
            _player = GetTree().GetFirstNodeInGroup("player") as Player;
            
            if (_player != null)
            {
                // Słuchaj sygnału level up
                _player.LevelUp += OnPlayerLevelUp;
                GD.Print("LevelUpManager połączony z graczem");
            }
            else
            {
                GD.PrintErr("BŁĄD: LevelUpManager nie znalazł gracza!");
            }
        }

        /// <summary>
        /// Znajdź lub stwórz LevelUpMenu
        /// </summary>
        private void SetupLevelUpMenu()
        {
            // Spróbuj znaleźć istniejące menu
            _levelUpMenu = GetTree().GetFirstNodeInGroup("levelup_menu") as LevelUpMenu;
            
            if (_levelUpMenu == null)
            {
                GD.Print("LevelUpMenu nie znalezione - będzie utworzone dynamicznie");
                // Menu zostanie utworzone przy pierwszym level up
            }
            else
            {
                // Podłącz sygnał wyboru
                _levelUpMenu.UpgradeSelected += OnUpgradeSelected;
                GD.Print("LevelUpMenu znalezione i podłączone");
            }
        }
        
        #endregion

        #region Level Up Logic - Serce systemu
        
        /// <summary>
        /// Event Handler: Gracz awansował na poziom
        /// </summary>
        private void OnPlayerLevelUp(int newLevel)
        {
            GD.Print($"Gracz awansował na poziom {newLevel}!");
            
            // Generuj 3 losowe opcje ulepszeń
            var upgradeOptions = GenerateUpgradeOptions();
            
            // Pokaż menu wyboru
            ShowLevelUpMenu(upgradeOptions);
        }

        /// <summary>
        /// Generuj 3 losowe, unikalne opcje ulepszeń
        /// KISS: Proste losowanie bez skomplikowanych algorytmów
        /// </summary>
        private List<UpgradeOption> GenerateUpgradeOptions()
        {
            var availableUpgrades = new List<UpgradeOption>(_allUpgrades);
            var selectedUpgrades = new List<UpgradeOption>();
            
            // Wylosuj 3 unikalne opcje
            for (int i = 0; i < 3 && availableUpgrades.Count > 0; i++)
            {
                int randomIndex = GD.RandRange(0, availableUpgrades.Count - 1);
                selectedUpgrades.Add(availableUpgrades[randomIndex]);
                availableUpgrades.RemoveAt(randomIndex);
            }
            
            // Jeśli mało opcji, dodaj duplikaty z większymi wartościami
            while (selectedUpgrades.Count < 3 && _allUpgrades.Count > 0)
            {
                var baseUpgrade = _allUpgrades[GD.RandRange(0, _allUpgrades.Count - 1)];
                var enhancedUpgrade = new UpgradeOption(
                    baseUpgrade.Name + " II",
                    baseUpgrade.Description + " (ulepszony)",
                    baseUpgrade.Type,
                    baseUpgrade.Value * 1.5f,
                    baseUpgrade.IconPath
                );
                selectedUpgrades.Add(enhancedUpgrade);
            }
            
            return selectedUpgrades;
        }

        /// <summary>
        /// Pokaż menu wyboru ulepszeń
        /// </summary>
        private void ShowLevelUpMenu(List<UpgradeOption> options)
        {
            // Zapisz opcje do użytku po wyborze gracza
            _currentUpgradeOptions = new List<UpgradeOption>(options);
            
            // Jeśli menu nie istnieje, stwórz je dynamicznie
            if (_levelUpMenu == null)
            {
                CreateLevelUpMenuDynamically();
            }
            
            if (_levelUpMenu != null)
            {
                _isUpgradeMenuOpen = true;
                
                // Zapauzuj grę
                GetTree().Paused = true;
                
                // Pokaż menu z opcjami
                _levelUpMenu.ShowUpgradeOptions(options);
                
                GD.Print("Menu level up wyświetlone");
            }
            else
            {
                GD.PrintErr("BŁĄD: Nie udało się stworzyć LevelUpMenu!");
                // Fallback: zastosuj losowe ulepszenie
                ApplyRandomUpgrade(options);
            }
        }

        /// <summary>
        /// Event Handler: Gracz wybrał ulepszenie (przez indeks)
        /// </summary>
        private void OnUpgradeSelected(int selectedIndex)
        {
            if (selectedIndex < 0 || selectedIndex >= _currentUpgradeOptions.Count)
            {
                GD.PrintErr($"BŁĄD: Nieprawidłowy indeks ulepszenia: {selectedIndex}");
                return;
            }
            
            var selectedUpgrade = _currentUpgradeOptions[selectedIndex];
            
            GD.Print($"Gracz wybrał: {selectedUpgrade.Name}");
            
            // Zastosuj ulepszenie do gracza
            ApplyUpgradeToPlayer(selectedUpgrade);
            
            // Ukryj menu i wznów grę
            HideLevelUpMenu();
        }

        /// <summary>
        /// Zastosuj wybrane ulepszenie do gracza
        /// Polimorfizm: Jedna metoda obsługuje wszystkie typy ulepszeń
        /// </summary>
        private void ApplyUpgradeToPlayer(UpgradeOption upgrade)
        {
            if (_player == null) return;
            
            switch (upgrade.Type)
            {
                case UpgradeType.Speed:
                    _player.AddSpeedBonus(upgrade.Value);
                    break;
                    
                case UpgradeType.Damage:
                    _player.AddDamageBonus(upgrade.Value);
                    break;
                    
                case UpgradeType.Defense:
                    _player.AddDefenseBonus(upgrade.Value);
                    break;
                    
                case UpgradeType.Health:
                    _player.AddHealthBonus(upgrade.Value);
                    break;
                    
                default:
                    GD.PrintErr($"BŁĄD: Nieznany typ ulepszenia: {upgrade.Type}");
                    break;
            }
            
            GD.Print($"Ulepszenie {upgrade.Name} zastosowane! Wartość: +{upgrade.Value}");
        }

        /// <summary>
        /// Ukryj menu level up i wznów grę
        /// </summary>
        private void HideLevelUpMenu()
        {
            _isUpgradeMenuOpen = false;
            
            // Wznów grę
            GetTree().Paused = false;
            
            // Ukryj menu
            if (_levelUpMenu != null)
            {
                _levelUpMenu.Hide();
            }
            
            GD.Print("Menu level up ukryte, gra wznowiona");
        }
        
        #endregion

        #region Dynamic Menu Creation - Fallback
        
        /// <summary>
        /// Stwórz LevelUpMenu dynamicznie jeśli nie ma go w scenie
        /// </summary>
        private void CreateLevelUpMenuDynamically()
        {
            // Spróbuj załadować scenę menu
            var menuScene = GD.Load<PackedScene>("res://scenes/UI/LevelUpMenu.tscn");
            
            if (menuScene != null)
            {
                _levelUpMenu = menuScene.Instantiate<LevelUpMenu>();
                GetTree().CurrentScene.AddChild(_levelUpMenu);
                _levelUpMenu.UpgradeSelected += OnUpgradeSelected;
                
                GD.Print("LevelUpMenu utworzone dynamicznie");
            }
            else
            {
                GD.PrintErr("BŁĄD: Nie znaleziono sceny LevelUpMenu.tscn");
            }
        }

        /// <summary>
        /// Fallback: Zastosuj losowe ulepszenie jeśli menu nie działa
        /// </summary>
        private void ApplyRandomUpgrade(List<UpgradeOption> options)
        {
            if (options.Count > 0)
            {
                var randomUpgrade = options[GD.RandRange(0, options.Count - 1)];
                ApplyUpgradeToPlayer(randomUpgrade);
                GD.Print($"Zastosowano losowe ulepszenie (fallback): {randomUpgrade.Name}");
            }
        }
        
        #endregion

        #region Public API - Interface dla innych systemów
        
        /// <summary>
        /// Wymuszenie level up (debug/testing)
        /// </summary>
        public void TriggerLevelUp()
        {
            if (_player != null)
            {
                int currentLevel = _player.Level;
                OnPlayerLevelUp(currentLevel + 1);
            }
        }

        /// <summary>
        /// Sprawdź czy menu level up jest otwarte
        /// </summary>
        public bool IsUpgradeMenuOpen => _isUpgradeMenuOpen;
        
        #endregion

        /// <summary>
        /// Rozłącz sygnały
        /// </summary>
        public override void _ExitTree()
        {
            // Rozłącz sygnały
            if (_player != null)
            {
                _player.LevelUp -= OnPlayerLevelUp;
            }
            
            if (_levelUpMenu != null)
            {
                _levelUpMenu.UpgradeSelected -= OnUpgradeSelected;
            }
            
            GD.Print("LevelUpManager cleanup completed");
            base._ExitTree();
        }
    }
}