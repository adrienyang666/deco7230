# Spatial Stories - IP2b 1.3.3 testing plan

Zixu Yang | 48939559 | DECO7230

Prepared 14 October 2026; updated 21 October 2026. **Plan only: no participant results are included.** Lecture principle names have been checked; Quest runs, the pilot and formal sessions remain pending.

## 1. Purpose and change from IP2a

IP2a supported browsing a floating album and adding text or shape stickers. IP2b tests a different authoring task: taking a prepared subject from a source photograph, combining it with another photograph, and controlling how it overlaps words. Users manipulate content in space and then check the resulting flat Story. The NEW entry and first-use demonstration support discovery of this task.

Version 1.3 adds a transparent placement prediction, a target outline, a brief placement confirmation, and clearer layer feedback. The prediction uses the same target, fit and layer-depth calculation as the final drop. These changes address **design hypotheses**, not newly observed participant problems.

The prototype runs on Meta Quest with controllers. Only two prepared subjects are available. Their alpha images are AI-assisted reconstructions, not runtime segmentation of arbitrary photographs. Publishing is a local simulation. These boundaries do not prevent testing the intended interaction, but limit claims about a complete photo-editing product.

## 2. Questions and observable evidence

| Question | What to observe | Evidence to retain |
|---|---|---|
| Can someone discover and try the new entry? | Recognition followed by an intentional reach, grip attempt or comment during initial exploration. | First attempt time; observed behaviour; direct/menu path; demonstration exposure; facilitator help. |
| Can someone predict where the subject will land? | First release onto a different photo; hesitation; failed releases; adjustments after placement; whether the user reports seeing the prediction. | Placement attempts/errors; target changes in logs; participant explanation; confirmed preview/drop mismatch if present. |
| Can someone control the overlap with words? | Deliberately put the word behind the subject, then explain which element is in front. | Outcome and help level; button actions; explanation; final composition. |
| Does spatial manipulation suit the task? | Participant's preferred layout, reasons for using the cutout, difficulty reaching/reading, fatigue or discomfort. | Exact short comments, naturalness and comfort ratings, optional observation of scaling. |

Do not treat selecting Publish as evidence that the collage or occlusion tasks were completed. Do not infer understanding from button presses alone.

## 3. Research summary - one paragraph

Spatial Stories uses familiar actions such as picking up, moving and arranging content, drawing on Jacob et al.'s (2008) reality-based interaction framework while treating familiarity as a design hypothesis to test. Hartson (2003) distinguishes cognitive, physical, sensory and functional affordances; this motivates evaluating whether the exposed cutout communicates an action, whether users can perform it, and whether they understand the visible result. Bowman and Hodges (1997) examine selection and manipulation techniques, supporting separate observation of acquiring a subject, positioning it and releasing it instead of relying only on total completion. The transparent drop prediction and overlap feedback are this prototype's design applications of those concerns, not features proven by those papers. Gabbard et al. (1999) describe an iterative, user-centred evaluation process for virtual environments; the present formative study therefore combines task outcomes, errors, assistance and short explanations to identify actionable changes. Saredakis et al. (2020) identify content and exposure factors associated with headset sickness, motivating recording discomfort and allowing participants to stop. Together, these sources inform a short Quest study of discovery, placement and occlusion; they do not establish that this prototype is more natural or effective than a phone interface.

## 4. Participants, setup and consistency

Aim for at least five classmates or peers, with about five minutes of interaction each, following the brief. Record the actual number and any early stops; do not fill absent trials. Collect anonymous IDs P01, P02 and so on, prior VR experience, Story/collage familiarity, and any earlier Spatial Stories version they have seen. Analyse first-use discovery only for people who have not seen the cutout feature before. These peers provide formative usability evidence, not a representative sample of all social-media users.

Before participants arrive:

1. Open the existing project with Unity 6000.3.20f1, clear compile errors, run the supplied 20 EditMode test cases, build and run on Quest.
2. Complete one end-to-end technical run on Quest using the device checklist in IP2b_START_HERE.md, including two-eye visibility, direct grip, off-target return, layer controls, preview and reset. Record the build, actual task duration and technical issues separately from participant results.
3. Then run one small pilot to check whether the core task can be completed within about five minutes. Record the actual duration, errors, help and feedback; label the session as a pilot and keep it separate from formal results. If it changes the build or instructions, restart formal recording with the revised version.
4. Keep Enable Collage, Enable Cutout Discovery, Enable Placement Feedback and Enable Gallery Glow on for every formal trial. Version 1.3.3 retains seated workspace scaling, glow and Trigger selection for cutouts, and adds one-shot workspace recovery after a pause or tracking recovery; Grip still lifts and moves them. Before the pilot, test removing and replacing the headset while keeping the composition, wait until tracking is stable, and record whether the layout recovers without losing content. Run the test seated. At each RESET, let the participant sit comfortably and look ahead before pressing BEGIN; this captures their actual eye height without moving the tracked camera or fixed floor. Record the actual posture in the notes. Check the glow brightness on Quest before the pilot. Log the actual version and flags. Changes between participants must be recorded.
5. Confirm a comfortable working position and reachable tools. Reset for each participant so the demonstration is shown once again. Check that the prior collage has cleared.
6. Prepare IP2b_OBSERVATIONS.csv, IP2b_EVENT_NOTES.csv and a stopwatch. Record the in-app session ID with the participant ID. Observe the headset view through casting if available; obtain permission before recording a person or their view.

