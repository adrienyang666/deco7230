# Spatial Stories IP2b Testing Plan

Zixu Yang | 48939559 | DECO7230

Protocol prepared 14 October 2026 and updated 21 October 2026. English edition organised 25 October 2026 from the existing project plan. This document describes the intended procedure; it does not establish that every step was carried out.

The supplied session records leave the test date, build and test mode unfilled. The current repository handoff lists version 1.3.4 / Android code 8, but this is not evidence of the build used in those sessions. Record actual session details from contemporaneous evidence rather than filling these gaps from the plan or repository version.

## 1. Purpose

IP2a supported browsing a floating photo wall and adding text or shape stickers. IP2b tests a new task: take a prepared subject from a source photo, place it onto another background photo, arrange its overlap with words, and check the resulting flat Story.

The exposed PHOTO CUTOUTS entry, short first-use demonstration, transparent placement preview, target outline and layer feedback are design ideas to evaluate. The prototype offers two prepared subjects, Surfer and Eiffel. Their transparent images are AI-assisted reconstructions, not live segmentation of arbitrary photos. Publishing is a local simulation.

## 2. Questions and evidence

| Question | What to observe and record |
|---|---|
| Can people discover and try the new entry? | First intentional reach, grip attempt or relevant comment; time from entry appearance; route used; demonstration exposure; facilitator help. Looking alone does not confirm discovery. |
| Can people predict where the subject will land? | Deliberate releases, invalid returns, corrections, hesitation, and the participant's explanation of the placement cue. Note any visible difference between the preview and final position. |
| Can people control overlap with words? | A deliberate layer change, visible overlap in the requested order, and a correct explanation of which element is in front. Button presses alone do not establish understanding. |
| Does spatial manipulation suit this task? | Reasons for the layout, perceived naturalness, reach and reading difficulty, fatigue or discomfort, and suggested changes. |

Core success means acquiring a subject, placing it on a background different from its source photo, adding words, producing and explaining the requested overlap, and checking the flat preview. Record simulated publishing separately. Do not score aesthetic quality or treat publishing alone as proof of core success.

## 3. Research summary

Spatial Stories uses picking up, moving and arranging content as familiar actions, informed by Jacob et al.'s (2008) reality-based interaction framework, while treating their suitability for this task as a question to test. Hartson's (2003) distinction between cognitive, physical, sensory and functional affordances supports checking whether the exposed cutout suggests an action, is usable, and communicates its result. Bowman and Hodges (1997) motivate observing subject acquisition, positioning and release separately, rather than relying only on overall completion. Placement previews and layer feedback are this prototype's applications of these concerns, not features validated by those papers. Gabbard et al. (1999) support an iterative, user-centred evaluation combining task outcomes, errors, assistance and participant explanations. Saredakis et al. (2020) identify factors associated with headset sickness, supporting a short session, discomfort notes and the option to stop. Together, these sources inform the design and testing of discovery, placement and overlap; they do not establish that this prototype is more natural or effective than a phone editor.

<!-- pagebreak -->

## 4. Participants and preparation

Aim for at least five classmates or peers and five minutes per person. Record anonymous IDs, actual attendance, early stops, VR experience, Story/collage familiarity and previous prototype exposure. Analyse first-use discovery only for people new to this feature.

Before participant sessions:

1. Use Unity 6000.3.20f1. Check compilation and EditMode tests; run the intended build on Quest. Record technical checks separately.
2. Check visibility in both eyes, text, reach, glow, grip, invalid-drop return, layer controls, preview, reset and tracking recovery. Check that rewearing the headset retains the composition.
3. Pilot the wording and timing. Record pilot problems, help and any subsequent build/procedure changes separately from formal sessions.
4. Plan for seated use. Keep Enable Collage, Enable Cutout Discovery, Enable Placement Feedback and Enable Gallery Glow consistent. Record actual version, switches and posture.
5. Prepare sheets, notes and a stopwatch; link participant/session IDs. Obtain recording permission. RESET between users to clear the composition and reset the demonstration; sit comfortably and look ahead before BEGIN.

Participants may pause or stop. Separate tracking/build faults from interaction mistakes. Use Quest or an approved XR device; label simulator runs separately. Setup and cleanup may occur outside the five-minute interaction window.

## 5. Five-minute procedure

Use the same simple wording across participants. Do not point out the cutout entry before the exploration period.

| Approximate time | Facilitator instruction | Record |
|---|---|---|
| 0:00-0:30 | “Trigger selects; grip holds. Two grips resize. Choose a background photo.” | Basic controls, selection and entry into editing. |
| 0:30-0:50 | “Explore the tools. Tell me what you think you can do.” | Discovery attempt/time, route, demo exposure and help. |
| 0:50-2:40 | “Make a travel Story. Put a subject from another photo onto your background and add a short phrase. Choose your layout and say what you notice.” | Acquisition, placement and text; releases, corrections, hesitation and help. |
| 2:40-3:30 | “Make the subject cover part of the words. Explain which is in front.” | Overlap, adjustment, explanation and help. If already achieved, reverse it, then restore the preferred order. |
| 3:30-4:00 | “Check the final preview. Keep it or go back to change it, then finish.” | Preview check, mismatch, revision and simulated publishing. |
| 4:00-4:25 | If core complete: “Remove one element, then restore it.” | **Optional** delete/undo. Otherwise continue core; code recovery N. |
| 4:25-5:00 | Ask the short questions below. | Ratings, reasons, confusion, finish time and stops. |

