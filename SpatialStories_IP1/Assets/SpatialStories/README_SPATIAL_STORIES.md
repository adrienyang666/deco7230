# Spatial Stories — Interactive Prototype 1

**Course:** DECO2300/7230 Digital Prototyping and Extended Reality  
**Prototype:** Unity horizontal prototype  
**Concept:** Instagram Story creation reimagined as spatial direct manipulation in mixed reality.

## What is already implemented

The project automatically creates `Assets/SpatialStories/Scenes/SpatialStories_IP1.unity` the first time it is opened in Unity.

The prototype includes the complete IP1 flow:

1. Floating six-photo album wall
2. Hover feedback
3. First selection with a yellow border
4. Second selection moves/enlarges the photo into the editing space
5. Floating `T` text tool
6. Floating `S` sticker tool and three sticker choices
7. Text/sticker overlays that can be selected and dragged on the photo
8. Spatial trash bin for deleting the selected overlay
9. Red `PUBLISH` button revealed after an edit
10. Published Story confirmation state
11. Reset control for repeated classroom testing
12. Automatic interaction logging in the Unity Console
13. Automatic CSV summary logging when a Story is published

## Controls

- **Left click:** simulate pinch / select / interact
- **Left click + drag an overlay:** reposition text or sticker
- **Right mouse drag:** look around
- **W A S D:** move
- **Q / E:** move down / up
- **R:** reset prototype for the next participant

## Open the prototype

Open:

`Assets/SpatialStories/Scenes/SpatialStories_IP1.unity`

If it has not been generated yet, wait for Unity to finish compiling. It will be generated automatically.

You can also manually rebuild it from:

`Tools > Spatial Stories > Build or Rebuild IP1 Prototype`

## Intended test flow

Ask a participant to create and publish a Story without giving step-by-step instructions. Observe whether they can:

- discover photo selection;
- understand the yellow selected state;
- bring a photo into the editing workspace;
- interpret the T and S spatial tools;
- move an overlay;
- discover the trash interaction;
- find and understand Publish.

## Test log

A CSV row is written every time a participant publishes. Unity prints the save location in Console as:

`[SpatialStories Test] CSV saved to: .../SpatialStories_TestLog.csv`

The CSV contains:

- selected photo
- number of photo selections
- text additions
- sticker additions
- deletions
- total completion time

This is intended to supplement observation notes, not replace them.

## Notes

The desktop controls deliberately simulate the later MR interaction vocabulary. The IP1 goal is to test the horizontal workflow and spatial affordances before investing in Quest hand tracking and passthrough integration.
