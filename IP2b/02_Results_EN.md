# Spatial Stories: IP2b Results

**Zixu Yang | 48939559 | DECO7230**  

English evidence summary prepared 25 October 2026. Actual session dates were not entered on the supplied sheets.

## What the records show

Three record pairs, labelled P01, P02 and P05, contain task outcomes. One records independent core completion; two record completion with extra help. The assisted steps were placement on another photo (P05) and setting the requested overlap (P01).

The new IP2b task extends the floating photo wall and T/S tools: take a prepared subject from a source photo, place it on another background, add text, control their overlap and check the flat result. The records support the feasibility of this workflow; placement and layer control still need clearer support.

P03 and P04 contain limited header markings but no task results or feedback. The analysis therefore uses **three outcome-bearing records**, not five completed tests. The preprinted slot labels identify source pages; they do not verify five distinct participants or formal-session status.

## Task outcomes

| Task | P01 | P02 | P05 | Counts: I / A / F / N |
|---|---|---|---|---|
| Lift subject | I | I | I | 3 / 0 / 0 / 0 |
| Place on a different photo | I | I | A | 2 / 1 / 0 / 0 |
| Add text | I | I | I | 3 / 0 / 0 / 0 |
| Set requested overlap | A | I | I | 2 / 1 / 0 / 0 |
| Explain overlap correctly | I | I | I | 3 / 0 / 0 / 0 |
| Scale without rotation error (optional) | I | F | I | 2 / 0 / 1 / 0 |
| Delete and Undo (optional) | I | N | I | 2 / 0 / 0 / 1 |
| Check flat preview | I | I | I | 3 / 0 / 0 / 0 |
| Simulated Publish | I | I | I | 3 / 0 / 0 / 0 |
| Overall core outcome | A | I | A | 1 / 2 / 0 / 0 |

I = completed without extra facilitator help (product demonstration allowed). A = completed with extra help. F = attempted but unfinished. N = not attempted or interrupted. Empty CSV cells mean unrecorded, not N or zero. Optional tasks are excluded from the core outcome. Overall outcomes are transcribed from the circles on the sheets.

## Ratings

| Record | Naturalness, 1-5 | Comfort, 1-5 |
|---|---|---|
| P01 | 2 | 3 |
| P02 | 4 | 5 |
| P05 | 4 | Unclear handwritten mark |

These are study-specific single-item ratings. Naturalness ranges from 2 to 4. Only two comfort values are readable; P05 is excluded from numeric comfort summaries. The rating reasons are partly illegible, so the scores should not be given an invented explanation.

<!-- pagebreak -->

## Feedback and proposed changes

The following are English translations or careful paraphrases of the handwritten notes. The source does not mark them as verbatim participant quotations. Uncertain wording is retained in the feedback CSV and transcription notes.

| Evidence | Interpretation | Proposed change and next check |
|---|---|---|
| P01: dragging felt delayed; interaction did not match intuition. | Direct manipulation was not consistently responsive or understandable. The cause of delay is unknown. | Check controller-to-object response on Quest; observe a repeated grab-drag-release task and separate input, tracking and rendering issues. |
| P01: did not know the release cue. P05: placement needed help. | Placement feedback may not be sufficiently noticeable or understood. | Strengthen the target and release cue; ask the next tester to identify when and where they would release, before coaching. |
| P01: overlap required help; note says layer order was not changed correctly. P05: layer bar could be higher. | Layer controls need clearer placement and feedback. | Trial a higher, reachable layer bar with a visible selected layer and clear front/behind feedback; observe overlap adjustment and ask for an explanation. |
| P02: Undo button was difficult to see; Delete and Undo marked N. | Undo visibility needs attention; the record does not establish why the task was skipped. | Check size, contrast, position and display time on the headset; test deletion and recovery as a separate task. |
| P02: scaling marked F; grabbed item was not centred. | Manipulation and object positioning need a focused follow-up. The notes do not establish a shared cause. | Observe two-handed scaling and inspect the grab offset with different starting positions. |
| P02: requested a record of already posted content. P05: suggested progress in a corner, but the action is unclear. | History is a clear extension request; the progress request needs clarification. | Consider a local story-history view after core interaction fixes; clarify what progress should be shown before implementing it. |

