# libTech - TODO

A list of planned features, improvements, and tasks for this project.

> **CPX (Complexity Points)** - 1 to 5 scale:
> - **1** - Single file control/component
> - **2** - Single file control/component with single function change dependencies
> - **3** - Multi-file control/component or single file with multiple dependencies, no architecture changes
> - **4** - Multi-file control/component with multiple dependencies and significant logic, possible minor architecture changes
> - **5** - Large feature spanning multiple components and subsystems, major architecture changes

> Instructions for the TODO list:
- Move all completed items into separate Completed section
- Consolidate all completed TODO items by combining similar ones and shortening the descriptions where possible

> How TODO file should be iterated:
- First handle the Uncategorized section, if any similar issues already are on the TODO list, increase their priority instead of adding duplicates (categorize all at once)
- When Uncategorized section is empty, start by fixing Active Bugs (take one at a time)
- After Active Bugs, handle the rest of the TODO file by priority and complexity (High priority takes precedance, then CPX points) (take one at a time).

---

## 🔴 .NET 9 Modernization **HIGHEST PRIORITY**

Migrate the entire solution from its current mixed .NET Framework 4.6–4.7 / .NET 7.0 state to .NET 9.0. This is a prerequisite for continued development — .NET 7 is end-of-life and .NET Framework 4.x blocks access to modern C# features and packages.

**Current state of all managed projects:**

| Project | Current Target | csproj Style | Depends On |
|---|---|---|---|
| libTech2 (engine) | net7.0 | SDK-style | FishGfx, FishGfx_Nuklear, NuklearDotNet, FML, LuaDotNet |
| FishGfx | .NET Fw 4.6.1 | Old-style | NuklearDotNet (sub-submodule) |
| FishGfx_Nuklear | .NET Fw 4.6.1 | Old-style | FishGfx, NuklearDotNet |
| NuklearDotNet | .NET Fw 4.6.1 | Old-style | — |
| FML (FishMarkupLanguage) | .NET Fw 4.6.1 | Old-style | — |
| LuaDotNet (Lua.NET) | .NET Fw 4.6 | Old-style | — |
| Game | .NET Fw 4.7 | Old-style | old libTech, FishGfx, FML, LuaDotNet |
| LegProcessor_Game | .NET Fw 4.7 | Old-style | old libTech, FishGfx |
| libTech_Bootstrapper | .NET Fw 4.7 | Old-style | old libTech (uses DllExport for native entry) |
| libTech (old engine) | .NET Fw 4.7 | Old-style | FishGfx, FishGfx_Nuklear, NuklearDotNet, FML, LuaDotNet |

### Phase 1 — Leaf Libraries (no managed dependencies)

These projects have no references to other managed projects in the solution and can be migrated first.

- [ ] **Convert Lua.NET to SDK-style csproj targeting net9.0** — Currently .NET Fw 4.6 old-style. Convert `LuaDotNet/Lua.NET/Lua.NET.csproj` to SDK-style, retarget to `net9.0`. Remove legacy AssemblyInfo.cs (use csproj properties). Verify native `lua51.dll` P/Invoke still works. — **CPX 2**
- [ ] **Convert FishMarkupLanguage to SDK-style csproj targeting net9.0** — Currently .NET Fw 4.6.1 old-style. Convert `FML/FishMarkupLanguage/FishMarkupLanguage.csproj` to SDK-style, retarget to `net9.0`. — **CPX 2**
- [ ] **Convert NuklearDotNet to SDK-style csproj targeting net9.0** — Currently .NET Fw 4.6.1 old-style. Convert `FishGfx/submodule_NuklearDotNet/NuklearDotNet/NuklearDotNet.csproj` to SDK-style, retarget to `net9.0`. Verify native `Nuklear.dll` P/Invoke. — **CPX 2**

### Phase 2 — Mid-Level Libraries

Depend on Phase 1 projects. Migrate after leaf libraries compile on net9.0.

- [ ] **Convert FishGfx to SDK-style csproj targeting net9.0** — Currently .NET Fw 4.6.1 old-style. Depends on NuklearDotNet. Replace raw DLL references (OpenGL.Net) with NuGet packages where available. Handle `System.Drawing` usage — replace with `System.Drawing.Common` NuGet or alternative. Verify GLFW/OpenGL native interop. — **CPX 3**
- [ ] **Convert FishGfx_Nuklear to SDK-style csproj targeting net9.0** — Currently .NET Fw 4.6.1 old-style. Depends on FishGfx and NuklearDotNet. — **CPX 2**

### Phase 3 — Main Engine

