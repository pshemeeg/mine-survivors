using Godot;
using MineSurvivors.scripts.interfaces;

namespace MineSurvivors.scripts.weapons
{
    /// <summary>
    /// Strzała - pocisk wystrzeliwany przez łuk.
    /// Demonstracja kompozycji - Bow "używa" Arrow.
    /// 
    /// Zasady OOP:
    /// - Enkapsulacja: Strzała zarządza swoim ruchem i kolizjami
    /// - Hermetyzacja: Logika pocisku jest ukryta w tej klasie
    /// - Kompozycja: Jest używana przez Bow, ale jest niezależnym obiektem
    /// </summary>
    public partial class Arrow : Area2D
    {
        #region Private State - Hermetyzacja

        private float _speed = 1000f;
        private float _damage = 10f;
        private float _travelledDistance = 0f;
        private float _maxRange = 1200f;

        #endregion

        #region Initialization

        public override void _Ready()
        {
            // Połącz sygnał kolizji
            BodyEntered += OnBodyEntered;

            // Dodaj do grupy dla łatwego zarządzania
            AddToGroup("projectiles");

            GD.Print($"Arrow initialized. Speed: {_speed}, Damage: {_damage}");
        }

        /// <summary>
        /// Publiczny interfejs do inicjalizacji strzały przez Bow.
        /// Enkapsulacja: Bow może skonfigurować strzałę bez znania jej wewnętrznej implementacji.
        /// </summary>
        public void Initialize(float speed, float damage)
        {
            _speed = speed;
            _damage = damage;
        }

        #endregion

        #region Movement - Hermetyzacja ruchu

        public override void _PhysicsProcess(double delta)
        {
            MoveArrow(delta);
            CheckRange();
        }

        /// <summary>
        /// Hermetyzacja: Ruch strzały w kierunku jej rotacji
        /// </summary>
        private void MoveArrow(double delta)
        {
            var movement = Vector2.Right.Rotated(Rotation) * _speed * (float)delta;
            Position += movement;
            _travelledDistance += movement.Length();
        }

        /// <summary>
        /// Hermetyzacja: Sprawdzenie czy strzała przekroczyła zasięg
        /// </summary>
        private void CheckRange()
        {
            if (_travelledDistance > _maxRange)
            {
                GD.Print("Arrow reached max range, destroying");
                QueueFree();
            }
        }

        #endregion

        #region Collision Handling

        /// <summary>
        /// Obsługa kolizji - zastosowanie obrażeń jeśli to możliwe
        /// </summary>
        private void OnBodyEntered(Node2D body)
        {
            GD.Print($"Arrow hit: {body.Name}");

            // Sprawdź czy obiekt może otrzymać obrażenia
            if (body is IDamageable damageable)
            {
                damageable.TakeDamage(_damage);
                GD.Print($"Arrow dealt {_damage} damage to {body.Name}");
            }

            // Usuń strzałę po trafieniu
            QueueFree();
        }

        #endregion

        #region Cleanup

        public override void _ExitTree()
        {
            GD.Print("Arrow destroyed");
            base._ExitTree();
        }

        #endregion
    }
}