# Project Overview
- Game Title: Math-Heroes
- High-Level Concept: An educational math battle game where players solve problems to defeat enemies and progress through a world map.
- Players: Single player
- Target Platform: StandaloneOSX
- Render Pipeline: URP (Installed but not active)

# Game Mechanics
## Core Gameplay Loop
1.  Encounter an enemy.
2.  Solve math problems (+, -, *, /).
3.  Correct answers deal damage to the enemy.
4.  Incorrect answers or timeouts result in taking damage from the enemy.
5.  Defeat enemies to gain XP and unlock new zones.

## Visuals and Feedback (Improvements)
- Added "Confetti/Stars" effect for correct answers.
- Added "Smoke" and "Screen Shake" for incorrect answers.
- Added UI "Pulse" animations for questions and buttons.
- Setup URP Post-processing (Bloom and Vignette).

# UI
- **BattleUI**: The central hub for battle interactions. It will be updated to include logic for feedback effects and UI animations.

# Key Asset & Context
- `Assets/_Scripts/Battle/BattleUI.cs`: Main script to modify for UI feedback and animations.
- `Assets/Settings/UniversalRP.asset`: Existing URP asset to be assigned.
- `Assets/SunnyLand Artwork/Sprites/Fx/`: Source of sprites for feedback effects.

# Implementation Steps

## 1. Project Cleanup
- **Description**: Delete "Examples & Extras" from TextMesh Pro and any other unnecessary demo folders to reduce project size.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

## 2. Enhance BattleUI with Feedback Effects
- **Description**: Modify `BattleUI.cs` to add methods for:
    - `TriggerConfetti()`: Animates stars from the center of the screen or enemy position.
    - `TriggerSmoke()`: Animates smoke at the player's position.
    - `ShakeScreen()`: Coroutine that shakes the main Canvas or Camera.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

## 3. Implement UI Animations (Juice)
- **Description**: Add coroutines to `BattleUI.cs` for:
    - `Pulse(RectTransform target)`: Scale animation (bounce) for the question text and buttons.
    - Integrate `Pulse` into `ShowQuestion` and button click listeners.
- **Assigned role**: developer
- **Dependencies**: Step 2
- **Parallelizable**: No

## 4. Setup Post-Processing (URP)
- **Description**: 
    1. Assign `Assets/Settings/UniversalRP.asset` in **Graphics Settings** and **Quality Settings**.
    2. In `BattleScene`, create a Global Volume with **Bloom** (for magic glow) and **Vignette**.
    3. Ensure the Main Camera has "Post Processing" enabled.
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

# Verification & Testing
- **Manual Check**: Play a battle and verify:
    - Confetti appears on correct answers.
    - Smoke and screen shake appear on incorrect answers.
    - Question text "pulses" when appearing.
    - Bloom effect is visible (especially on projectiles/feedback).
- **Project Size**: Verify the "TextMesh Pro/Examples & Extras" folder is gone.