The participant can pause or stop at any time. Record a tracking/build problem separately from a user action error. Headset setup, permission and cleanup can occur outside the five-minute interaction window.

## 5. Facilitator script and schedule

Use simple English or the Chinese equivalent consistently. Do not point to the cutout entry before the exploration period.

| Time | Say / do | Record |
|---|---|---|
| 0:00-0:30 | “Use the trigger to select and the grip to hold. Two grips resize the same object. Choose a background photo.” / “Trigger 用来选择，Grip 用来抓取；两只手抓同一个物体可以缩放。请选一张背景照片。” | Basic-control difficulty; background choice; first editor entry. |
| 0:30-0:50 | “Explore the tools for a moment. Tell me what you think you can do.” / “先自由看看这些工具，告诉我你觉得可以做什么。” | First intentional attempt and time; entry used; demonstration completed/interrupted; no inference from looking alone. |
| 0:50-2:40 | “Make a travel Story you would want to share. Put a subject from another photo onto your background, and add a short phrase. Choose your own layout. As you work, quietly say what you are thinking, noticing or finding confusing; you do not need to explain every action.” / “做一张你愿意分享的旅行 Story，把另一张照片里的主体放到背景上，再加一句短文字。布局由你决定。操作时请轻声说出你正在想什么、注意到什么，或哪里让你困惑，不必解释每个动作。” | Acquisition, cross-photo placement and text outcomes; invalid drops; hesitation; help. |
| 2:40-3:30 | “Make the subject cover part of the words, so the words sit behind it. Explain which is in front.” / “让主体挡住一部分文字，使文字在主体后面，再告诉我哪个在前面。” | Deliberate overlap change, understanding and help. If this is already achieved, ask them to reverse it and restore their preferred order. |
| 3:30-4:00 | “Check the final preview. Keep it or go back to change it, then finish.” / “检查最终预览，可以保留或返回修改，然后完成。” | Whether the participant checks overlap and notices inconsistencies; preview and simulated publish outcomes. |
| 4:00-4:25 | If the main task is complete: “Remove one element, then restore it.” / “删除一个元素，再恢复它。” | Optional recovery check. Otherwise allow time to finish the core task and mark recovery N. |
| 4:25-5:00 | Ask the short questions below; finish on time or record the actual longer duration. | Ratings, reason, most confusing moment, stopping reason if relevant. |

Task wording describes the desired result. Do not name FORWARD, BACKWARD, NEXT LAYER or tell the participant when to release unless assistance becomes necessary.

The schedule is approximate: start the twenty-second exploration when the entry actually appears, even if that is earlier or later than 0:30. Keep the entry timestamp and the attempt timestamp on the same session clock; do not reset the discovery timer after the participant has already seen or tried it.

### Consistent help policy

After roughly ten seconds without progress, or a direct request, ask “What are you trying to do?” If still blocked, offer one short goal-related hint; demonstrate a specific control only if needed. Log the time and exact help wording, and mark that subtask assisted. These timings are a facilitation rule for consistency, not a research-established threshold.

The built-in two-second demonstration is product guidance. Independent means no extra facilitator help beyond the standard task and controls; it does not mean no guidance. Record demo exposure separately.

### Short end questions

- “How natural did moving the content feel?” 1 = very unnatural, 3 = neither, 5 = very natural. Ask why.
- “How comfortable was it?” 1 = very uncomfortable, 3 = neither, 5 = very comfortable. Note reaching, reading, fatigue or dizziness if mentioned.
- “What told you where to let go?”
- “What would you change?”

Record a short quote or clearly labelled paraphrase for each open question. Keep the questions within the remaining time and record any that were not asked.

These are study-specific single-item ratings, not SUS, SSQ or validated scale scores.

## 6. Coding outcomes and timing

For each subtask use:

- I: completed without additional facilitator help; product guidance may have been seen.
- A: completed after additional facilitator help.
- F: attempted but not completed in the available time.
- N: not attempted, omitted or interrupted; include the reason.

Record visible discovery as Y/N/uncertain, supported by an action or comment. Time first intentional attempt manually from the entry appearing. An unsuccessful grip attempt can count as an attempt. The automatic cutout_entry_used event records a **successful acquisition**, not the first attempt; subtract cutout_discovery_shown time from that event only for seconds_to_first_successful_grab. Leave an unobserved time blank and explain it rather than entering zero.

A placement attempt is a deliberate release intended to attach an element to a photograph. An invalid return is an error; a tracking cancellation or deliberate bin drop is not. Count first-cutout placement attempts separately if distinguishing the new task from later text editing. If an object appears to jump away from the shown prediction, note the object, target, time and whether the hand moved during release.

Occlusion success requires visible overlap, the requested order, and a correct explanation. Overall core success requires taking a subject, placing it on a different source photo's background, adding words, producing the requested overlap, and checking the flat result. Record publishing separately. Aesthetic quality is not scored.