- [ ] **Update libTech2 from net7.0 to net9.0** — Change `<TargetFramework>net7.0</TargetFramework>` to `net9.0` in `libTech2/libTech2.csproj`. Update NuGet packages: `Magick.NET-Q16-AnyCPU` (13.4.0 → latest .NET 9 compatible), `SharpZipLib` (1.4.2 → latest), `System.Drawing.Common` (7.0.0 → 9.0.x). Address `System.Drawing.Common` Windows-only restriction (add `<RuntimeIdentifier>win-x64</RuntimeIdentifier>` or migrate away from System.Drawing). Update BEPUphysics reference to .NET 9 build. — **CPX 3**

### Phase 4 — Support Projects & Old Engine Cleanup

- [ ] **Decide fate of old libTech project** — The old `libTech/` (NET Fw 4.7) is largely a duplicate of `libTech2/`. Decide: deprecate and remove from solution, or convert. If keeping, convert to SDK-style targeting net9.0 and replace all raw DLL references (BulletSharp, OpenGL.Net, SharpFileSystem, SourceUtils, Facepunch.Parse, etc.) with NuGet equivalents. Remove `packages.config`. — **CPX 4**
- [ ] **Migrate or deprecate Game project** — Currently .NET Fw 4.7, references old libTech. Either retarget to net9.0 referencing libTech2, or remove from solution. — **CPX 2**
- [ ] **Migrate or deprecate LegProcessor_Game** — Currently .NET Fw 4.7, references old libTech. Same decision as Game project. — **CPX 2**
- [ ] **Migrate or deprecate libTech_Bootstrapper** — Currently .NET Fw 4.7 with DllExport (allows loading .NET from native C launcher). DllExport doesn't support .NET 9. Replace with `hostfxr` hosting API, NativeAOT, or a different launch strategy. — **CPX 4**

### Phase 5 — Solution & Build Cleanup

- [ ] **Clean up solution file** — Remove deprecated projects from `libTech.sln` (or add migrated ones). Ensure all net9.0 projects build. Remove stale platform configurations if no longer needed. — **CPX 1**
- [ ] **Remove packages/ directory and packages.config files** — After all projects use SDK-style csproj with PackageReference, the old `packages/` NuGet folder and `packages.config` files are no longer needed. — **CPX 1**
- [ ] **Update submodule references** — Ensure FishGfx, FML, LuaDotNet submodule pins point to commits with .NET 9 csproj changes. — **CPX 1**
- [ ] **Verify full solution build on .NET 9 SDK** — End-to-end build verification. Fix any remaining API breaks from .NET Fw/.NET 7 → .NET 9 (e.g., obsolete APIs, breaking changes in BCL). — **CPX 2**

---

## Features

### High Priority

- [ ] **Implement DbgDrawPhysics methods** — 15+ debug draw methods (DrawAabb, DrawBox, DrawArc, DrawPlane, DrawTriangle, DrawTransform, etc.) all throw `NotImplementedException`. Only DrawLine, DrawCylinder, DrawContactPoint, and DrawSphere partially work. `Graphics/DbgDrawPhysics.cs` — **CPX 3**
- [ ] **Implement VirtualFileSystem write operations** — `CreateDirectory()`, `CreateFile()`, and `Delete()` all throw `NotImplementedException` in the base `FileSystemProvider` class. `FileSystem/VirtualFileSystem.cs:18-28` — **CPX 3**
- [ ] **Complete Lua.NET object/delegate marshaling** — Cannot push non-null objects to Lua (`Advanced.cs:74`), Thread/UserData types cannot be marshaled (`Advanced.cs:65`), and delegate marshaling is stubbed out (`Advanced.cs:129`). `LuaDotNet/Lua.NET/Advanced.cs` — **CPX 3**
- [ ] **Enable and complete player physics system** — Player physics disabled by default (`EnablePlayerPhys = false`). Old Bullet physics implementation was removed; new BEPUphysics integration incomplete. Commented-out capsule shape, rigid body construction. `Entities/Player.cs:57-95` — **CPX 4**

### Medium Priority

