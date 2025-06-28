using Godot;
using MineSurvivors.scripts.interfaces;
using MineSurvivors.scripts.player;

namespace MineSurvivors.scripts.weapons
{
    /// <summary>
    /// Łuk - konkretna implementacja broni dystansowej.
    /// Demonstracja dziedziczenia i kompozycji (używa Arrow jako pocisku).
    /// 
    /// Zasady OOP:
    /// - Dziedziczenie: Dziedziczy funkcjonalność z Weapon
    /// - Kompozycja: Używa Arrow jako osobny obiekt
    /// - Hermetyzacja: Logika łuku jest ukryta w tej klasie
    /// - Polimorfizm: Implementuje ExecuteAttack() na swój sposób
    /// </summary>
    public partial class Bow : Weapon
    {
        #region Bow-specific Properties

        [ExportGroup("Bow Settings")]
        [Export] private float _arrowSpeed = 1000f;
        [Export] private PackedScene _arrowScene;

        #endregion

        #region Private Components - Hermetyzacja

        private Marker2D _shootingPoint;
        private Player _player; // Reference do gracza dla positioning

        #endregion

        #region Initialization

        public override void _Ready()
        {
            base._Ready(); // Wywołaj base initialization

            // Znajdź komponenty
            _shootingPoint = GetNode<Marker2D>("ShootingPoint");
            _player = GetTree().GetFirstNodeInGroup("player") as Player;

            // Load arrow scene jeśli nie ustawiony w edytorze
            if (_arrowScene == null)
            {
                _arrowScene = GD.Load<PackedScene>("res://scenes/weapons/Arrow.tscn");
            }

            // Walidacja
            if (_shootingPoint == null)
                GD.PrintErr("BŁĄD: Bow nie znalazł ShootingPoint!");
            if (_arrowScene == null)
                GD.PrintErr("BŁĄD: Bow nie ma Arrow scene!");

            GD.Print("Bow ready to shoot!");
        }

        public override void _Process(double delta)
        {
            base._Process(delta); // Cooldown update
            UpdateWeapon(delta);   // Bow-specific update
        }

        #endregion

        #region Weapon Implementation - Polimorfizm

        /// <summary>
        /// Implementacja abstrakcyjnej metody - jak łuk atakuje.
        /// Tworzy strzałę i wystrzeliwuje ją w kierunku rotacji łuku.
        /// </summary>
        protected override bool ExecuteAttack()
        {
            if (_arrowScene == null || _shootingPoint == null)
                return false;

            // Stwórz nową strzałę - kompozycja w działaniu
            var arrow = _arrowScene.Instantiate<Arrow>();

            // Ustaw pozycję i rotację strzały
            arrow.GlobalTransform = _shootingPoint.GlobalTransform;

            // Ustaw prędkość strzały
            arrow.Initialize(_arrowSpeed, Damage);

            // Dodaj do sceny (parent node)
            GetTree().CurrentScene.AddChild(arrow);

            GD.Print($"Bow shot arrow! Direction: {Rotation}");
            return true;
        }

        /// <summary>
        /// Update specyficzny dla łuku - obracanie w kierunku kursora myszy.
        /// </summary>
        protected override void UpdateWeapon(double delta)
        {
            // Obróć łuk w kierunku kursora myszy
            RotateTowardsMouse();

            // Follow gracza jeśli istnieje
            FollowPlayer();
        }

        #endregion

        #region Input Handling

        public override void _Input(InputEvent @event)
        {
            // Strzał na input - używamy tylko "attack" żeby uniknąć problemów z brakiem Input Map
            if (@event.IsActionPressed("attack"))
            {
                TryShoot();
            }
        }

        /// <summary>
        /// Hermetyzacja: Próba oddania strzału
        /// </summary>
        private void TryShoot()
        {
            if (CanAttack)
            {
                // Użyj polimorficznej metody attack
                PerformAttack(null); // Łuk nie potrzebuje konkretnego target
            }
            else
            {
                GD.Print("Bow on cooldown!");
            }
        }

        #endregion

        #region Private Helpers - Hermetyzacja logiki

        /// <summary>
        /// Hermetyzacja: Obracanie łuku w kierunku kursora myszy
        /// </summary>
        private void RotateTowardsMouse()
        {
            var mousePosition = GetGlobalMousePosition();
            LookAt(mousePosition);
        }

        /// <summary>
        /// Hermetyzacja: Łuk podąża za graczem
        /// </summary>
        private void FollowPlayer()
        {
            if (_player != null && IsInstanceValid(_player))
            {
                GlobalPosition = _player.GlobalPosition;
            }
        }

        #endregion
    }
}