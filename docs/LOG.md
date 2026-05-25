## 2026-05-21
- Moved AudioScript from GameMaster to GameManager: added AudioSource + AudioScript components to GameManager GameObject.
- Updated PlayerController and TubeScript to reference GameManager's AudioScript instead of GameObject.Find("GameMaster").
- Deleted GameMaster GameObject from scene.
- Deleted PlayerScript.cs and GameMasterScript.cs (dead files, fully replaced by PlayerController + GameManager).
- Removed unused PlayerEventCall static event and FunctionEventCall method from PlayerController.
- Removed unused System.Collections.Generic using from PlayerController.
- Fixed stale scene GUID: old PlayerScript GUID 47e6fc19 replaced with PlayerController GUID 61c23c96 in scene YAML (MCP component operations left the wrong GUID on serialized MonoBehaviour data).
- Scene reimported to rebuild Library cache from fixed file.
- Play mode verified: no runtime NREs, both characters tracked by camera, game stable.
- Remaining: 3 "missing script" warnings on play mode entry (Library cache artifact from deleted scripts — cosmetic, no runtime impact).

## 2026-05-22
- Completed rename pass (TimerScript.cs — 8 remaining string/GUIStyle fields from prior session).
- Renamed all player-identity references to neutral Red/Blue: mario/luigi, left/right across all scripts.
- Renamed `Hand` enum → `PlayerCharacter`, values Left/Right → Red/Blue; updated all call sites.
- Renamed `HandInput` struct → `PlayerCharacterInput`; `GetLeftHand`/`GetRightHand` → `GetRedInput`/`GetBlueInput`.
- Fixed bug in PlayerController.cs: `input._isShootPressedPressed` was a broken field name — corrected to `input.shootPressed`.
- Fixed Rule 4 double-underscore violations in TubeScript.cs (e.g. `__gameObjectMarioArrivedInTube`).
- Fixed missed cross-ref compile error: TubeScript.cs still referenced `assignedHand` — updated to `assignedPlayerCharacter`.
- Deleted `TextMesh Pro/Examples & Extras/` — cleared all third-party obsolete-API warnings from console.

## 2026-05-23
- Correctness pass across all 15 scripts: naming violations (Rules 1–7), unused imports, commented-out code, brace style, Update/FixedUpdate loose code extracted into named methods.
- FireBallScript: Destroy was called every FixedUpdate frame — moved to activation point so it fires exactly once per fireball.
- EnemyScript: double-underscore field, method name typo, redundant GetComponent on cached collider, indirect bool replaced with explicit values.
- PlayerController: three GameObject.Find calls replaced with GameManager.Instance; ground-check tautology removed; input field name typo fixed.
- CameraScript: redundant .transform on Transform parameter removed.
- Comment pass across all 15 scripts: header comment on every file, inline WHY comments on non-obvious logic and invariants.

## 2026-05-23
- Fixed WASD bug: rename commit wiped `assignedPlayerCharacter` on Blue player, both defaulted to Red. Reassign in Inspector.
- Same rename wiped Transform inspector refs on both players (groundCheck, headCheck, fireBallSocket, savePoint). Reassign in Inspector.
- Added `isFlying` to `EnemyScript`: gravity off, Y locked, ground checks skipped. Non-flying enemies unaffected.
- Added shoot system to `EnemyScript`: `ShootDirection` enum (L/R/U/D), interval timer, null-guarded so non-shooting enemies are safe.
- Created `ProjectileScript`: gravity scale, size, lifetime, damage, optional hit effect — all Inspector-tunable.

- New PlayerRed and PlayerBlue layers, players can now jump off each other
## 2026-05-24
- Enemy contact did no damage — `PlayerHurt()` was a stub. Added `TakeHit()` to `PlayerController`: state-based damage (Fire→Normal→Small→die), wired to enemy raycasts.
- Multi-hit bug: physics checks in `Update()` fired every frame. Moved all raycasts/OverlapCircles to `FixedUpdate()` across `EnemyScript`, `PlayerController`, `FireBallScript`. Input-driven movement stays in `Update()`.
- Added 1.5s invincibility frames after taking a hit.
- Also fixed `MoveEnemy()` running unconditionally — moved inside bounce-kill guard.
- Replaced per-player lives with shared pool: `GameManager` owns count and HUD. Added `InitializeSinglePlayerMode()` / `InitializeTwoPlayerMode()` (commented) for future two-player switch.
- Game-over fix: lives hitting 0 now forces both characters to die simultaneously via `TriggerGameOver()`.
- Introduced stack overflow: `ForcePlayerDied()` → `PlayerDied()` → `TriggerGameOver()` — infinite loop. Fixed: extracted `ExecuteDeathSequence()`, `_playerDied = true` set as first line of `PlayerDied()`.
- `ProjectileScript` damage wiring deferred to next session.

## 2026-05-24
- Goal: when player B hangs on a platform wall and player A jumps on top, B must not slide down.
- Removed: _hasPlayerOnTop detection, HoldPositionIfCarryingPlayer, IsInAir() — caused oscillation and brick bumping in earlier attempt.
- Renamed _isOnGround → _isGrounded; replaced IsInAir() with IsHanging(), added SetHanging(), UpdateWallHang().
- Tried gravity=0 + velocityY=0 in FixedUpdate — physics contact resolution overrides velocity, B still slides.
- Tried FreezePositionY — does not prevent Box2D contact position correction.
- Settled on bodyType=Static — truly immovable, contact forces cannot move it.
- Problem: Static-Static contacts generate no events in Box2D — bodyType switch fires spurious OnCollisionExit2D → rapid oscillation.
- Fixed: _bodyTypeJustChanged flag suppresses spurious exit; hang exits via _inputH==0 in UpdateWallHang.
- Problem: player grazing wall with no input engaged Static → slow slide.
- Fixed: IsHanging() returns false when _inputH==0 — Static only on active wall press.

## 2026-05-25
- Added soundPlayerHit to PlayerController — plays on shrink, not on death.
- savePointTransform was public unnecessarily — made private, renamed to _savePointTransform.
- Added LockPlayerZPosition() in LateUpdate — prevents Z drift from transform.position assignments.
- Camera boundary walls: BoxCollider2D children of the camera, local X driven by CameraScript each frame to track zoom.