- [ ] **Implement voxel lighting system** — Both `Chunk.cs:176` and `ChunkMap.cs:399` have `// TODO: Actual lights` — only ambient occlusion attempted, no point/directional light support. `Graphics/Voxels/` — **CPX 4**
- [ ] **Port FreetypeFont to FishGfx** — File-level TODO (`Graphics/FreetypeFont.cs:15`): `// TODO: Port to FishGfx`. Font rendering is tightly coupled to engine instead of the graphics library. — **CPX 4**
- [ ] **Implement BSP water rendering** — Water materials are hardcoded to a placeholder string `"water"` instead of proper water shader/rendering. `Map/BSP.cs:402-404` — **CPX 3**
- [ ] **Complete GUI skinning system** — Skinning marked TODO (`GUI/NuklearGUI.cs:111`); hardcoded texture atlas coordinates for UI elements. — **CPX 3**
- [ ] **Implement NuklearGUI Lua call stacking** — Cannot recurse Lua GUI calls (`GUI/NuklearGUI.cs:586`): `// TODO: Stack these so you can recurse`. — **CPX 3**
- [ ] **Implement FishGfx SMD Save** — `Smd.Save()` throws `NotImplementedException`. `FishGfx/Formats/Smd.cs:98` — **CPX 3**
- [ ] **Complete FishGfx Mesh2D** — `// TODO: Port code from Mesh3D` — 2D mesh rendering not implemented. `FishGfx/Graphics/Drawables/Mesh2D.cs` — **CPX 3**
- [ ] **Complete ModelConv format conversions** — Multiple format conversion paths throw `NotImplementedException`. FOAM format import/export commented out entirely. `FishGfx/ModelConv/Program.cs:64-81` — **CPX 3**
- [ ] **Implement font kerning support** — FreetypeFont skips kerning values (`Graphics/FreetypeFont.cs:189`); BitmapFont throws `NotImplementedException` for kerning pairs (`FishGfx/Formats/BitmapFont.cs:145`). — **CPX 2**
- [ ] **Complete CVar and ArgumentParser type parsing** — Both throw `NotImplementedException` for unsupported types. CVar supports only int/float/bool/string (`Engine/CVar.cs:159`); ArgumentParser similar (`Engine/ArgumentParser.cs:113`). — **CPX 2**
- [ ] **Implement Msdfgen glyph methods** — Two `NotImplementedException` throws in native binding. `native/Msdfgen.cs:33,51` — **CPX 2**
- [ ] **Complete FML value parsing and escape handling** — Non-boolean/number identifiers throw `NotImplementedException` (`FML.cs:336`); string escaping incomplete (`FML.cs:347`: `// TODO: Make it better`). — **CPX 2**
- [ ] **Optimize NuklearGUI Lua environment usage** — Creates a new environment per Lua invocation instead of reusing (`GUI/NuklearGUI.cs:49`): `// TODO: Single environment`. Also `line 453`: setup variables once and copy. — **CPX 2**
- [ ] **Implement DDS/VTF unsupported format handling** — Both throw `NotImplementedException` for some texture formats. `Utilities/DDS.cs:91`, `Textures/VTF.cs:69` — **CPX 2**
- [ ] **Complete PhysShape missing shape types** — `AddToSimulation` and `ComputeInertia` throw `NotImplementedException` for unhandled shapes. `Physics/PhysShape.cs:57,77` — **CPX 2**
- [ ] **Complete EntityKeyValues type support** — Only supports Color, float, string; other types throw `NotImplementedException`. `Entities/Entity.cs:130` — **CPX 2**

### Lower Priority

- [ ] **Implement DbgDraw.DrawAABB** — Throws `NotImplementedException`. `Graphics/DbgDraw.cs:70` — **CPX 1**
- [ ] **Implement Utils.ToBitmap for non-Windows** — Throws `NotImplementedException("ToBitmap not implemented on non-Windows OS")`. `Utilities/Utils.cs:512` — **CPX 2**
- [ ] **Complete UTF-8 code point conversion** — `// TODO: convert to code points` in `Utilities/Utils.cs:188` — **CPX 2**
- [ ] **Implement FishGfx render state caching** — `// TODO: Cache state and only do delta-enable` — all states applied every call. `FishGfx/Graphics/Gfx.cs:71` — **CPX 3**
- [ ] **Implement Color.Clamp()** — Currently returns `Color.White` with `// TODO`. `FishGfx/Color.cs:87` — **CPX 1**
- [ ] **Implement FishGfx font padding** — `// TODO: Padding 'nd shit` in `FishGfx/GfxFont.cs:82` — **CPX 1**

---

## Improvements

