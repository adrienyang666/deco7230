# IP2b implementation notes

Current prototype version: **1.3**, Android version code **4**. This iteration adds predictive placement feedback and clearer layer feedback to version 1.2.

## Import configuration fix (2026-10-15)

The first Unity 6000.3.20f1 import screenshot showed `Assembly has duplicate references: UnityEngine.TestRunner,UnityEditor.TestRunner` for `SpatialStories.Immersive.Tests.asmdef`. The file combined explicit TestRunner references with `optionalUnityReferences: ["TestAssemblies"]`, which supplies test references during import. Removed the two redundant explicit references, retaining the test marker, Editor-only platform, runtime/XRI references and every test. This matches the existing Quest test assembly's reference pattern. Runtime version remains 1.3 because no runtime code or content changed.

Validation: all nine project assembly-definition files parse and have no repeated explicit references or combined legacy TestAssemblies/explicit TestRunner references. Only the affected assembly-definition file and three project notes changed relative to the delivered 1.3 archive. Unity recompilation, Test Runner execution and Quest verification remain pending on the user's computer.

Reference: https://docs.unity.com/en-us/engine/6000.3/manual/packages-list/cus-pkg-lp/cus-pkg-development/cus-tests

## Placement feedback update (1.3)

- `TryFindCollagePlacement` is the single query used by the live prediction and final drop. It rejects bin positions and full/deleted/inactive targets, preserves an existing layer rank, and predicts the front position when transferring to a different photo. The query does not write any transforms or ownership.
- A separate feedback root follows the target photo. Static mesh visuals and the text label are copied without interactables, colliders, rigidbodies, event handlers or controller scripts. The source and actual held object remain unchanged.
- The transparent `CollageGhost` shader preserves source texture/UVs, checks source alpha before applying 38% opacity, keeps stereo macros, and does not write depth. The ghost still depth-tests against the composition. It is a placement aid, not a full-opacity final preview.
- The target frame and RELEASE TO PLACE label appear only for a valid target. Oversized items show RESIZED TO FIT. PLACED lasts about 0.7 seconds; layer moves outline the selected element and name the crossed element for about 1.1 seconds.
- The layer buttons have overlapping-card icons, with mint denoting the moved card. The selected layer label names its back/middle/front position. NEXT LAYER still selects obscured content.
- The feedback root is a sibling of photos under contentRoot, so the photo-only RenderTexture capture does not include it. Ghost materials are reused during one grab, then destroyed on release/reset/disable. No new package or online runtime service was added.
- `enablePlacementFeedback` is enabled in both IP2b scene copies. All three extension flags are explicit. Original Full scenes remain unchanged.
- `PlacementFeedbackTests.cs` adds eight Unity EditMode cases. The shared query is exercised against actual placement, non-mutation, capacity, bin priority and stack depth. These tests require Unity and have not been run in this environment.

## First-use discovery update (1.2)

- A stationary, grabbable surfer sits half outside a small source photograph beside the existing S sphere. It reuses the same prepared PNG and source identity as the full source menu.
- An independent `EXAMPLE` composition combines Auckland, the surfer and overlapping WEEKEND text. It is parented to `contentRoot`, not a user photo, and never enters the editable overlay list or publish capture.
- The first editor entry in each trial runs a two-second lift/move/settle animation on a separate, non-interactive ghost. The ghost has no enabled collider, `FloatingStoryItem` or XR interactable.
- Aiming at the real source stops the motion. Grabbing or selecting an authoring action consumes the invitation, hides NEW and the example, and leaves the compact source entry available.
- The compact entry hides while text/sticker/source/admin panels are open. It is separate from held subjects, so dismissing a panel or the invitation cannot disable the grabbed object.
- Returning to the album or editor does not rearm the animation. Reset does, per participant. `enableCutoutDiscovery` can disable this extension for a separately labelled comparison condition.