The immediate priorities are placement cues, layer controls and responsive manipulation, followed by visible Undo and reliable scaling. A history view is a secondary extension. These are proposed responses to the evidence, not completed fixes or validated improvements.

## What was and was not validated

The recorded workflow can reach the flat preview and simulated publication. Independent completion is only partly supported: the core outcome is I in one record and A in two. Correct overlap explanations in all three records are encouraging, but do not establish independent mastery of layer adjustment.

Discovery time, completion time, product-demo exposure, assistance wording and help/error counts were not entered. The records cannot establish how quickly the new entry was discovered, whether the preview reduced errors, or whether IP2b outperformed IP2a. There was no recorded comparison condition. Publishing successfully is not treated as proof that every editing step succeeded independently.

The P01 note about not deleting unwanted content differs from its Delete and Undo = I circle. Both are preserved. It is not possible to reconstruct their order or determine whether they refer to the same attempt.

<!-- pagebreak -->

## Review against the five lecture principles

This is a retrospective review of available evidence. The original five-principle worksheet (page 11) was blank. The principle names follow the project's existing lecture cross-reference: Week6 _1.pptx, slide 21. None is marked fully passed solely from these records.

| Principle | Available evidence and judgement | Next validation |
|---|---|---|
| Text Readability | P02 could not see the Undo button clearly. Text, size, contrast and location are not isolated. Readability is not yet established. | Check Undo, layer labels and placement messages in the headset at the actual viewing distance. |
| Ergonomics | P05 asks for a higher layer bar; P02 reports an off-centre grabbed item. Comfort is 3 and 5 in the two readable records. Reach and fatigue were not measured. | Test seated reach, grab offsets and workspace height with several users; ask about arm/neck effort. |
| Brightness Changes | No reliable brightness-change observations are recorded. The unclear P01 comfort reason is not treated as evidence of flicker. | Observe selection glow, placement confirmation and scene transitions in the headset; record abrupt brightness changes. |
| Avoiding simulator sickness | General comfort ratings are available, but no symptom-specific assessment is recorded. Comfort is not a validated sickness measure. | Ask about symptoms during and after use and record any stop; check movement, tracking recovery and frame stability. |
| Button Placement | Undo visibility and the request for a higher layer bar identify placement concerns. Layer adjustment required help in P01. | Check whether users can locate and reach Undo and layer controls without coaching or awkward posture. |

All five principles remain relevant to this prototype; none is classified as not applicable. Missing evidence is distinguished from a failed design requirement.

## Evidence limits and version context

The supplied PDF contains five reserved two-page slots and a blank principles sheet. Actual session ID, date, build, trial type, posture, feature settings, timing and event/help logs are unfilled. The planned date (22 October 2026), version 1.3.3/code 7, seated setup and enabled switches printed on the form are plans, not verified session metadata. Total attendance and the division between pilot, technical and formal tests cannot be established from this file.

The repository handoff separately documents version 1.3.4/code 8, removal of decorative posts obscuring the T/S tools, a successful Android build and the user's visual confirmation. Its earlier 1.3.3 entry reports 37/37 EditMode tests and a user check of tracking recovery. These are repository-documented technical checks, not new participant results and not tests rerun for this report. The actual build used for the handwritten records remains unknown.

## Source and reproducibility

The original annotated PDF is preserved unchanged in `source/Original_Annotated_Record_Sheets.pdf`. Task/rating transcription is in `03_Task_Records_EN.csv`; field-level English feedback and uncertainty notes are in `04_Feedback_EN.csv`; mapping and unresolved entries are in `05_Transcription_Notes_EN.md`. Source pages 1-2, 3-4 and 9-10 support the reported task counts. The two unrecorded slots remain in the data table.

AI assistance was used to transcribe and translate the supplied notes, organise the evidence and format this package. Handwriting was checked visually; unresolved entries remain marked unclear. No participant responses, observations or missing measurements were generated to fill gaps.