- [ ] **Replace generic Exception throws with specific types** — ~15+ locations across the codebase throw `new Exception(...)` instead of `ArgumentException`, `InvalidOperationException`, `FileNotFoundException`, etc. Affects: `Map/libTechMap.cs`, `Importer/Importer.cs`, `Physics/PhysShape.cs`, `Scripting/Lua.cs`, `GUI/NuklearGUI.cs`, `GUI/libGUI.cs`, `Graphics/Voxels/ChunkMap.cs`, `native/RenderDoc.cs`, `Entities/Entity.cs` — **CPX 2**
- [ ] **Extract hardcoded Steam/game paths to configuration** — `"C:/Program Files (x86)/Steam/steamapps/common/GarrysMod"` hardcoded in `libTech2.cs:152` and `Program.cs:151-152`. Should use config file, environment variable, or Steam registry. — **CPX 2**
- [ ] **Make view model FOV a ConVar** — Hardcoded 54° FOV (`Entities/Player.cs:64`): `// TODO: Convar the view model FOV` — **CPX 1**
- [ ] **Move frame cap to proper location** — Frame cap logic in main loop (`libTech2.cs:308`): `// TODO: Move frame cap somewhere else`. Uses `Thread.Sleep(0)` spin-wait. — **CPX 1**
- [ ] **Normalize GUI child positions to parent client area** — Only right/top borders accounted for, not left/bottom (`GUI/Controls/Control.cs:49`). — **CPX 2**
- [ ] **Refactor ResourceManager auto-loading** — `GetTexture()` auto-loads missing textures and can recurse infinitely if error texture also missing (`Engine/ResourceManager.cs:33-48`). Add depth limit or explicit load pattern. — **CPX 2**
- [ ] **Add null pointer validation in libTech_DedicatedGPU** — `LoadLibrary()` and `GetProcAddress()` return values never checked before use (`libTech_DedicatedGPU/main.c:14-28`). Add `FreeLibrary()` cleanup. — **CPX 1**
- [ ] **Add error handling to Bootstrapper** — No try-catch around `Main()`, always returns 0 regardless of errors. `libTech_Bootstrapper/Bootstrapper.cs:13` — **CPX 1**
- [ ] **Fix thread safety in LegProcessor_Game** — Lock doesn't protect all shared state; `Parallel.For` modifies vertex arrays without full synchronization. `LegProcessor_Game/LegProcessor.cs:150-196` — **CPX 3**
- [ ] **Decouple draw and update in FishGfx game loop** — Update runs after draw (`FishGfx/Game/FishGfxGame.cs:72`): `// TODO: Decouple draw and update`. — **CPX 2**
- [ ] **Unify FishGfx framebuffer attachment handling** — Separate dictionaries for Texture and Renderbuffer attachments (`FishGfx/Graphics/FramebufferObject.cs:12`): `// TODO: Unify`. — **CPX 2**
- [ ] **Add null validation in libNative callbacks** — Global `RenderFunc` pointer called without null check (`libNative/libNative.cpp:11,18`); `GizmoInit()` has no double-init guard. — **CPX 1**
- [ ] **Finalize Noise scale factors** — Both simplex 2D/3D use preliminary scale factors (`Engine/Noise.cs:169,264`): `// TODO: The scale factor is preliminary!` — **CPX 1**
- [ ] **Enable nullable reference types** — `<Nullable>disable</Nullable>` in libTech2.csproj removes null safety. — **CPX 3**

---

## Documentation **LOW PRIORITY**

- [ ] API reference documentation
- [ ] Getting started guide
- [ ] Architecture overview — document the relationship between libTech (old, .NET 4.7) and libTech2 (new, .NET 7.0)
- [ ] Document submodule version requirements (FishGfx, FML, LuaDotNet)
- [ ] Add build instructions to README.md
- [ ] Document the x64-only platform requirement and native dependency setup

---

## Code Cleanup & Technical Debt

### Code Refactoring

- [ ] **Rename inappropriate variable names** — `FuckOff` variable in `native/Msdfgen.cs:73`; profanity in FishGfx comments. — **CPX 1**
- [ ] **Remove large commented-out code blocks** — `Entities/Player.cs` (old Bullet physics), `Utilities/Utils.cs` (old physics conversions), `Engine/GConsole.cs` (console UI), `Graphics/Voxels/Chunk.cs` (old Raylib calls), `Game/BaseGame.cs` (dead menu code), `FML/FML.cs:276-291`, `libNative.cpp:2,7-8` — **CPX 1**
- [ ] **Implement or remove GConsole stubs** — `Update()`, `Clear()`, and `Write()` contain bare `// TODO` with no implementation. `Engine/GConsole.cs:195-205` — **CPX 2**
- [ ] **Resolve libTechModel.Vertices field** — Marked `// TODO: Remove?` (`Models/libTechModel.cs:27`). Decide if still needed. — **CPX 1**
- [ ] **Update outdated NuGet packages (old libTech)** — If old libTech is kept: CARP.ArgumentParser 1.1.0, Magick.NET 7.10.1, System.Linq.Dynamic 1.0.7, System.Numerics.Vectors 4.5.0. (Covered by .NET 9 Modernization Phase 4 if project is migrated) — **CPX 2**
- [ ] **Remove or fix ValveMaterial goto pattern** — `goto RETRY` used for include handling (`Materials/ValveMaterial.cs:49`); should use recursion. — **CPX 2**
- [ ] **Clean up DbgDrawPhysics commented-out throws** — Multiple methods have `//throw new NotImplementedException()` with partial implementations. Finalize which are done. `Graphics/DbgDrawPhysics.cs:54,71,75,92` — **CPX 1**
- [ ] **Fix BSP path resolution hack** — `// TODO: Ugh, fix` — uses `Path.GetFullPath("." + FilePath)` (`Map/BSP.cs:325-326`). — **CPX 2**
- [ ] **Include missing projects in solution** — Game, LegProcessor_Game, and Bootstrapper projects not in libTech.sln. — **CPX 1**
- [ ] **Update LuaDotNet .csproj** — Still using ToolsVersion="4.0" (VS2012 era). (Covered by .NET 9 Modernization Phase 1 — SDK-style conversion) — **CPX 1**
- [ ] **Add CI/CD pipeline** — `.github/workflows/` is empty (only FUNDING.yml exists). — **CPX 3**
- [ ] **Remove FishGfx System.Drawing dependency from AABB** — `// TODO: REMOVE!` on import (`FishGfx/AABB.cs:8`); still used for `Rectangle.Union`. — **CPX 1**