Source-menu and direct-entry handles are distinguished by `isDiscoveryHandle`. Both are registered in `sourceHandles` until lifted, then become ordinary overlays. This avoids treating an inactive direct handle as the menu's existing source, or hiding a held cutout with its display card.

## Delta from the supplied IP2a

| Existing behaviour | IP2b change |
|---|---|
| Floating gallery with six photos, T/S spheres | Preserved scene layout and XR rig; new IP2b copies enable the collage extension. |
| Shape sticker spawned beside one photo | S → FROM PHOTO exposes two photo detail cards with grabbable prepared subjects. |
| Overlay placement limited to selected photo | Drop geometry finds a nearby photo, preserves world size across parent changes, fits rotated bounds, then reparents after XRI detach. |
| Depth based on global creation index | Each photo has explicit layer ownership and order. Forward/backward swaps adjacent visible layers; undo retains logical order. |
| Two-hand scaling could also turn the object | A final grab transformer holds rotation while two controllers scale; one-hand rotation remains. |
| Review of the spatial composition | A real orthographic RenderTexture capture displays a flat image before simulated publishing. |

The new task tests selecting a subject from one image, relocating it into another and deciding occlusion relative to words. This is a project-level interaction extension, not a claim that cutout collage itself is a new invention.

## Files and implementation

- `ImmersiveStory.cs`: guarded IP2b hooks, release-completion gating, deliberate bin drop, preview availability.
- `ImmersiveStory.Collage.cs`: runtime source cards, layer controls, ownership, transfer, recovery and material lifetime.
- `ImmersiveStory.Preview.cs`: mono offscreen capture in Built-in RP; temporary capture layers/cameras/highlights restored in `finally`; confirmation waits for a successful capture.
- `ImmersiveStory.PlacementFeedback.cs`: independent live prediction, transient confirmation, layer icons and visual cleanup.
- `Resources/CollageGhost.shader`: transparent, stereo-aware prediction material.
- `ImmersiveStory.Discovery.cs`: direct entry, example visuals, two-second ghost animation and runtime visibility/interruption.
- `CutoutDiscoveryState.cs`: per-trial one-shot, completion, interruption and reset state, independent of animation timing.
- `FloatingStoryItem.cs`: collage metadata, Busy state while XRI finishes releasing, stable scaling registration.
- `CollagePlacement.cs`: photo-local drop projection and rotated footprint fitting. The supplied scene uses positive uniform scales.
- `CollageScaleRotationLock.cs`: registered after `XRGeneralGrabTransformer` in the multiple-hand pipeline.
- `CollageDepth.shader`: stereo-compatible alpha-tested, depth-writing material for cutouts and caption glyphs. Transparent pixels do not obscure other layers.
- `IP2bProjectTools.cs`: open scene, validate essential resources/settings and build only the IP2b Quest scene.
- `Tests/Editor/CollageRegressionTests.cs`: 7 EditMode cases, supplied but not executed in this environment.
- `Tests/Editor/CutoutDiscoveryTests.cs`: 5 additional EditMode cases for invitation lifecycle; also not executed here.

Both new scenes are byte-preserving copies of their original Full scenes except the `enableCollage`, `enableCutoutDiscovery` and `enablePlacementFeedback` fields. Original Full scene assets, six source photographs, rig references, dependency manifests and XR/OpenXR settings were retained. Shared code takes the original behaviour when `enableCollage` is false. Product name, version, Android application ID and enabled build scene intentionally changed in this new project copy.

All additional UI and materials are created at runtime. Inspect in Play mode to reposition or tune the controls; source placement constants live in `InitializeCollage`. This keeps the supplied scenes and rig intact, but the new controls are not prebuilt editable scene objects outside Play mode.

## Asset provenance and disclosure

AI assistance: the new runtime code, shader, editor tools, tests, documentation and project configuration in this IP2b copy were prepared with ChatGPT/Codex assistance. Their behaviour still requires the author's review and Unity/Quest verification.