Start exploration when the entry appears; time appearance and first attempt on one clock. Do not name layer buttons or explain when to release unless help is needed.

<!-- pagebreak -->

### Help policy

After roughly ten seconds without progress, or a request, ask: “What are you trying to do?” If still blocked, give a short hint; demonstrate only if necessary. Record time and exact wording; code that subtask A. Ten seconds is a consistency rule, not a validated threshold.

Independent completion allows product guidance but no extra facilitator help beyond standard instructions. Record built-in demonstration exposure separately.

### End questions

- “How natural did moving the content feel?” Rate 1 = very unnatural, 3 = neither, 5 = very natural; ask why.
- “How comfortable was it?” Rate 1 = very uncomfortable, 3 = neither, 5 = very comfortable. Note reaching, reading, fatigue or dizziness if mentioned.
- “What told you where to let go?”
- “What would you change?”

Label quotes/paraphrases and unasked questions. These single-item ratings are study-specific, not SUS or SSQ scores.

## 6. Recording rules

| Code | Meaning |
|---|---|
| I | Completed without additional facilitator help; product guidance may have been seen. |
| A | Completed after additional facilitator help. |
| F | Attempted but not completed within the available time. |
| N | Not attempted, omitted or interrupted; record the reason. |

Code each subtask separately. Scaling without unintended rotation and delete/undo are optional; record N if omitted. They do not determine core success. Keep publishing separate. Blank means missing data, not F or N.

- **Discovery:** code Y/N/uncertain with an action or comment. Time the first intentional attempt from entry appearance; an unsuccessful grip counts as an attempt.
- **First successful acquisition:** `cutout_entry_used` minus `cutout_discovery_shown` measures time to successful grab, not first attempt.
- **Placement attempt:** a deliberate release onto a photo. Invalid returns count as errors; tracking cancellations and deliberate bin drops do not. Separate first-cutout attempts from text editing where possible.
- **Overlap:** require visible overlap, requested order and a correct explanation.
- **Timing:** record actual duration, including overruns. Leave unknown times blank, explain gaps and never substitute zero.

## 7. Logs and interpretation

Retain logs from `Application.persistentDataPath/SpatialStories_IP2b/` with participant/session IDs. `prototype_config` records version and switches after BEGIN. Discovery, lift/place, invalid-drop, layer and preview events can support observer notes.

Logs cannot establish what someone saw, understood or intended. Record technical interruptions, help and explanations; do not infer gaze from controller events.

Report denominators, I/A/F/N counts, missing data, recorded times and comments by record ID. Link proposals to evidence. Without a controlled comparison, do not claim preview effects or superiority. Post results to GitHub; submit the plan and research paragraph to Blackboard.

<!-- pagebreak -->

## 8. Five lecture principles

The existing project plan identifies these exact terms from **Week6 _1.pptx, slide 21**, “Five Principles to consider while designing ….”:

| Principle | Planned observation |
|---|---|
| Text Readability | Whether instructions, labels and text can be read at the working distance; requests to move or enlarge them. |
| Ergonomics | Seated reach, arm effort, posture, fatigue and comfort while selecting and arranging content. |
| Brightness Changes | Whether glow, highlights or transitions cause visual discomfort or obscure content. |
| Avoiding simulator sickness | Reported dizziness, nausea or discomfort, interruptions and early stopping. |
| Button Placement | Whether controls are visible, reachable and easy to select without unintended actions. |

These are observation prompts, not completed judgements. In the design review, link each judgement to actual evidence and participant/session IDs. Keep technical-run, pilot and participant evidence distinct. Explain any N/A judgement; do not infer that a principle was satisfied merely because no comment was recorded.

## 9. References

Bowman, D. A., & Hodges, L. F. (1997). An evaluation of techniques for grabbing and manipulating remote objects in immersive virtual environments. *Proceedings of the 1997 Symposium on Interactive 3D Graphics*. https://doi.org/10.1145/253284.253301

Gabbard, J. L., Hix, D., & Swan, J. E., II. (1999). User-centered design and evaluation of virtual environments. *IEEE Computer Graphics and Applications, 19*(6), 51-59. https://doi.org/10.1109/38.799740

Hartson, R. (2003). Cognitive, physical, sensory, and functional affordances in interaction design. *Behaviour & Information Technology, 22*(5), 315-338. https://doi.org/10.1080/01449290310001592587

Jacob, R. J. K., Girouard, A., Hirshfield, L. M., Horn, M. S., Shaer, O., Solovey, E. T., & Zigelbaum, J. (2008). Reality-based interaction: A framework for post-WIMP interfaces. *Proceedings of CHI 2008*, 201-210. https://doi.org/10.1145/1357054.1357089

Saredakis, D., Szpak, A., Birckhead, B., Keage, H. A. D., Rizzo, A., & Loetscher, T. (2020). Factors associated with virtual reality sickness in head-mounted displays: A systematic review and meta-analysis. *Frontiers in Human Neuroscience, 14*, Article 96. https://doi.org/10.3389/fnhum.2020.00096

## 10. AI assistance

ChatGPT/Codex assisted with prototype implementation, reference lookup and preparation of the testing documents. This English edition condenses the existing plan. Asset disclosures and generation prompts remain in `IP2b_IMPLEMENTATION.md` and `IP2b_ASSET_PROMPTS.json`. Participant responses and outcomes must come from the recorded sessions; none are generated by this plan.