---

## Known Issues / Bugs

### Active Bugs

- [ ] **ResourceManager infinite recursion** — `GetTexture()` calls `Load<Texture>()` which on failure falls back to `GetTexture("error")`. If error texture also missing, infinite recursion. `Engine/ResourceManager.cs:34-44` — **CPX 2**
- [ ] **libTech_DedicatedGPU null pointer dereference** — If `LoadLibrary()` fails, handle is null but immediately used. If `GetProcAddress()` fails, function pointer is null but called. `libTech_DedicatedGPU/main.c:14-28` — **CPX 1**
- [ ] **BSP coordinate transformation uncertainty** — `// TODO: Is this correct? A better way to fix?` — nodraw texture filtering may skip valid faces. `Map/BSP.cs:390-394` — **CPX 2**
- [ ] **Player ground trace not handled** — `// TODO: Ground trace missed` — no fallback when ground trace fails. `Entities/Player.cs:289/317` — **CPX 2**
- [ ] **GUI Control size calculation missing SizeMode** — `Size` getter throws `NotImplementedException` for unhandled `SizeMode` values. `GUI/Controls/Control.cs:55` — **CPX 1**
- [ ] **FishGfx GameLevel Y-axis inversion** — Multiple TODOs about coordinate system inconsistency between editor and engine. `FishGfx/Test/GameLevel.cs:44,56,73` — **CPX 2**
- [ ] **FishGfx BufferObject incomplete disposal** — `GraphicsDispose()` has `// TODO` — potential GPU memory leak. `FishGfx/Graphics/BufferObject.cs:79` — **CPX 1**
- [ ] **FishGfx MSAA validation missing** — `// TODO: Check MSAA` in framebuffer attachment. `FishGfx/Graphics/FramebufferObject.cs:57` — **CPX 2**
- [ ] **LegProcessor thread safety** — `Parallel.For` modifies shared vertex arrays; `Interlocked` operations don't fully protect state. `LegProcessor_Game/LegProcessor.cs:150-196` — **CPX 3**
- [ ] **Glyph packing overflow** — When atlas is full, `NotImplementedException("Cannot pack glyph")` crashes instead of resizing or handling gracefully. `Graphics/FreetypeFont.cs:154` — **CPX 2**

### Uncategorized (Analyze and create TODO entries in above appropriate sections with priority. Do not fix or implement them just yet. Assign complexity points where applicable. Do not delete this section when you are done, just empty it)

*No uncategorized items*

---

## Notes

- **Solution structure**: libTech (old, .NET Framework 4.7) and libTech2 (new, .NET 7.0) share most source files. libTech2 is the active version.
- **Submodules**: FishGfx (graphics), FML (markup language), LuaDotNet (Lua bindings) are git submodules.
- **Native dependencies**: libNative (C++), libTech_DedicatedGPU (C) — x64 only.
- **Physics**: Transitioned from BulletSharp to BEPUphysics; migration incomplete in several areas.
- **GUI**: Uses Nuklear via NuklearDotNet with Lua scripting for UI layouts (FML documents).
- **Content pipeline**: Supports Source engine formats (VPK, VMT, VTF, BSP, SMD).

---

## Completed

### Features

*No completed features yet*

### Improvements

*No completed improvements yet*

### Fixed Bugs

*No fixed bugs yet*
