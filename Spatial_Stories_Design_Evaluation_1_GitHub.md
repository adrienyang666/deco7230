# Spatial Stories - Interactive Prototype Outcome Evaluation 1

**Course:** DECO7230 Digital Prototyping and Extended Reality  
**Student:** Zixu Yang  
**Student ID:** 48939559  
**In-class test:** 5 participants, 20 August 2026

## 1. Objective and validation metrics

**Testing objective.** Evaluate whether first-time users could understand and complete the spatial Instagram Story workflow with minimal guidance: select a floating photo, enter Edit Mode, use T/S tools, directly manipulate content, delete an unwanted item, and publish.

**Original success criterion.** A core interaction was considered promising when at least 80% of participants completed it without facilitator help, no repeated critical error occurred, and the relevant ease/naturalness rating averaged 4/5 or higher.

| Metric | Result |
|---|---:|
| Full completion | 4/5 (80%) |
| Prompt-free completion | 0/5 (0%) |
| Mean ease rating | 4.8/5 |
| Mean spatial-workflow naturalness | 4.1/5 |

## 2. Results

Four participants fully completed the workflow and one partially completed it. All five required facilitator support: three required one prompt, while two required two or more prompts. T/S use was marked successful for all five participants. Delete and Publish were marked successful for all four participants with recorded values; the P02 Delete/Publish fields were left blank and are treated as missing data rather than failures.

| ID | Outcome | Prompts | T/S | Delete | Publish | Ease | Naturalness | Time |
|---|---|---:|---|---|---|---:|---:|---|
| P01 | Complete | 1 | Yes | Yes | Yes | 5 | 4 | 4:13 |
| P02 | Partial | 2+ | Yes | Not recorded | Not recorded | 5 | 5 | Not recorded |
| P03 | Complete | 1 | Yes | Yes | Yes | 5 | 4 | Not recorded |
| P04 | Complete | 2+ | Yes | Yes | Yes | 5 | 4 | Not recorded |
| P05 | Complete | 1 | Yes | Yes | Yes | 4 | 3.5 | Not recorded |

**Selected participant comments:** “clear instructions”; “rotating feature is very useful, photo floating is intuitive”; “Red should be deleting”; “sticker should show before starting.”

**Data-quality note.** Only P01 had a recorded completion time, so no mean task time is reported. P02 Delete/Publish values were not recorded.

## 3. Analysis / Insights

### A. Core spatial metaphor is promising
High ease (4.8/5) and naturalness (4.1/5), plus the comment that photo floating was intuitive and rotation useful, indicate that treating media as manipulable spatial objects supports the MR concept. The problem is not the core metaphor.

### B. Discoverability is the dominant usability weakness
Completion reached 80%, yet no participant was prompt-free. This gap shows that users can operate the prototype once guided, but several affordances are not sufficiently self-explanatory for first use.

### C. Action semantics and sequencing need stronger hierarchy
“Red should be deleting” suggests a mismatch between colour meaning and the current Publish emphasis. Notes also indicate Publish appeared too early. Destructive and completion actions need clearer semantic separation and timing.

### D. Editing state needs clearer entry, exit and tool visibility
Feedback that the sticker should be visible before starting, together with a request for a reverse/deselect action, suggests that users need stronger cues about available tools and a clearer way to back out of a selection.

### E. Guidance should be contextual rather than permanently explanatory
Instructions were described as clear, but another note said they should be highlighted. The content is useful; the visual hierarchy and timing of guidance need refinement.

## 4. Evaluation of aims

| Aim / assumption | Status | Evidence | Evaluation |
|---|---|---|---|
| Users can complete the end-to-end workflow | Partially validated | 4/5 full completion | The broad sequence works, but the result depended on facilitator prompts. |
| Users can discover interactions without help | **Invalidated** | 0/5 prompt-free; 2/5 needed 2+ prompts | Discoverability did not meet the defined success criterion. |
| Spatial direct manipulation feels appropriate | Validated for concept stage | Ease 4.8/5; naturalness 4.1/5; positive floating/rotation feedback | Keep the spatial object model and test it with real XR input later. |
| Delete / Publish semantics are self-evident | Partially validated | Delete/Publish succeeded when recorded, but colour/timing feedback was criticised | Functionality works; meaning and timing need refinement. |
| Real gesture naturalness is established | Uncertain | Desktop mouse was a proxy for MR input | Requires Meta Quest / hand-tracking testing in a later prototype. |

**Overall evaluation:** the interaction model is promising, but it is not yet sufficiently discoverable.

## 5. Concept iteration

| Evidence / insight | Design response | Why this change |
|---|---|---|
| All 5 participants needed prompts; instructions should be highlighted | Replace persistent guidance with short stage-based contextual prompts that highlight only the next required desktop/MR action. | Reduces facilitator dependence without explaining every interface meaning in advance. |
| “Red should be deleting”; Publish appeared too early | Use red feedback for Trash/drop state; give Publish a non-destructive treatment and reveal it only after a meaningful edit. | Aligns colour semantics with expectation and makes workflow progression clearer. |
| “Sticker should show before starting” | Add a small sticker preview / clearer S-tool affordance before selection. | Makes tool purpose visible without requiring a facilitator prompt. |
| Request for reverse/deselect action | Add click-empty-space deselect and a clear Back control between Browse and Edit states. | Improves user control and recovery from wrong selections. |
| Timing/flow note; editing state still required thought | Increase separation between photo-forward movement and tool reveal; use a clearer staged transition. | Makes Browse -> Edit state change easier to perceive. |
| Floating photo intuitive; rotation useful | Retain floating media, drag, scale and rotation unchanged for the next test. | Affirms successful design decisions instead of redesigning validated interactions. |

**Revised sequence:**  
`BROWSE (contextual cue) -> SELECT -> EDIT (T/S visible) -> MANIPULATE -> DELETE (red Trash feedback) -> PUBLISH (revealed after edit)`

## 6. Reflection and next steps

**Method strengths.** The task-based test, standardised 10-second help rule, participant ratings and short observation notes produced both behavioural and attitudinal evidence. The prototype was stable enough to expose interaction problems rather than technical failures.

**Method limitations.** The sample was small and convenience-based; the desktop prototype only simulated MR input; completion time was recorded for only one participant; and two P02 interaction fields were left blank. Facilitator prompts may also have influenced later ratings.

**Next testing plan.** For the next round, use a stricter per-step data sheet, record time for every participant, record exactly where each prompt occurs, and preserve the prompt-free completion metric. After the discoverability changes are implemented, move gesture naturalness, spatial reach, depth and embodied interaction to a Meta Quest / XR test rather than inferring them from mouse input.

**Design decision:** retain the validated spatial metaphor; iterate the cues, semantics and state transitions that prevented independent first-use performance.

---

## Appendix A - Raw participant data

Raw participant data is reproduced in the PDF/DOCX submission. Blank fields are reported as “not recorded” rather than recoded as failure.
