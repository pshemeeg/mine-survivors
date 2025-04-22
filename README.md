# Mine Survivors 🚇⛏️ ///

**Mine Survivors** to dynamiczna gra akcji inspirowana tytułami takimi jak *Vampire Survivors* oraz *Brotato*. Akcja gry rozgrywa się w podziemnych korytarzach kopalń, gdzie wcielasz się w górnika eksplorującego tajemnicze tunele, zbierającego surowce oraz odpierającego fale przeciwników. Gra stanowi również świetną okazję do nauki programowania obiektowego i rozwoju portfolio.

## 📚 Spis treści

- [Opis projektu](#opis-projektu)
- [Cel projektu 🎯](#cel-projektu-)
- [Stack technologiczny 💻](#stack-technologiczny-)
- [Mechaniki rozgrywki 🎮](#mechaniki-rozgrywki-)
- [Klasy postaci 👷‍♂️](#klasy-postaci-%EF%B8%8F)
- [Architektura kodu 🏗️](#architektura-kodu-%EF%B8%8F)
- [Diagram UML 🧜‍♀️](#diagram-uml-%EF%B8%8F)
- [Licencja 📄](#licencja-)

## Opis projektu

**Mine Survivors** to gra, w której wcielasz się w górnika eksplorującego kopalnie pełne niebezpieczeństw, surowców oraz przeciwników. W trakcie rozgrywki zarządzasz maszynami wydobywczymi i korzystasz z unikalnych umiejętności, aby przetrwać w podziemnym świecie. 🌌

## Cel projektu 🎯

- Nauka programowania obiektowego poprzez praktyczną implementację gry.
- Rozwój portfolio i prezentacja umiejętności w tworzeniu gier.
- Eksperymentowanie z silnikiem **Godot** oraz innymi technologiami.

## Stack technologiczny 💻

- **Silnik gry:** Godot Engine 4.4.1 (Mono) – wykorzystujemy wbudowany Godot API do obsługi logiki gry oraz zapisu i odczytu stanu gry.
- **Język programowania:** C#
- Zapis **ustawień** i **stanu gry** odbywa się za pomocą wbudowanego systemu Godota (ConfigFile i pliki .save)
- **Grafika:** Aseprite (pixel art, 2D top-down)
- **Audio:** Audacity (edycja dźwięków, chiptune 16-bit)
- **Narzędzia:** Visual Studio / Git, GitHub
- **Gotowe assety** Itch.io, Godot asset library
- **Wsparcie AI:** ChatGPT / OpenAI o1, OpenAI o3-mini

## Mechaniki rozgrywki 🎮

- **Poruszanie się:** Sterowanie postacią w ciasnych tunelach. 🏃‍♂️
- **Automatyczny atak:** Postać automatycznie strzela do przeciwników w zasięgu. 🔫
- **Awansowanie** Rozwijaj swojego bohatera zdobywając kolejne rangi aby odblokować lepszy sprzęt. ⏫
- **Maszyny wydobywcze:** Urządzenia generujące surowce, które przyciągają wrogów. ⚙️
- **Obrona maszyn:** Fale przeciwników atakują maszyny – wymaga strategicznego podejścia. 🛡️
- **Eksploracja kopalni:** Decyduj, czy chronić maszyny, czy ryzykować zejście na niższe poziomy, gdzie czekają cenniejsze surowce i trudniejsi przeciwnicy. 🌑

## Iterfejs

**Iterfjes** pokazuje ilość hp, ilość amunicji

## Klasy postaci 👷‍♂️

Gra oferuje trzy klasy postaci, z których każda specjalizuje się we współpracy z maszynami wydobywczymi:

1. **Operator Maszyn**
   *Specjalizacja:* Sterowanie maszynami wydobywczymi.
   *Zdolność specjalna – Turbo Sterowanie:* Tymczasowo zwiększa wydajność maszyn, co przekłada się na szybsze generowanie surowców oraz krótkotrwałą ochronę przed atakami przeciwników. ⚡

2. **Mechanik Kopalni**
   *Specjalizacja:* Naprawa i ulepszanie kopalnianych urządzeń.
   *Zdolność specjalna – Szybka Naprawa:* Natychmiast przywraca część wytrzymałości pobliskim maszynom, zwiększając ich odporność na uszkodzenia przez określony czas. 🔧

3. **Eksplozjonista Górniczy**
   *Specjalizacja:* Wykorzystanie kontrolowanych eksplozji do otwierania nowych przejść, niszczenia przeszkód oraz zadawania obrażeń przeciwnikom.
   *Zdolność specjalna – Łańcuchowa Detonacja:* Inicjuje sekwencję eksplozji, która osłabia wrogów oraz może ujawniać ukryte surowce lub przełamywać strukturalne bariery kopalni. 💥

## Architektura kodu 🏗️

### 📌 Paradygmat obiektowy

- **Struktura klas**
  - Klasa bazowa `Character` dla gracza i przeciwników (zdrowie, prędkość, atak).
  - `Player` dziedziczący po `Character` (sterowanie, interakcja z obiektami).
  -  Klasy postaci: `Operator Maszyn`, `Mechanik Kopalni` i `Eksplozjonista Górniczy` dziedziczą z `Player` i implementują unikalne zdolności specjalne.
  - `Enemy` jako klasa dziedzicząca (sztuczna inteligencja, wzorce ruchu).
  - `Machine` – klasa bazowa dla maszyn wydobywczych.
  - `ResourceMachine` i `DefenseMachine` jako klasy pochodne.
  - `Item` – klasa bazowa reprezentująca przedmioty dostępne w grze. Zawiera podstawowe właściwości, takie jak unikalny identyfikator, nazwa, opis oraz wizualną reprezentację (np. ikona), umożliwiającą jednolite zarządzanie wszystkimi przedmiotami.
  - `ExperiencePoint` – klasa dziedzicząca po `Item`, reprezentująca punkty doświadczenia. Po zebraniu przez gracza automatycznie zwiększa zdobywany poziom doświadczenia, wpływając na rozwój postaci oraz odblokowywanie nowych umiejętności.
  - `Material` – klasa dziedzicząca po `Item`, reprezentująca surowce. Materiały mogą być wykorzystywane do ulepszania maszyn, budowy struktur.

- **Dziedziczenie**
- Klasy postaci (`Operator Maszyn`, `Mechanik Kopalni`, ` Eksplozjonista Górniczy`) dziedziczą z `Player` i rozszerzają możliwości poprzez swoje zdolności specjalne.
  - Przeciwnicy dziedziczą wspólne cechy z klasy `Enemy`, ale różnią się zachowaniem i AI.
  - Różne typy maszyn dziedziczą z klasy `Machine`, dostosowując funkcjonalności do konkretnych zadań.
  - Klasy `ExperiencePoint` oraz `Material` dziedziczą z klasy `Item`, ujednolicając sposób przechowywania oraz interakcji z przedmiotami w grze.

- **Polimorfizm**
  - Wspólny interfejs `IDamageable` pozwala na jednolite traktowanie wszystkich obiektów podatnych na obrażenia.
  - Interfejs `IAttack` definiuje sposób ataku (strzały, walka wręcz, eksplozje).
  - nterfejs `ICollectible` umożliwia spójny mechanizm zbierania przedmiotów przez gracza.

- **Hermetyzacja**
  - Kluczowe zmienne, takie jak `health` czy `damage`, są prywatne i dostępne wyłącznie poprzez gettery/settery, co zapewnia kontrolę nad modyfikacją wartości.
  - Właściwości klas dziedziczących po `Item` są chronione, co pozwala na bezpieczną modyfikację ich stanów podczas rozgrywki.

## Diagram UML 🧜‍♀️

Diagram przedstawia relacje między klasami w grze oraz wykorzystanie paradygmatów programowania obiektowego.

```mermaid

classDiagram
    class Character {
        - health: int
        - Movementspeed: int
        - attackDamage: int
        + getHealth() : int
        + setHealth(value: int): void
        + getSpeed(): int
        + getAttack(): int
        + takeDamage(amount: int): void
        + death(): void
    }
    %% Klasa gracza oraz jej rozszerzenia
    class Player{
        - level: int
        - experience: int
        + move(): void
        + interact(machine: Machine): void
        + collectItem(item: Item): void
        + levelUp(): void
        + useSpecialAbility(): void
    }
    Character <|-- Player : dziedziczy

    class OperatorMaszyn {
        - boostDuration: float
        - boostMultiplier: float
        + turboSterowanie(): void
        + enhanceMachines(machines: Machine[]): void
    }
    Player <|-- OperatorMaszyn : dziedziczy

    class MechanikKopalni {
        - repairAmount: int
        - durabilityBonus: float
        + szybkaNaprawa(): void
        + repairNearbyMachines(radius: float): void
    }
    Player <|-- MechanikKopalni : dziedziczy

    class EksplozjonistaGórniczy {
        - explosionRadius: float
        - explosionDamage: int
        + lancuchowaDetonacja(): void
        + placeBomb(): void
        + detonateAll(): void
    }
    Player <|-- EksplozjonistaGórniczy : dziedziczy

    %% Klasa przeciwnika
    class Enemy {
        - detectionRange: float
        - attackRange: float
        - dropRate: float
        + AI(): void
        + move(): void
        + targetMachine(machine: Machine): void
        + targetPlayer(player: Player): void
        + dropLoot(): Item[]
    }
    Character <|-- Enemy : dziedziczy

    %% Klasy związane z maszynami
    class Machine{
        - durability: int
        - maxDurability: int
        - operationTime: float
        + operate(): void
        + takeDamage(amount: int): void
        + repair(amount: int): void
        + upgrade(): void
        + breakdown(): void
    }
    class ResourceMachine{
        - resourceType: string
        - productionRate: float
        - storageCapacity: int
        + produceResource(): Material
        + getStorageLevel(): int
        + collectResources(): Material[]
    }
    class DefenseMachine{
        - attack: int
        - attackSpeed: float
        - attackRange: float
        + scanForEnemies(): Enemy[]
        + changeTargetPriority(): void
        + attack(target: IDamageable): void
    }

    Machine <|-- ResourceMachine : dziedziczy
    Machine <|-- DefenseMachine : dziedziczy

    %% Klasa bazowa dla przedmiotów
    class Item {
        - id: int
        - name: string
        - description: string
        - icon: string
        + getId(): int
        + getName(): string
        + getDescription(): string
        + getIcon(): string
    }

    class ExperiencePoint{
        - value: int
        + applyExperience(player: Player): void
        + getValue(): int
    }
    Item <|-- ExperiencePoint : dziedziczy

    class Material{
        - materialType: string
        - rarity: int
        - value: int
        + getMaterialType(): string
        + getRarity(): int
        + getValue(): int
    }
    Item <|-- Material : dziedziczy

    %% Interfejsy
    class IDamageable {
        <<interface>>
        + health: int
        + takeDamage(amount: int): void
        + isDead(): boolean
    }
    class IAttack {
        <<interface>>
        + attackDamage: int
        + attackRange: float
        + attack(target: IDamageable): void
    }
    class ICollectible {
        <<interface>>
        + collectionRadius: float
        + isCollected: boolean
        + collect(player: Player): void
    }

    %% Relacje implementacji interfejsów
    Character ..|> IDamageable : realizuje
    Machine ..|> IDamageable : realizuje
    Player ..|> IAttack : realizuje
    Enemy ..|> IAttack : realizuje
    DefenseMachine ..|> IAttack : realizuje
    Item ..|> ICollectible : realizuje
    %% Dodatkowe relacje
    Player "1" --> "*" Item : używa
    Player "1" --> "*" Machine : używa
    ResourceMachine --> Material : zawiera
    Enemy "*" --> "1..*" Machine : używa

```

## Licencja 📄

Projekt **Mine Survivors** jest udostępniony na licencji MIT. Pełna treść licencji znajduje się w pliku [LICENSE](LICENSE).

---

Dziękujemy za zainteresowanie projektem! Śledź postępy oraz zgłaszaj swoje sugestie na GitHub! 🚀
