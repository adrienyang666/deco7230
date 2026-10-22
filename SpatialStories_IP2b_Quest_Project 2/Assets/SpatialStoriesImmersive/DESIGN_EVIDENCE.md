# Spatial Stories — Design evidence and implementation decisions

This reconstruction used the full text of the original concept report, IP1 testing plan, key findings and evaluation. The handwritten sheets were not visually rechecked in this pass; participant figures below are from the existing evaluation.

## Documents revisited

- DECO7230_Concept_Report_Zixu_Yang_48939559_With_AI_Coversheet_Final.pdf
- Instagram_Story_XR_Concept_Report.docx
- Spatial_Stories_IP1_Testing_Plan_Zixu_Yang_48939559.docx
- Spatial_Stories_IP1_Key_Findings.md
- Spatial_Stories_Design_Evaluation_1_GitHub.md and its editable report with typed Appendix A

The original concept and recorded findings take priority over later portfolio summaries. New implementation choices are identified separately from tested findings.

## Required preservation and supported iteration

| Element | Original concept / actual IP1 implementation | Feedback evidence | Faithful implementation implication |
| --- | --- | --- | --- |
| Floating photo wall | Curved album wall, floating photos/videos in front of user at comfortable viewing/reach distance. IP1 uses six personal travel photos. | Floating intuitive, rotation useful (P02); KF2 explicitly retain spatial object model rather than conventional 2D menu. | Restore clearly spatial curved free-floating cards, maintain 6 actual photographs, visible depth and selection; do not hide concept behind a fixed flat desk UI. |
| Selection | Point or pinch -> yellow outline. IP1 adds hover scale/forward movement/bright caption. | 0/5 prompt-free indicates discoverability weakness, not reason to remove state. | Strong hover/selection highlight, distinct states and gentle forward offset. |
| Grab / inspect | Directly grab selected media, follows hand and expands to centre; head/body inspection; pinch inward minimises and returns to album. IP1 second mouse click is an explicit desktop proxy. | P02 floating/rotation positive; real gesture naturalness remains uncertain. | Implement controller grip grab/enlarge/manipulation with clear return-to-album action; do not mistake desktop second-click as sole original XR action. Controller input is an adaptation, not validated handtracking. |
| T/S tools | Two separate floating **spheres**, T for text and S for stickers, fixed spatial editing zone. | T/S succeeded for all 5 with support; sticker visibility/pre-start preview requested. | Preserve two tangible spheres. Add visible Text/Sticker labels, tool glow, sticker preview; contextual palettes can follow sphere activation. |
| Text content | Create text then direct placement/scale/rotation. IP1 expressly excludes full keyboard and uses preset content sufficient for discovery test. | No actual test evidence requiring full keyboard or specific font styles. | Presets are legitimate documented scope; optional editable text is an enhancement. Do not imply full keyboard was in testedIP1. |
| Sticker content | Create sticker via S then grab/place/scale/rotate. | P03 “sticker should show before starting”. | Preview before activation and choices near S, retaining object manipulation. |
| Direct manipulation | Grab/move/scale/rotate; direct handgestures intended. IP1 desktop drag/scroll/Z-X. | KF2 retain drag/scale/rotation. | Proper XR one-hand translation/rotation + two-hand scaling; report actual tests pending. |
| Trash | Recognisable **left-side bin**, fixed zone. Throw unwanted **media or overlays** left. IP1 implements overlay drag-and-release. | P03 “Red should be deleting”. | Restore a recognisable spatial bin with rim/interior; red proximity/drop-ready feedback, reliable release deletion, recovery. Throw acceptance may extend initial stable release behavior; distinguish scope. |
| Publish | Large tangible red submit control, appears only after editing, strike tosend; IP1 press feedback/photo motion/delayed confirmation. | Red semantic mismatch; Publish tooearly. | Preserve large final physical control but change to blue/mint; reveal aftermeaningful edit. Add confirmation/poke/press where supported, then photo motion + success. |
| Deselect/back | Original pinch-to-minimise/return forcontinued selection. | P04 explicit reverse/deselect request. | Clear Back, deselect, photo return and safe change-photo flow; do not remove original inspect-return loop. |
| Transitions | Selected photo forward then tools appear, browseUI suppressed. | P05 timingintervals feedback; state progression required thought. | Stage photo movement and toolreveal rather than simultaneous change; avoid disruptive camera movement. |
| Contextual hints | Visible yellowhighlight/tool glow, fixedspatial zones. | Clearinstructions (P01) plus highlightinstructions note; everyone prompted. | Brief conspicuous nextaction hints that update perstate; not a wall of always-on instructions. |
| Reset/logging | IP1 repeatable reset/eventlogging. | Missingtime for P02–P05, missing P02delete/publish. | Preserve reset, record per-step time and prompt locations; do not manufacture results. |

## Scope distinctions and limits

- Original target is **room-scale MR with hand tracking**. Current user asks a Quest VR version based on classroom learning. Quest-controller VR is a sensible current adaptation but must be explicitly stated; it does not prove handtracking or passthrough behavior.
- Original includes **videos**; actual IP1 plan identifies only six travel photographs. No video content source located in these documents. Support for a future VideoClip is distinguishable from delivering real supplied video media.
- Original **throw media or overlays** is broader than IP1 tested overlay drag-to-trash. This reconstruction supports both media and overlay deletion, plus a bounded throw toward the bin; this broader behavior still needs an XR trial.
- No evidence requires completing text AND sticker AND deletion before publishing. Actual accepted design response is **reveal Publish after a meaningful edit**. Mandatory all-step gate would be a new implementation/testing decision.
- No evidence that precise 0.35s/0.15s transition durations, fixed five-second Undo, audience settings, haptic thresholds, custom keyboard, or full sticker catalogue are IP1 findings. These may be later design additions, never present them as measured/confirmed requirements.
- The statistics are **4/5 full completion, 0/5 promptfree, 4.8/5 ease,4.1/5naturalness**. T/S success5/5 with facilitator support. P02 Delete/Publish **not recorded**, not failure. Only P01 has time4:13. Real XR comfort/gesture validity remains unknown.

