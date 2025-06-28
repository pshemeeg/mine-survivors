using Godot;
using MineSurvivors.scripts.interfaces;

namespace MineSurvivors.scripts.weapons
{
    /// <summary>
    /// Abstrakcyjna klasa bazowa dla wszystkich broni.
    /// Demonstracja dziedziczenia i polimorfizmu w systemie broni.
    /// 
    /// Zasady OOP:
    /// - Dziedziczenie: Bazowa funkcjonalność dla wszystkich broni
    /// - Polimorfizm: Abstrakcyjna metoda PerformAttack() dla różnych typów broni
    /// - Hermetyzacja: Prywatne pola z kontrolowanym dostępem
    /// - Interfejs: Implementuje IAttack dla spójności z innymi systemami
    /// </summary>
    public abstract partial class Weapon : Node2D, IAttack
    {
        #region Exported Properties - Konfiguracja w edytorze

        [ExportGroup("Weapon Stats")]
        [Export] public float Damage { get; protected set; } = 10f;
        [Export] public float Cooldown { get; protected set; } = 1.0f;
        [Export] public float Range { get; protected set; } = 500f;

        #endregion

        #region Private State - Hermetyzacja

        // Stan broni - ukryty przed światem zewnętrznym
        private float _cooldownTimer = 0f;
        private bool _canAttack = true;

        // Publiczny dostęp tylko do odczytu
        public bool CanAttack => _canAttack && _cooldownTimer <= 0f;

        #endregion

        #region Initialization

        public override void _Ready()
        {
            // Setup wspólny dla wszystkich broni
            AddToGroup("weapons");
            GD.Print($"Weapon {GetType().Name} ready. Damage: {Damage}, Cooldown: {Cooldown}");
        }

        public override void _Process(double delta)
        {
            // Aktualizuj cooldown
            if (_cooldownTimer > 0)
            {
                _cooldownTimer -= (float)delta;

                // Reset flagi gdy cooldown się skończy
                if (_cooldownTimer <= 0)
                {
                    _canAttack = true;
                    OnCooldownFinished();
                }
            }
        }

        #endregion

        #region IAttack Implementation

        /// <summary>
        /// Publiczny interfejs do wykonania ataku.
        /// Sprawdza cooldown i deleguje do konkretnej implementacji.
        /// </summary>
        public float PerformAttack(IDamageable target)
        {
            if (!CanAttack) return 0f;

            // Wykonaj atak specyficzny dla typu broni
            bool attackSuccessful = ExecuteAttack();

            if (attackSuccessful)
            {
                // Uruchom cooldown
                StartCooldown();
                return Damage;
            }

            return 0f;
        }

        #endregion

        #region Abstract Methods - Polimorfizm

        /// <summary>
        /// Abstrakcyjna metoda - każda broń implementuje swój sposób ataku.
        /// To jest serce polimorfizmu w systemie broni.
        /// </summary>
        /// <returns>True jeśli atak się udał, false w przeciwnym razie</returns>
        protected abstract bool ExecuteAttack();

        /// <summary>
        /// Abstrakcyjna metoda - każda broń może mieć różną logikę update'u.
        /// Np. łuk obracanie w kierunku myszy, miecz tracking najbliższego wroga.
        /// </summary>
        /// <param name="delta">Delta time</param>
        protected abstract void UpdateWeapon(double delta);

        #endregion

        #region Protected Helpers - Wspólne dla klas pochodnych

        /// <summary>
        /// Hermetyzacja: Uruchomienie cooldown'u - dostępne dla klas pochodnych
        /// </summary>
        protected void StartCooldown()
        {
            _cooldownTimer = Cooldown;
            _canAttack = false;

            // Opcjonalnie: trigger visual feedback
            OnCooldownStarted();
        }

        /// <summary>
        /// Virtual method - klasy pochodne mogą override dla custom effects
        /// </summary>
        protected virtual void OnCooldownStarted()
        {
            // Base implementation - można rozszerzyć w klasach pochodnych
        }

        /// <summary>
        /// Virtual method wywoływany gdy cooldown się kończy
        /// </summary>
        protected virtual void OnCooldownFinished()
        {
            // Base implementation - można rozszerzyć w klasach pochodnych
            GD.Print($"{GetType().Name} ready to attack again!");
        }

        #endregion
    }
}