## 2026-05-23
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

## 2026-05-23
- Completed rename pass (TimerScript.cs — 8 remaining string/GUIStyle fields from prior session).
- Renamed all player-identity references to neutral Red/Blue: mario/luigi, left/right across all scripts.
- Renamed `Hand` enum → `PlayerCharacter`, values Left/Right → Red/Blue; updated all call sites.
- Renamed `HandInput` struct → `PlayerCharacterInput`; `GetLeftHand`/`GetRightHand` → `GetRedInput`/`GetBlueInput`.
- Fixed bug in PlayerController.cs: `input._isShootPressedPressed` was a broken field name — corrected to `input.shootPressed`.
- Fixed Rule 4 double-underscore violations in TubeScript.cs (e.g. `__gameObjectMarioArrivedInTube`).
- Fixed missed cross-ref compile error: TubeScript.cs still referenced `assignedHand` — updated to `assignedPlayerCharacter`.
- Deleted `TextMesh Pro/Examples & Extras/` — cleared all third-party obsolete-API warnings from console.

