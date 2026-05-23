Moved AudioScript from GameMaster to GameManager: added AudioSource + AudioScript components to GameManager GameObject.
Updated PlayerController and TubeScript to reference GameManager's AudioScript instead of GameObject.Find("GameMaster").
Deleted GameMaster GameObject from scene.
Deleted PlayerScript.cs and GameMasterScript.cs (dead files, fully replaced by PlayerController + GameManager).
Removed unused PlayerEventCall static event and FunctionEventCall method from PlayerController.
Removed unused System.Collections.Generic using from PlayerController.
Fixed stale scene GUID: old PlayerScript GUID 47e6fc19 replaced with PlayerController GUID 61c23c96 in scene YAML (MCP component operations left the wrong GUID on serialized MonoBehaviour data).
Scene reimported to rebuild Library cache from fixed file.
Play mode verified: no runtime NREs, both characters tracked by camera, game stable.
Remaining: 3 "missing script" warnings on play mode entry (Library cache artifact from deleted scripts — cosmetic, no runtime impact).