Original inputs: user-supplied `Assets/SpatialStories/Textures/Photo_03.png` (Piha) and `Photo_05.png` (Paris).

Prepared assets: `Runtime/Resources/IP2b/Surfer.png` and `Eiffel.png`, made on 2026-10-14 with the built-in image generation/editing tool using those photos as references and `transparent_background=true`.

The actual prompts are preserved verbatim in `IP2b_ASSET_PROMPTS.json`. They requested only the surfer/board or illuminated tower, removal of the surroundings, transparent backgrounds, preservation of pose/perspective and no sticker outlines. These are AI-assisted reconstructions, not exact segmentation masks. Original generated RGBA files are copied without pixel editing. The material samples a tight UV rectangle around their visible alpha region and uses a 0.5 alpha cutoff to suppress peripheral glow. Text uses a lower cutoff for glyph edges.

The app does not claim runtime segmentation. Only these two prepared subjects can be lifted; choosing arbitrary image regions, importing photographs and automatic background repair are outside this prototype.

## Logging and evaluation

Local CSV folder: `Application.persistentDataPath/SpatialStories_IP2b/`.

New events include `open_photo_sources`, `lift_photo_subject`, `place_photo_subject`, `cross_photo_composition`, `transfer_layer`, `fit_to_photo`, `invalid_subject_drop`, `layer_forward`, `layer_backward`, `flat_preview_ready` and `flat_preview_failed`. Existing grip, two-hand interaction, delete, undo, prompt and publish events remain.

Version 1.2 adds `cutout_discovery_shown`, `cutout_demo_completed`, `cutout_demo_stopped_on_aim`, `cutout_discovery_dismissed` and `cutout_entry_used`. The last event distinguishes `direct_beside_s` from `source_menu`. A demonstration is interface guidance, not a participant action or successful task result. Event elapsed time is relative to BEGIN; subtract `cutout_discovery_shown` from the first `cutout_entry_used` to measure delay from the invitation to the first successful grip. First intentional attempts, including failed grips, need observer timing. Record facilitator help separately.

Version 1.3 adds `prototype_config` (version/discovery/placement-feedback flags), `placement_preview_target` (first valid target or target switch during a grip), and `placement_preview_lost` (leaving a valid target). No per-frame event is written. These events indicate system state, not confirmed user attention.

The full protocol and five academic references are in `IP2b_TESTING_PLAN.md`; blank participant and event records are included. The exact five lecture-principle names still need the lecture source and are explicitly left pending in `IP2b_DESIGN_REVIEW.csv`.

`cross_photo_composition` means a placed subject's source photo ID differs from its destination. A participant can publish without completing the requested collage, so log completion is not a task-success score. Record independent completion, help, recovery, qualitative feedback and device type separately. No participant evidence was generated during development.

## API references

- Unity XR Interaction Toolkit 3.6, XRGrabInteractable: https://docs.unity.cn/Packages/com.unity.xr.interaction.toolkit@3.6/manual/xr-grab-interactable.html
- Unity XRBaseGrabTransformer API: https://docs.unity.cn/Packages/com.unity.xr.interaction.toolkit@3.5/api/UnityEngine.XR.Interaction.Toolkit.Transformers.XRBaseGrabTransformer.html
- Unity Camera.Render: https://docs.unity3d.com/ScriptReference/Camera.Render.html
- Unity Camera.stereoTargetEye: https://docs.unity3d.com/ScriptReference/Camera-stereoTargetEye.html

- Unity single-pass instancing: https://docs.unity3d.com/6000.0/Documentation/Manual/SinglePassInstancing.html
- Unity shader blending: https://docs.unity3d.com/6000.0/Documentation/Manual/writing-shader-blending-modes.html

These are implementation references, not a substitute for the assignment's academic literature requirements. Unity 6000.3.20f1 import/API compilation and Quest validation remain pending.