## 7. Logs, notes and interpretation

Events are saved under Application.persistentDataPath/SpatialStories_IP2b/. Copy them off the device after the session and keep their session IDs with the observation sheet. The BEGIN event is followed by prototype_config, including the app version and feedback flags. Relevant events include cutout_discovery_shown, cutout_entry_used, lift_photo_subject, placement_preview_target, placement_preview_lost, place_photo_subject, invalid_subject_drop, invalid_drop_return, layer_forward, layer_backward and flat_preview_ready.

Log events support observation; they do not establish what participants saw or understood. Technical interruptions, deliberate corrections, facilitator instructions and participant explanations need observer notes. No attempt is made to infer gaze from Quest 3S controller events.

After testing, report:

1. Actual sample and build(s), number attempted for each task, and I/A/F/N counts.
2. Median/range of observed times where useful, always with the denominator and missing values.
3. A few repeated problems or successes with participant IDs, observed actions and short comments.
4. Specific next changes justified by that evidence. Do not describe a proposed improvement as a measured result.

This is a small formative study of one configuration. It cannot isolate the causal effect of the new preview or establish superiority over IP2a or mobile editing. A separate comparison would need matched tasks, controlled settings and attention to learning order; first-use discovery should only be analysed on first exposure.

At the end of the testing session, post actual testing results to GitHub and submit the testing plan, including the research summary, to Blackboard as required by the brief. Retain anonymised observations and relevant session logs, and base conclusions on the real sessions. This documentation update does not upload, commit or submit anything.

## 8. Review against the five lecture principles

The exact terms below are from **Week6 _1.pptx, slide 21**, “Five Principles to consider while designing ….”:

1. Text Readability
2. Ergonomics
3. Brightness Changes
4. Avoiding simulator sickness
5. Button Placement

IP2b_DESIGN_REVIEW.csv contains these terms, the lecture reference and planned Quest observation points. The observation points are applications to this prototype, not additional lecture requirements or measured results. Leave quest_evidence, meets_partial_not_met_or_NA and reason_or_next_change blank until actual testing. Then record the observed behaviour, relevant participant/session IDs and a supported judgement for each principle; explain any N/A judgement. Record technical-run and pilot evidence separately from formal participant evidence.

## 9. References

Bowman, D. A., & Hodges, L. F. (1997). An evaluation of techniques for grabbing and manipulating remote objects in immersive virtual environments. Proceedings of the 1997 Symposium on Interactive 3D Graphics. https://doi.org/10.1145/253284.253301

Gabbard, J. L., Hix, D., & Swan, J. E., II. (1999). User-centered design and evaluation of virtual environments. IEEE Computer Graphics and Applications, 19(6), 51-59. https://doi.org/10.1109/38.799740

Hartson, H. R. (2003). Cognitive, physical, sensory, and functional affordances in interaction design. Behaviour & Information Technology, 22(5), 315-338. https://doi.org/10.1080/01449290310001592587

Jacob, R. J. K., Girouard, A., Hirshfield, L. M., Horn, M. S., Shaer, O., Solovey, E. T., & Zigelbaum, J. (2008). Reality-based interaction: A framework for post-WIMP interfaces. Proceedings of CHI 2008, 201-210. https://doi.org/10.1145/1357054.1357089

Saredakis, D., Szpak, A., Birckhead, B., Keage, H. A. D., Rizzo, A., & Loetscher, T. (2020). Factors associated with virtual reality sickness in head-mounted displays: A systematic review and meta-analysis. Frontiers in Human Neuroscience, 14, Article 96. https://doi.org/10.3389/fnhum.2020.00096

Reading links checked 14 October 2026:

- Bowman and Hodges author project summary: https://people.cs.vt.edu/~bowman/grab.html
- Gabbard et al. author-hosted paper: https://ed-swan.github.io/publications/papers/1999_Gabbard-Hix-Swan_User-Centered_IEEE-CGA.pdf
- Hartson publisher abstract and bibliographic record: https://www.tandfonline.com/doi/abs/10.1080/01449290310001592587
- Jacob et al. author-hosted paper: https://www.cs.tufts.edu/~jacob/papers/chi08.pdf
- Saredakis et al. full article: https://www.frontiersin.org/journals/human-neuroscience/articles/10.3389/fnhum.2020.00096/full

The sources motivate design choices and evaluation, not claimed usability findings for Spatial Stories. The Bowman author summary and Hartson publisher abstract were used alongside verified publication records; full-paper access was not available through every publisher page.

## 10. AI assistance and review status

ChatGPT/Codex assisted with runtime code, shader, tests, this plan, reference lookup and documentation. The two prepared subject assets retain the disclosure and exact generation prompts in IP2b_IMPLEMENTATION.md and IP2b_ASSET_PROMPTS.json. No participant responses, observations or test outcomes were generated. The exact lecture principle names were verified against Week6 _1.pptx, slide 21 on 15 October 2026. The author must review the plan, perform Unity/Quest checks and conduct the sessions before reporting results or claiming readiness.
