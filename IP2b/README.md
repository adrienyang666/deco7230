# Spatial Stories - IP2b submission materials

**Zixu Yang | 48939559 | DECO7230**  
English package prepared 25 October 2026 from the supplied test records and the current repository.

**Start with the [English results report (PDF)](02_Results_EN.pdf).** Three record pairs contain task outcomes: one independent core completion and two assisted completions. Two other reserved slots have no outcome data. Missing measurements and unclear handwriting are retained explicitly.

## Files

| File | Purpose |
|---|---|
| [Testing plan (PDF)](01_Testing_Plan_EN.pdf) / [Markdown](01_Testing_Plan_EN.md) | English task, five-minute procedure, measures, help rules, research paragraph and five academic references. |
| [Results report (PDF)](02_Results_EN.pdf) / [Markdown](02_Results_EN.md) | Recorded outcomes, ratings, feedback, proposed improvements, evidence limits and a retrospective review of the five lecture principles. |
| [Task records (CSV)](03_Task_Records_EN.csv) | All five source slots; three with task outcomes. Blank and unclear fields are not filled with invented values. |
| [English feedback (CSV)](04_Feedback_EN.csv) | Field-level translations/paraphrases, source pages and uncertainty notes. |
| [Transcription notes](05_Transcription_Notes_EN.md) | Data dictionary, page mapping, unresolved handwriting and the P01 deletion discrepancy. |
| [Original annotated record sheets (PDF)](source/Original_Annotated_Record_Sheets.pdf) | Unchanged bilingual source with handwritten data; this is the original evidence, not an English-only document. |

## Current prototype and how to run it

The Unity source is in [SpatialStories_IP2b_Quest_Project 2](../../SpatialStories_IP2b_Quest_Project%202). Its [CODEX_HANDOFF.md](../../SpatialStories_IP2b_Quest_Project%202/CODEX_HANDOFF.md) contains the dated technical history. The latest handoff entry documents **1.3.4 / code 8**, including removal of decorative posts obscuring the T/S tools. This does not establish the build used in the handwritten test records: those actual-build fields are blank.

1. Clone or download the repository. In Unity Hub, add the `SpatialStories_IP2b_Quest_Project 2` folder containing `Assets`, `Packages` and `ProjectSettings`.
2. Open with **Unity 6000.3.20f1** and allow package import to finish.
3. Select **Tools > Spatial Stories IP2b > 1 - Open Quest scene**. The desktop alternative is **2 - Open Simulator scene**.
4. Run **3 - Validate open IP2b scene**. For regression tests, use **Window > General > Test Runner > EditMode > Run All**.
5. For Quest, use the configured Android build profile and **4 - Build IP2b Quest APK**. Desktop simulation is useful for debugging; it does not replace headset evaluation.

The new task is to move a prepared subject from a source photo onto a different background, add text, adjust their overlap and inspect the flat story before simulated publishing. The two cutouts are prepared assets; the prototype does not perform arbitrary-photo or live AI segmentation. Publishing is local simulation.

| Action | Control |
|---|---|
| Select a background | BEGIN, then point at a photo and use Trigger. |
| Get a cutout | Use the exposed PHOTO CUTOUTS entry beside S, or S > FROM PHOTO; Grip the subject. |
| Place it | Move towards the target photo and release Grip when the placement preview and release cue appear. |
| Add text | T sphere, choose/create text, then move it onto the photo. |
| Change overlap | Select an element, then FORWARD/BACKWARD; NEXT LAYER selects a covered layer. |
| Resize or rotate | Two-hand Grip for size; one-hand wrist movement for rotation. |
| Delete and recover | Put the element inside the left bin and release; select UNDO within five seconds. |
| Inspect final story | Blue Publish sphere > FLAT STORY PREVIEW; return to edit or confirm simulated publication. |

## Submission scope

The IP2 brief requires the IP2b results on GitHub and a detailed testing plan. For DECO7230 the plan includes a research paragraph with at least five relevant academic references. The English plan and report above are ready to download; the report also records the current evidence against all five lecture principles.

**Blackboard remains a separate submission step.** This GitHub upload does not submit the testing plan/research material to Blackboard or prove that the class-end deadline was met. No APK or video is newly supplied by this documentation upload; the existing Unity source remains linked above.

**Design Evaluation 3 and Reflection is separate.** Its brief calls for a maximum four-page body (up to three pages of results and one page of reflection), raw-data appendix, and submission to both Blackboard and GitHub. This IP2b package provides evidence for that work; it is not represented as a completed Evaluation 3 reflection.

## Evidence and AI assistance

The preprinted planned date, version and settings on the source form are not treated as actual recorded metadata. Record-slot labels do not establish attendance or a formal participant count. Technical self-tests documented in the project handoff are kept distinct from the handwritten outcomes.

AI assistance was used for visual transcription checks, English translation, evidence organisation and document formatting. Unclear entries are labelled; no missing participant data was generated. The existing prototype also uses AI-assisted code/documentation and prepared cutout artwork as disclosed in the project materials.
