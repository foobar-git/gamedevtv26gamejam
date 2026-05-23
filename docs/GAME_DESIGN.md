# 2 Player Platformer — Game Design Document

## Elevator Pitch
A classic 2D Mario-style platformer where one player controls **both characters simultaneously**. WASD for Mario, Arrow keys for Luigi, Spacebar to swap which hand controls which character. Both must reach the goal. Coordination is the game.

## Core Premise
- 2D side-scrolling platformer in the style of Super Mario Bros.
- Two characters on screen.
- **One player** controls both at the same time
- Both characters must advance for the camera to advance
- Both must finish the level (reach the flagpole / goal) — order doesn't matter
- If one dies, the level resets (shared lives or instant reset — TBD)

## Controls
| Action  | Default Left  | Default Right  |
|---------|---------------|----------------|
| Move    | A / D         | ← / →          |
| Jump    | W             | ↑              |
| Special | S             | ↓              |
| Swap    | Spacebar      | —              |

- **Spacebar** toggles which control scheme maps to which character
- This prevents the "crossed arms" problem where Mario is on the right but controlled by the left hand

## Camera
- **Single screen** — both characters always visible
- Camera advances only when **both** characters have crossed a given threshold
- Prevents one character from being dragged off-screen by the other
- Forces the player to keep both characters moving forward together

## Cooperative Mechanics

### Direct Physical
- **Stacking** — One stands on the other's head to reach high ledges
- **Tether swing** — Short rope between them; one holds a ledge, the other swings across gaps
- **Body bridge** — One lies flat so the other walks across (then teleport / rope retrieval to reunite)
- **Throw launch** — One throws the other upward or across gaps

### Shared Obstacles
- **Weight buttons** — Both must stand on pressure plates simultaneously to open gates
- **Double-brick** — Certain blocks only break when both hit from below at the same time
- **Co-op stomp** — Enemies that require two stomps (one from each character) to defeat
- **Shared carry** — Heavy objects neither can lift alone, but together they can push/drag

### Puzzle Elements
- **Electric floor** — One stands on a deactivation pad while the other crosses
- **Weight-sensitive platforms** — Only move if both are on them
- **Phasing blocks** — Only one character can pass; they must hit a switch for the other
- **Key / door** — One character holds a key, the other opens the door on the other side

### Enemy Interactions
- **Double stomp** — A tough enemy (e.g. spiked shell) requires both characters to stomp in quick succession
- **Pass the hot potato** — An enemy grabs one character; the other must attack it to free them
- **Bait and switch** — One character lures an enemy into a trap while the other triggers it

## Level Completion
- Both characters must reach the **flagpole / goal zone**
- Order does not matter
- Camera only considers a level "cleared" once both have crossed the finish line

## Design North Star
> Every obstacle should be **trivial for a single character** but **requires coordination** between the two.

The game is not about raw platforming skill — it's about managing two bodies in space at once. Split attention, coordination, and timing are the real challenges.

## Future Expansions (not in scope for Phase 1)
- Power-ups (shared or per-character)
- Cooperative power-ups (e.g., one gets a propeller, the other holds on)
- Secret exits requiring specific co-op actions

