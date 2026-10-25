# Transcription notes and data dictionary

Prepared 25 October 2026 from the supplied annotated, bilingual record-sheet PDF. These notes document the English transcription; they do not replace the source.

## Source integrity

- Source copy: [Original annotated record sheets](source/Original_Annotated_Record_Sheets.pdf)
- Pages: 11; annotations and handwriting retained unchanged.
- SHA-256: `a72db849819e9e81984cfdc0b468ce83b483d6685213fc79c8ff96fc79685aa2`
- Source pages 1-2 = P01; 3-4 = P02; 5-6 = P03; 7-8 = P04; 9-10 = P05; 11 = principles sheet.
- The PDF text layer contains the form, while the handwritten marks are annotations. Visual inspection, not text extraction alone, was used for transcription.

## Record identifiers and missing values

`record_slot` is the preprinted slot on the form. It is not a verified participant or session ID. P01/P02 have marks resembling 1/2 in their ID fields. P03/P04 have limited header marks but no outcome data. P05 has task circles and feedback but no handwritten ID. The actual ID columns are left empty to avoid converting slot labels into verified participant identity.

Three records have task outcomes: P01, P02 and P05. P03 and P04 are retained as `no_outcomes_recorded`. Their missing cells are not coded N: the source does not establish whether a task was attempted. Blank numeric cells are missing, never zero.

Actual test date, session ID, trial classification, build, posture and switch settings were not entered. Device and VR-experience markings are incomplete; no precise model or experience level is inferred. Preprinted planned values are not copied into actual-data fields. No device event logs have been matched to these records.

## Outcome and rating fields

- I: completed without extra facilitator assistance; the product demonstration is permitted.
- A: completed after extra assistance.
- F: attempted but unfinished.
- N: not attempted or interrupted, where this option was explicitly circled.
- Scaling and Delete/Undo are optional subtasks. A failure or omission on these does not automatically change the core outcome.
- `overall_core_outcome` is the circled overall result, not a newly calculated score.
- Naturalness and comfort use the printed 1-5, study-specific scales. Naturalness: 1 very unnatural, 3 neither, 5 very natural. Comfort: 1 very uncomfortable, 3 neither, 5 very comfortable.
- P05 `comfort_1_to_5` is empty and `comfort_status` is `unclear`. The handwritten mark is not reliably identifiable as a valid 1-5 response; it is not recoded to 0 or 5.

## Handwriting and apparent discrepancies

| Source | Handling |
|---|---|
| P01, page 2, naturalness reason | The mismatch with intuition is legible. The following English word resembles `tiger`, but is unclear. It is not silently changed to `trigger`. |
| P01, page 2, comfort reason | The reason is not reliably legible. No claim about flicker, habits or the cause of discomfort is derived from it. |
| P01, pages 1-2, layer order | Overlap is circled A and its explanation I; the note reports that layer order was not changed correctly. Both stand; timing and exact help are unavailable. |
| P01, pages 1-2, deletion | Delete/Undo is circled I. A bottom note says unwanted content was not deleted, with the final object unclear. This unresolved difference is preserved. |
| P02, page 4, naturalness reason | English handwriting may include `reaching`; the phrase is not confidently readable and is excluded from interpretation. |
| P02, page 4, published-content record | The request to record already posted content is legible; the exact word `menu` or `memo` is unclear. Translated as a request for a history/record, without prescribing a UI. |
| P02, page 4, grab note | Translated as the grabbed item not being centred. No cause, measured offset or failed-grab count is inferred. |
| P05, page 10, findability | Only `hard to find the` is reliably legible. The final word is left unclear; the note is not assigned to Undo, cutouts or another particular feature. |
| P05, page 10, comfort | Both the numeric mark and the reason are uncertain. A legible intensifier meaning `very` does not establish a positive or negative judgement. |
| P05, page 10, layer bar | The legible clause requests a higher layer bar. Opening wording remains unclear. |
| P05, page 10, margin note | The legible clause requests progress in a corner. The English action label is unclear; it is not assumed to mean recording, reading or loading. |
| Page 11 | The principles checklist is blank. The report's principles review is a new, clearly labelled retrospective synthesis, not a completed original checklist. |

The feedback file includes missing fields for the three outcome-bearing records so that blank answers remain visible. P03/P04 have no feedback entries because their feedback pages are blank; their status remains visible in the task CSV. Q/P/O was not entered in the event tables, so translated notes are not presented as authenticated verbatim participant quotations.

## Repository context

The preparation sources were read at commit `bf745a3bb7f078415187de8cfcf7c655a00559e6`. The latest entry in `SpatialStories_IP2b_Quest_Project 2/CODEX_HANDOFF.md` describes 1.3.4/code 8. Older text in START_HERE and VALIDATION includes historical statuses. Technical checks in the handoff are attributed to those records and were not repeated during this documentation task.

If an unclear entry is confirmed later, update the affected CSV and report together, retain the original PDF, and note the correction in Git history.
