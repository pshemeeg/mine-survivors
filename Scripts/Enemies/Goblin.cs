using Godot;

namespace MineSurvivors.scripts.enemies;

/// <summary>
/// Goblin - pierwszy konkretny typ przeciwnika w naszej grze.
/// Demonstruje jak prosta może być implementacja konkretnego przeciwnika
/// gdy mamy dobrze zaprojektowaną klasę bazową.
/// 
/// Charakterystyka Goblina:
/// - Agresywny, podstawowy przeciwnik
/// - Porusza się bezpośrednio w kierunku gracza
/// - Nieco szybszy niż standardowy Enemy (pokazuje customization)
/// - Idealne wprowadzenie do systemu przeciwników
/// 
/// Zasady OOP w działaniu:
/// - Dziedziczenie: dziedziczy wszystko z Enemy
/// - Polimorfizm: nadpisuje tylko metodę CalculateMovement
/// - Hermetyzacja: wykorzystuje protected/public interface klasy bazowej
/// - Kompozycja: używa tej samej struktury komponentów co Enemy
/// </summary>
public partial class Goblin : Enemy
{
    protected override Vector2 CalculateMovement(Vector2 targetPosition)
    {
        // Oblicz kierunek do gracza - fundamentalna matematyka wektorowa
        // Normalizacja zapewnia że kierunek ma zawsze długość 1, niezależnie od odległości
        Vector2 direction = (targetPosition - GlobalPosition).Normalized();

        // Goblin jest agresywny - porusza się 20% szybciej niż domyślny Enemy
        // To pokazuje jak łatwo można customizować behavior przez simple math
        float goblinSpeed = MoveSpeed * 1.2f;

        // Zwróć wektor prędkości - Godot automatycznie aplikuje to w systemie fizyki
        // To jest piękny przykład separation of concerns: my obliczamy intencję,
        // Godot zajmuje się implementation details jak kolizje i delta time
        return direction * goblinSpeed;
    }

       
    public override void _Ready()
    {
        base._Ready();
            
        GD.Print($"Goblin spawned at {GlobalPosition} - ready to hunt!");
    }
        
    public override void _ExitTree()
    {
        GD.Print($"Goblin at {GlobalPosition} has fallen! Another threat eliminated.");
            
        base._ExitTree();
    }
}