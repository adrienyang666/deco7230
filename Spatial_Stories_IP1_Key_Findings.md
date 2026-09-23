# Spatial Stories — IP1 Testing Key Findings

**Course:** DECO7230 Digital Prototyping and Extended Reality  
**Prototype:** Interactive Prototype 1 — Unity Horizontal Prototype  
**Project:** Spatial Stories  
**Student:** Zixu Yang  
**Student ID:** 48939559  
**Participants:** 5 (P01–P05)

---

## 1. Testing Outcome Summary

Five participants completed the IP1 usability test using the Unity desktop prototype. The test focused on whether users could understand the spatial Story-creation workflow with minimal facilitator guidance, including photo selection, text/sticker tools, direct manipulation, deletion, and publishing.

| Measure | Result |
|---|---:|
| Participants tested | 5 |
| Full task completion | 4/5 (80%) |
| Partial completion | 1/5 (20%) |
| Prompt-free completion | 0/5 (0%) |
| Participants needing 1 prompt | 3/5 |
| Participants needing 2+ prompts | 2/5 |
| Approx. average ease rating | 4.8/5 |
| Approx. average spatial-workflow naturalness | 4.1/5 |

The prototype therefore achieved the target completion rate, but **did not meet the original prompt-free success criterion**. The main problem was not whether the prototype functioned, but whether all interactions were immediately discoverable without help.

---

# 2. Key Findings

## KF1 — The overall spatial workflow is understandable, but discoverability still needs improvement

### Evidence
- 4 of 5 participants completed the full workflow.
- All 5 participants required at least one facilitator prompt.
- 2 participants required two or more prompts.
- Ease ratings were generally high.

### Interpretation
The concept and end-to-end workflow are understandable once users recognise the available interactions. However, some affordances are not yet self-explanatory enough for first-time users.

### Design Action
- Strengthen contextual visual cues instead of adding more features.
- Make the next available action easier to identify.
- Reduce the need for facilitator instructions in the next prototype.

---

## KF2 — Floating photos and direct manipulation are strong parts of the concept

### Evidence
One participant specifically noted that the **photo floating interaction was intuitive** and that the **rotation feature was useful**. Participants were generally able to use the spatial editing workflow and the naturalness ratings were positive overall.

### Interpretation
Treating photos and editing elements as spatial objects supports the intended MR concept. This interaction model should be retained rather than replaced with a conventional 2D menu-based interface.

### Design Action
- Keep the floating photo interaction.
- Keep drag, scale, and rotation as core direct-manipulation interactions.
- Test these interactions again with real XR input in a later prototype.

---

## KF3 — Delete and Publish need clearer visual semantics

### Evidence
Participant feedback indicated that **red was more naturally associated with deletion**, while the current prototype uses a prominent red Publish action. Another observation noted that **Publish appeared too early** in the editing sequence.

### Interpretation
The visual meaning of destructive and completion actions is not fully aligned with users' expectations. Showing Publish too early may also encourage users to finish before exploring the editing interactions.

### Design Action
- Use stronger red feedback for the Trash/delete state.
- Change Publish to a visually distinct non-destructive colour.
- Delay the appearance of Publish until the user has completed a meaningful edit.
- Keep the final Publish action visually prominent only when the Story is ready.

---

## KF4 — Editing-state affordances need to be clearer

### Evidence
Testing notes suggested that:
- sticker availability should be clearer before users begin editing;
- users would benefit from a clearer deselect/back action;
- some participants required additional thought before progressing through editing.

### Interpretation
Users can perform the editing interactions, but the transition between selection, editing, and finishing still needs clearer state communication.

### Design Action
- Make Sticker availability more visible, for example through a small preview or clearer tool feedback.
- Add an obvious deselect/back interaction.
- Strengthen the visual distinction between Browse Mode and Edit Mode.
- Continue refining transition timing so tools appear in a clear sequence.

---

## KF5 — Instructions are useful, but their visual hierarchy should be improved

### Evidence
One observation noted that the **instructions should be highlighted**, while another participant described the instructions as clear.

### Interpretation
The instruction content itself is useful, but it may not attract enough attention at the correct moment. This suggests a presentation problem rather than a content problem.

### Design Action
- Replace persistent instructions with short contextual prompts.
- Highlight only the control needed at the current stage.
- Keep instructions focused on translating desktop input into the future MR interaction rather than explaining the meaning of every interface element.

---

# 3. Evaluation Against the Original Success Criteria

The original success criterion defined a promising interaction as one that:
- is completed by at least 80% of participants;
- requires no facilitator help;
- has no repeated critical error; and
- receives an average ease/naturalness rating of approximately 4/5 or higher.

### Result

| Criterion | Outcome |
|---|---|
| ≥80% completion | ✅ Met — 4/5 participants fully completed the task |
| Prompt-free completion | ❌ Not met — every participant required at least one prompt |
| No repeated critical failure | ✅ No repeated critical technical failure was recorded |
| Ease ≈ 4/5 or higher | ✅ Met — approximately 4.8/5 |
| Spatial-workflow naturalness ≈ 4/5 or higher | ✅ Met — approximately 4.1/5 |

### Overall Interpretation

**The interaction model is promising but not yet sufficiently discoverable.**

The testing indicates that the broad Spatial Stories concept should be retained. The highest-priority problem for the next iteration is not functionality, but reducing the amount of facilitator guidance required for users to understand what to do next.

---

# 4. Priorities for the Next Iteration

Based on the IP1 test, the next prototype should prioritise:

1. **Improve first-time discoverability** through clearer contextual cues.
2. **Clarify Delete vs Publish semantics**, especially the use of red.
3. **Delay Publish** until meaningful editing has occurred.
4. **Improve Sticker visibility and editing-state feedback.**
5. **Add a clear deselect/back interaction.**
6. **Retain floating photos, rotation, drag and scale interactions.**
7. **Move real gesture naturalness testing to the later Meta Quest/XR prototype.**

---

## Conclusion

IP1 testing supports the core direction of Spatial Stories. Participants generally found the workflow easy and the spatial concept natural, and direct manipulation of floating photos was positively received. However, the fact that all participants needed facilitator prompts shows that several interactions are not yet sufficiently self-explanatory.

The next iteration will therefore focus on **discoverability, clearer visual semantics, and stronger state feedback**, while retaining the interaction patterns that participants already found intuitive.
