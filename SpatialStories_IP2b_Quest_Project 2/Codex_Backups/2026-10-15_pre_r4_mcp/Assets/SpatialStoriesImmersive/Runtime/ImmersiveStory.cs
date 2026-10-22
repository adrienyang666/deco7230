using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace SpatialStories.Immersive
{
    /// <summary>Saved spatial gallery, direct manipulation, tool spheres and publish workflow.</summary>
    public sealed partial class ImmersiveStory : MonoBehaviour
    {
        public Camera head;
        public Transform contentRoot, focusAnchor, spawnAnchor, reviewAnchor;
        public FloatingStoryItem[] mediaCards;
        public Button[] photoButtons;
        public Vector3[] albumPositions, albumScales;
        public Quaternion[] albumRotations;
        public GameObject welcomePanel, textPanel, stickerPanel, reviewPanel, successPanel, adminPanel;
        public GameObject toolGroup, navigation, publishObject, undoObject;
        public Collider trashVolume;
        public Renderer[] trashGlow;
        public Material trashIdleMaterial, trashActiveMaterial;
        public Text stageLabel, hintLabel, draftLabel, undoLabel, reviewLabel, sessionLabel;
        public FloatingStoryItem textTemplate;
        public FloatingStoryItem[] stickerTemplates;
        [Min(1)] public int maxOverlaysPerPhoto = 12;
        [Min(3)] public float undoSeconds = 5f;
        [Range(1f, 2f)] public float inspectScale = 1.45f;
        public const string KeyboardCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.,!?'-";
        [HideInInspector] public int editorPreviewMode;

        enum Stage { Welcome, Browse, Edit, Review, Published }
        Stage stage;
        readonly List<FloatingStoryItem> overlays = new List<FloatingStoryItem>();
        readonly Dictionary<FloatingStoryItem, bool> edited = new Dictionary<FloatingStoryItem, bool>();
        readonly Dictionary<FloatingStoryItem, Vector3> initialGrabPosition = new Dictionary<FloatingStoryItem, Vector3>();
        readonly Dictionary<FloatingStoryItem, Quaternion> initialGrabRotation = new Dictionary<FloatingStoryItem, Quaternion>();
        readonly Dictionary<FloatingStoryItem, Vector3> initialGrabScale = new Dictionary<FloatingStoryItem, Vector3>();
        readonly HashSet<FloatingStoryItem> twoHandLogged = new HashSet<FloatingStoryItem>();
        readonly Dictionary<FloatingStoryItem, Coroutine> motions = new Dictionary<FloatingStoryItem, Coroutine>();
        FloatingStoryItem selected, textTarget, deletedItem, lastTouchedText, pendingTextEdit;
        bool transitioning, upperCase = true, bold;
        float undoDeadline, lastActionTime;
        int lastAction = -1, serial;
        string draft = "", audience = "Friends";
        Color chosenColor = Color.white;
        StorySessionRecord recorder;
        readonly Color[] colors = { Color.white, new Color(1f,.81f,.23f), new Color(.30f,.9f,.73f),
            new Color(1f,.47f,.7f), new Color(.35f,.65f,1f), new Color(.06f,.08f,.12f) };
        public string StageName => stage.ToString();
        bool Holding => AllItems().Any(i => i && (enableCollage ? i.Busy : i.Held));
        bool CanReview => selected && edited.TryGetValue(selected, out var value) && value &&
            !Holding && !transitioning && overlays.Where(i => i && !i.deleted && IsOnSelected(i)).All(i => i.placed);

        void Start()
        {
            if (!head) head = Camera.main;
            if (!contentRoot || !focusAnchor || !spawnAnchor || mediaCards == null || mediaCards.Length == 0)
            {
                Debug.LogError("Full concept scene is incomplete. Use Tools > Spatial Stories Full Concept > 1 - Create or open full scenes.", this);
                enabled = false; return;
            }
            EnsureDesktopSimulatorHeight();
            recorder = gameObject.AddComponent<StorySessionRecord>();
            EnsureEventSystem();
            foreach (var canvas in contentRoot.GetComponentsInChildren<Canvas>(true)) canvas.worldCamera = head;
            // A runtime guard for old/externally edited RawImage serialization, including inactive panels.
            foreach (var image in contentRoot.GetComponentsInChildren<RawImage>(true)) image.uvRect = new Rect(0,0,1,1);
            for (int i=0; i<mediaCards.Length; i++)
            {
                var card = mediaCards[i]; if (!card) continue;
                card.controller = this; edited[card] = false; Home(card); card.SavePose();
            }
            stage = Stage.Welcome;
            InitializeCollage();
            ClosePanels(); Set(adminPanel, false); Refresh();
            Hint("Your memories, floating within reach.\nPoint at BEGIN and press the trigger.");
            StartCoroutine(InitialAlignment());
        }

        IEnumerator InitialAlignment()
        {
            yield return new WaitForSecondsRealtime(.5f);
            if (stage != Stage.Welcome) yield break;
            // Simulated poses are applied by the Input System after Awake/Start.
            // Establish the offset again before taking the one-time alignment snapshot.
            if (EnsureDesktopSimulatorHeight()) yield return null;
            if (stage == Stage.Welcome)
            {
                Recenter();
                if (gameObject.scene.name == "SpatialStories_IP2b_Simulator") LogDesktopSimulatorPositions();
            }
        }

        // Keep these startup helpers in this file so updating ImmersiveStory.cs
        // alone cannot introduce missing-method errors from a partial import.
        bool EnsureDesktopSimulatorHeight()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying || !enableCollage || !head ||
                gameObject.scene.name != "SpatialStories_IP2b_Simulator") return false;
            var simulator = FindFirstObjectByType<XRInteractionSimulator>();
            if (!simulator || !simulator.isActiveAndEnabled || simulator.gameObject.scene != gameObject.scene) return false;
            var inputSubsystems = new List<XRInputSubsystem>();
            SubsystemManager.GetSubsystems(inputSubsystems);
            foreach (var subsystem in inputSubsystems)
                if (subsystem.running) return false;

            // In the supplied rig this parent contains the camera and both
            // controllers. Do not write the camera's tracked local pose.
            var offset = head.transform.parent;
            if (!offset || offset.name != "Camera Offset")
            {
                Debug.LogError("[IP2b startup] Expected Main Camera directly under Camera Offset in the supplied XR rig.", this);
                return false;
            }

            // With no XR input subsystem, CameraYOffset need not be applied by
            // XROrigin. Translate the common parent explicitly instead.
            // Only this desktop scene uses the known world-floor Y = 0.
            const float eyeHeight = 1.65f;
            offset.position += Vector3.up * (eyeHeight - head.transform.position.y);
            return true;
#else
            return false;
#endif
        }

        [ContextMenu("Log IP2b desktop positions")]
        public void LogDesktopSimulatorPositions()
        {
            if (!head || !contentRoot || !welcomePanel) return;
            var offset = head.transform.parent;
            var rect = welcomePanel.GetComponent<RectTransform>();
            float bottom = welcomePanel.transform.position.y;
            if (rect)
            {
                var corners = new Vector3[4];
                rect.GetWorldCorners(corners);
                foreach (var corner in corners) bottom = Mathf.Min(bottom, corner.y);
            }
            Debug.Log($"[IP2b startup r3] Scene={gameObject.scene.name}; Stage={StageName}; " +
                $"Camera world Y={head.transform.position.y:F3}; " +
                $"Offset local Y={(offset ? offset.localPosition.y.ToString("F3") : "missing")}; " +
                $"Content world Y={contentRoot.position.y:F3}; Welcome bottom Y={bottom:F3}. " +
                "Fresh desktop startup should have camera Y about 1.650, content Y 0.000 and welcome bottom Y 1.150.", this);
        }

        void EnsureEventSystem()
        {
            var events = FindFirstObjectByType<EventSystem>();
            if (!events) events = new GameObject("EventSystem - XR UI").AddComponent<EventSystem>();
            foreach (var module in events.GetComponents<BaseInputModule>())
                if (!(module is XRUIInputModule)) { module.enabled = false; Destroy(module); }
            if (!events.GetComponent<XRUIInputModule>()) events.gameObject.AddComponent<XRUIInputModule>();
        }

        IEnumerable<FloatingStoryItem> AllItems() => (mediaCards ?? Array.Empty<FloatingStoryItem>()).Concat(overlays).Concat(sourceHandles);
        bool IsOnSelected(FloatingStoryItem item) => selected && item && (item.ownerPhoto == selected || item.transform.IsChildOf(selected.transform));
        static void Set(GameObject obj, bool value) { if (obj && obj.activeSelf != value) obj.SetActive(value); }
        public void Hint(string value) { if (hintLabel) hintLabel.text = value; }
        void Record(string action, string detail = "") { if (recorder) recorder.Event(action, StageName, detail); }

        public bool IsActionAllowed(int action)
        {
            if (!Application.isPlaying || !enabled || !recorder) return false;
            if (action == 20 || action == 21 || action == 22) return true;
            if (Holding || transitioning) return false;
            if (action >= 600 && action <= 604) return CollageActionAllowed(action);
            if (action == 1) return stage == Stage.Welcome;
            if (action == 8 || action == 23) return true;
            if (action == 3) return deletedItem && Time.unscaledTime < undoDeadline && stage != Stage.Review && stage != Stage.Published;
            if (action == 2 || action == 4) return stage == Stage.Edit || stage == Stage.Browse;
            if (action >= 100 && action < 100 + mediaCards.Length) return stage == Stage.Browse || stage == Stage.Edit;
            if (action == 5) return stage == Stage.Edit && CanReview;
            if (action == 6) return stage == Stage.Review && (!enableCollage || flatPreviewReady);
            if (action == 7 || action == 18 || action == 19) return stage == Stage.Review;
            if (action == 9 || action == 10 || action == 11) return stage == Stage.Edit;
            if (action >= 200 && action < 206) return stage == Stage.Edit && stickerPanel && stickerPanel.activeSelf;
            if (action == 12 || action == 13 || action == 14 || action == 15 || action == 16 || action == 17 ||
                action == 500 || action >= 300 && action < 304 || action >= 400 && action < 406 || action >= 1000)
                return stage == Stage.Edit && textPanel && textPanel.activeSelf;
            return false;
        }

        public void Act(int action)
        {
            if (!IsActionAllowed(action)) return;
            // UI and physical face may report the same press in one frame.
            if (action == lastAction && Time.unscaledTime - lastActionTime < .15f) return;
            lastAction = action; lastActionTime = Time.unscaledTime;
            if (stage == Stage.Edit && action != 20 && action != 21 && action != 22)
                DismissCutoutDiscovery("tool_" + action);
            if (action >= 600 && action <= 604) { CollageAction(action); return; }
            if (action >= 1000 && action < 1000 + KeyboardCharacters.Length)
            {
                var character = KeyboardCharacters[action-1000];
                if (!upperCase) character = char.ToLowerInvariant(character);
                if (draft.Length < 48) draft += character;
                UpdateDraft(); return;
            }
            if (action >= 400 && action < 406) { chosenColor = colors[action-400]; UpdateDraft(); return; }
            if (action >= 300 && action < 304)
            {
                draft = new[] { "Weekend", "Good day", "Wish you were here", "Hello from..." }[action-300];
                UpdateDraft(); return;
            }
            if (action >= 200 && action < 206) { AddSticker(action-200); return; }
            if (action >= 100 && action < 100 + mediaCards.Length) { SelectCard(mediaCards[action-100]); return; }
            switch (action)
            {
                case 1:
                    recorder.Begin(); stage = Stage.Browse;
                    if (enableCollage) Record("prototype_config", "version=" + Application.version +
                        " | discovery=" + enableCutoutDiscovery + " | placement_feedback=" + enablePlacementFeedback);
                    Refresh(); Hint("Point + trigger to highlight a memory.\nGrip to bring it closer, or press its trigger again."); break;
                case 2: ReturnToAlbum(); break;
                case 3: UndoDelete(); break;
                case 4: ReturnToAlbum(); break;
                case 5: Review(); break;
                case 6: Publish(); break;
                case 7: stage = Stage.Edit; Refresh(); Hint("Keep shaping your Story."); break;
                case 8: ResetSession(); break;
                case 9: ClosePanels(); Hint("Grip an item to move, turn or resize it."); break;
                case 10: OpenText(lastTouchedText && !lastTouchedText.deleted && IsOnSelected(lastTouchedText) ? lastTouchedText : null); break;
                case 11: ClosePanels(); Set(stickerPanel,true); Record("open_sticker_sphere"); break;
                case 12: textTarget = null; draft = ""; UpdateDraft(); break;
                case 13: ApplyText(); break;
                case 14: if (draft.Length>0) draft=draft.Substring(0,draft.Length-1); UpdateDraft(); break;
                case 15: draft=""; UpdateDraft(); break;
                case 16: upperCase=!upperCase; UpdateDraft(); break;
                case 17: if(draft.Length<48) draft+=" "; UpdateDraft(); break;
                case 18: audience="Friends"; UpdateReview(); break;
                case 19: audience="Only me"; UpdateReview(); break;
                case 20: recorder.Prompt(StageName); UpdateSession(); break;
                case 21: Set(adminPanel, !adminPanel.activeSelf); UpdateSession(); break;
                case 22: recorder.End("incomplete","facilitator ended trial"); UpdateSession(); break;
                case 23: Recenter(); break;
                case 500: bold=!bold; UpdateDraft(); break;
            }
            RefreshCutoutDiscovery();
        }

        void SelectCard(FloatingStoryItem card)
        {
            if (!card || card.deleted) return;
            if (stage == Stage.Edit)
            {
                if (card != selected) { Hint("Use Back to album to change your memory."); return; }
                return;
            }
            if (selected == card) { EnterEditor(card); return; }
            if (selected) { selected.persistentHighlight = false; Home(selected); }
            selected = card; card.persistentHighlight = true;
            int index = Array.IndexOf(mediaCards, card);
            var target = albumPositions[index] + card.transform.localRotation * Vector3.back * .18f;
            Move(card, target, albumRotations[index], albumScales[index]*1.08f, .25f, null);
            Record("select_photo", card.itemId);
            Hint("Selected: " + card.content + ".\nGrip to inspect, or trigger this photo again.");
        }

        public void ItemHovered(FloatingStoryItem item, bool hovered)
        {
            if (!item || !Application.isPlaying) return;
            if (enableCollage && hovered && item.isSourceHandle && !Holding)
            {
                // Keep the real grab target still and end the ghost movement when it is aimed at.
                if (item.isDiscoveryHandle) StopDiscoveryDemoOnAim();
                Hint("Grip this subject to lift it from the source photo.\nMove it over a background photo and release.");
            }
            if (hovered && item.isPhoto && stage == Stage.Browse && !Holding)
                Hint("Point + trigger to select " + item.content + ".\nGrip to bring this memory closer.");
        }

        public void ItemGrabbed(FloatingStoryItem item)
        {
            if (!item || !recorder) return;
            // Two hands may resize one object; they must not start two competing photo transitions.
            if (AllItems().Any(other => other && other != item && (enableCollage ? other.Busy : other.Held)))
            {
                if (item.Grab && item.Grab.interactionManager)
                    // XRGrabInteractable otherwise binds the removed XRBaseInteractable overload.
                    item.Grab.interactionManager.CancelInteractableSelection((IXRSelectInteractable)item.Grab);
                Hint("Use both hands on the same object, or release it before choosing another.");
                return;
            }
            StopMotion(item);
            if (stage != Stage.Browse && stage != Stage.Edit) return;
            if (enableCollage && item.isSourceHandle && !LiftSubject(item)) return;
            if (stage == Stage.Edit) DismissCutoutDiscovery("grab_" + item.itemId);
            if (enableCollage && !item.isPhoto) SetActiveLayer(item);
            initialGrabPosition[item] = item.transform.position;
            initialGrabRotation[item] = item.transform.rotation;
            initialGrabScale[item] = item.transform.localScale;
            twoHandLogged.Remove(item);
            if (item.isText) lastTouchedText = item;
            if (item.isPhoto && stage == Stage.Browse)
            {
                if (selected && selected != item) { selected.persistentHighlight=false; Home(selected); }
                selected=item; item.persistentHighlight=true;
                item.BeginHeldEnlargement(albumScales[Array.IndexOf(mediaCards,item)] * inspectScale);
                Hint("The photo follows your hand. Release to inspect.\nUse both grips to resize.");
            }
            else Hint("Turn your wrist to rotate. Two grips to resize.\nMove left to discard, or release to place.");
            if (enableCollage && item.isCutout) Hint("Move the subject onto a photo, then release.\nTwo grips resize; one hand turns it.");
            Record("grab", item.itemId);
            Refresh();
        }

        public void ItemActivated(FloatingStoryItem item)
        {
            if (!item || transitioning) return;
            if (item.isText && stage == Stage.Edit && IsOnSelected(item))
            {
                if (item.Held) { pendingTextEdit = item; Hint("Release the text to edit its words."); }
                else if (!Holding) OpenText(item);
            }
            else if (item.isPhoto && stage == Stage.Browse && !item.Held && !Holding) SelectCard(item);
        }

        public void ItemReleased(FloatingStoryItem item, bool cancelled)
        {
            if (!item || !recorder) return;
            if (cancelled)
            {
                if (enableCollage && item.justLifted) { ReturnUnusedSubject(item, "tracking_cancel"); return; }
                if (pendingTextEdit == item) pendingTextEdit = null;
                Record("tracking_cancel_return", item.itemId); Refresh(); return;
            }
            if (stage != Stage.Browse && stage != Stage.Edit) { item.RestorePose(); return; }
            Record("release", item.itemId);
            if (InsideBin(item.transform.position)) { Delete(item); return; }
            var toBin = trashVolume ? trashVolume.bounds.center-item.transform.position : Vector3.zero;
            var velocity = item.ReleaseVelocity;
            if (!enableCollage && trashVolume && toBin.magnitude < 1.4f && velocity.magnitude > .65f &&
                Vector3.Dot(velocity.normalized,toBin.normalized) > .80f)
            { StartCoroutine(TossToBin(item,velocity)); return; }
            ResolvePlacement(item);
            if (pendingTextEdit == item)
            {
                pendingTextEdit = null;
                if (!item.deleted && stage == Stage.Edit && !Holding) OpenText(item);
            }
        }

        IEnumerator TossToBin(FloatingStoryItem item, Vector3 velocity)
        {
            transitioning=true; Refresh();
            velocity=Vector3.ClampMagnitude(velocity,2.5f);
            float elapsed=0;
            Record("throw_toward_bin",item.itemId);
            while (elapsed<.65f && item && !item.deleted)
            {
                float step=Mathf.Min(Time.deltaTime,.033f); elapsed+=step;
                velocity+=Vector3.down*1.8f*step;
                item.transform.position+=velocity*step;
                if(InsideBin(item.transform.position))
                { transitioning=false; Delete(item); yield break; }
                yield return null;
            }
            transitioning=false;
            if(item) { item.RestorePose(); Record("missed_throw_return",item.itemId); }
            Hint("The item returned safely. Move it over the open bin and release."); Refresh();
        }

        void ResolvePlacement(FloatingStoryItem item)
        {
            if (enableCollage && !item.isPhoto) { PlaceCollageItem(item); return; }
            if (item.isPhoto)
            {
                var homeScale=albumScales[Array.IndexOf(mediaCards,item)].x;
                if (item.transform.localScale.x < homeScale*.72f)
                { Record("pinch_minimise",item.itemId); ReturnToAlbum(); return; }
                if (stage == Stage.Browse) { EnterEditor(item,true); return; }
                var local = contentRoot.InverseTransformPoint(item.transform.position);
                if (Mathf.Abs(local.x)>1.2f || local.y<.55f || local.y>2.45f || local.z<.35f || local.z>2.6f)
                { item.RestorePose(); Hint("Returned to a comfortable reach. Grip and move again."); Record("boundary_return",item.itemId); }
                else
                {
                    bool moved=initialGrabPosition.TryGetValue(item,out var pos)&&Vector3.Distance(pos,item.transform.position)>.035f;
                    bool rotated=initialGrabRotation.TryGetValue(item,out var rot)&&Quaternion.Angle(rot,item.transform.rotation)>8f;
                    bool scaled=initialGrabScale.TryGetValue(item,out var scale)&&Mathf.Abs(scale.x-item.transform.localScale.x)>.05f;
                    if(moved||rotated||scaled) edited[item]=true;
                    item.SavePose(); Record("photo_transform",item.itemId);
                    Hint("Photo placed. Use T / Text or S / Sticker to personalise it.");
                }
            }
            else if (selected && IsOnSelected(item))
            {
                var local=selected.transform.InverseTransformPoint(item.transform.position);
                if(Mathf.Abs(local.x)<selected.size.x*.5f+.18f && Mathf.Abs(local.y)<selected.size.y*.5f+.18f && Mathf.Abs(local.z)<.28f)
                {
                    float angle=Vector3.SignedAngle(selected.transform.up,
                        Vector3.ProjectOnPlane(item.transform.up,selected.transform.forward),selected.transform.forward);
                    float sx=Mathf.Abs(item.transform.localScale.x), sy=Mathf.Abs(item.transform.localScale.y);
                    float a=angle*Mathf.Deg2Rad;
                    float halfX=(Mathf.Abs(Mathf.Cos(a))*item.size.x*sx+Mathf.Abs(Mathf.Sin(a))*item.size.y*sy)*.5f;
                    float halfY=(Mathf.Abs(Mathf.Sin(a))*item.size.x*sx+Mathf.Abs(Mathf.Cos(a))*item.size.y*sy)*.5f;
                    local.x=Mathf.Clamp(local.x,-Mathf.Max(0,selected.size.x*.5f-halfX),Mathf.Max(0,selected.size.x*.5f-halfX));
                    local.y=Mathf.Clamp(local.y,-Mathf.Max(0,selected.size.y*.5f-halfY),Mathf.Max(0,selected.size.y*.5f-halfY));
                    local.z=-.035f-.004f*overlays.IndexOf(item);
                    item.transform.localPosition=local;
                    item.transform.localRotation=Quaternion.Euler(0,0,angle);
                    item.placed=true; item.SavePose(); edited[selected]=true;
                    Hint("Placed on your Story. Grip again to adjust.\nThe blue Publish sphere is now available.");
                    Record("place_overlay",item.itemId);
                }
                else { item.RestorePose(); Hint("Release near your photo to attach, or over the bin to delete."); Record("invalid_drop_return",item.itemId); }
            }
            Refresh();
        }

        void EnterEditor(FloatingStoryItem card, bool fromGrip = false)
        {
            selected=card; stage=Stage.Edit; transitioning=true; ClosePanels(); Refresh();
            Hint("Bringing your memory closer...");
            var local=contentRoot.InverseTransformPoint(focusAnchor.position);
            var targetScale=fromGrip && card.ResizedWithTwoHandsDuringGrab ? card.transform.localScale :
                albumScales[Array.IndexOf(mediaCards,card)]*inspectScale;
            Move(card,local,Quaternion.Inverse(contentRoot.rotation)*focusAnchor.rotation,targetScale,.42f,()=>
            {
                transitioning=false; card.SavePose(); Refresh();
                Hint("Your memory, in your hands.\nChoose T / Text or S / Sticker; grip the photo to inspect.");
                if (enableCollage) Hint(discoveryEntry && enableCutoutDiscovery ?
                    "T adds words. Grip the cutout beside S, or open S for more.\nTwo grips resize without turning." :
                    "T adds words. S > FROM PHOTO lifts a subject.\nGrip to move; two grips resize without turning.");
                Record("editor_ready",card.itemId);
                BeginCutoutDiscovery();
            });
        }

        void ReturnToAlbum()
        {
            if(Holding||transitioning) return;
            ClosePanels();
            if(selected) { selected.persistentHighlight=false; Home(selected); }
            selected=null; lastTouchedText=null; pendingTextEdit=null; stage=Stage.Browse; Refresh();
            Hint("Choose another memory. Your edits stay with each photo."); Record("back_to_album");
        }

        void Home(FloatingStoryItem card)
        {
            StopMotion(card); int i=Array.IndexOf(mediaCards,card);
            if(i<0||albumPositions==null||i>=albumPositions.Length) return;
            card.transform.SetParent(contentRoot,false);
            card.transform.localPosition=albumPositions[i];
            card.transform.localRotation=albumRotations[i]; card.transform.localScale=albumScales[i];
            card.SavePose();
        }

        void StopMotion(FloatingStoryItem item)
        {
            if(motions.TryGetValue(item,out var motion) && motion!=null) StopCoroutine(motion);
            motions.Remove(item);
        }
        void Move(FloatingStoryItem item,Vector3 position,Quaternion rotation,Vector3 scale,float duration,Action finished)
        {
            StopMotion(item); motions[item]=StartCoroutine(Animate(item,position,rotation,scale,duration,finished));
        }
        IEnumerator Animate(FloatingStoryItem item,Vector3 pos,Quaternion rot,Vector3 scale,float duration,Action finished)
        {
            var start=item.transform.localPosition; var sr=item.transform.localRotation; var ss=item.transform.localScale;
            float elapsed=0;
            while(elapsed<duration && item && !item.Held)
            {
                elapsed+=Time.deltaTime; float t=Mathf.SmoothStep(0,1,elapsed/duration);
                item.transform.localPosition=Vector3.Lerp(start,pos,t);
                item.transform.localRotation=Quaternion.Slerp(sr,rot,t);
                item.transform.localScale=Vector3.Lerp(ss,scale,t); yield return null;
            }
            motions.Remove(item);
            if(item && !item.Held)
            {
                item.transform.localPosition=pos; item.transform.localRotation=rot; item.transform.localScale=scale;
                item.SavePose(); finished?.Invoke();
            }
        }

        void OpenText(FloatingStoryItem target)
        {
            DismissCutoutDiscovery("open_text");
            ClosePanels(); textTarget=target;
            draft=target ? target.content : ""; chosenColor=target ? target.textColor : Color.white;
            bold=target && target.label && target.label.fontStyle==FontStyle.Bold;
            Set(textPanel,true); UpdateDraft(); Record("open_text_sphere");
            Hint("Type with the floating keyboard, or start with a phrase.\nThen create your text and grip it onto the photo.");
            RefreshCutoutDiscovery();
        }
        void UpdateDraft()
        {
            if(!draftLabel) return;
            draftLabel.text=string.IsNullOrEmpty(draft) ? "Your words..." : draft;
            draftLabel.color=chosenColor; draftLabel.fontStyle=bold ? FontStyle.Bold : FontStyle.Normal;
        }
        void ApplyText()
        {
            if(string.IsNullOrWhiteSpace(draft)) { Hint("Type a word or choose a phrase first."); return; }
            var item=textTarget;
            if(!item)
            {
                item=Spawn(textTemplate); if(!item) return;
                item.isText=true; item.itemId="text_"+(++serial);
            }
            item.content=draft; item.textColor=chosenColor;
            if(item.label) { item.label.text=draft; item.label.color=chosenColor; item.label.fontStyle=bold ? FontStyle.Bold : FontStyle.Normal; }
            if(textTarget) edited[selected]=true;
            ClosePanels(); Record("create_or_edit_text",item.itemId); Refresh();
            Hint("Your text is floating beside the photo.\nHold grip to place, rotate and resize it.");
        }
        void AddSticker(int index)
        {
            if(stickerTemplates==null||index>=stickerTemplates.Length) return;
            var item=Spawn(stickerTemplates[index]); if(!item) return;
            item.itemId="sticker_"+index+"_"+(++serial); item.stickerIndex=index;
            ClosePanels(); Record("create_sticker",item.itemId); Refresh();
            Hint("Your sticker is floating beside the photo.\nGrip to move it; two grips to resize.");
        }
        FloatingStoryItem Spawn(FloatingStoryItem template)
        {
            if(!selected||!template) return null;
            if(overlays.Count(i=>i&&!i.deleted&&IsOnSelected(i))>=maxOverlaysPerPhoto)
            { Hint("This photo has twelve elements. Remove one before adding another."); return null; }
            if(overlays.Any(i=>i&&!i.deleted&&IsOnSelected(i)&&!i.placed))
            { Hint("Place your floating element on the photo before creating another."); return null; }
            var item=Instantiate(template, selected.transform);
            item.controller=this; item.isPhoto=false; item.placed=false; item.deleted=false;
            item.transform.position=spawnAnchor.position; item.transform.rotation=selected.transform.rotation;
            item.transform.localScale=Vector3.one/Mathf.Max(.1f,selected.transform.lossyScale.x);
            item.gameObject.SetActive(true); item.SavePose(); overlays.Add(item);
            if (enableCollage) PrepareCollageOverlay(item);
            return item;
        }
        void ClosePanels() { Set(textPanel,false); Set(stickerPanel,false); CloseSources(); textTarget=null; }

        bool InsideBin(Vector3 point)
        {
            if(!trashVolume) return false;
            var b=trashVolume as BoxCollider;
            if(b)
            {
                var p=b.transform.InverseTransformPoint(point)-b.center; var s=b.size*.5f;
                return Mathf.Abs(p.x)<=s.x&&Mathf.Abs(p.y)<=s.y&&Mathf.Abs(p.z)<=s.z;
            }
            return trashVolume.bounds.Contains(point);
        }
        void Delete(FloatingStoryItem item)
        {
            if (enableCollage && item.justLifted) { ReturnUnusedSubject(item, "discard_unused_subject"); return; }
            if (pendingTextEdit == item) pendingTextEdit = null;
            if (lastTouchedText == item) lastTouchedText = null;
            deletedItem=item; undoDeadline=Time.unscaledTime+undoSeconds;
            item.deleted=true; item.persistentHighlight=false; item.gameObject.SetActive(false);
            if(item.isPhoto) { selected=null; stage=Stage.Browse; ClosePanels(); }
            else if(selected) edited[selected]=true;
            if (enableCollage) { SetActiveLayer(null); Restack(item.ownerPhoto); }
            Record("delete",item.itemId); Refresh();
            Hint("Removed. Undo is available for " + undoSeconds.ToString("0") + " seconds.");
        }
        void UndoDelete()
        {
            if(!deletedItem) return;
            var item=deletedItem; deletedItem=null;
            item.deleted=false; item.gameObject.SetActive(true); item.RestorePose();
            if(item.isPhoto) { Home(item); stage=Stage.Browse; }
            if (enableCollage && !item.isPhoto) { Restack(item.ownerPhoto); SetActiveLayer(item); }
            Record("undo_delete",item.itemId); Hint("Restored. Your content is back."); Refresh();
        }

        void Review()
        {
            ClosePanels(); stage=Stage.Review; UpdateReview(); Refresh();
            Hint("Review the composition and audience.\nPress CONFIRM to simulate publishing."); Record("review");
            if (enableCollage) CaptureFlatPreview();
        }
        void UpdateReview()
        {
            if(reviewLabel) reviewLabel.text="Publish this Story?\n"+(selected ? selected.content : "")+
                "\nAudience: "+audience+"\nPrototype preview — no online upload";
        }
        void Publish()
        {
            if(!selected) return;
            stage=Stage.Published; transitioning=true; Refresh();
            Record("publish_confirmed",selected.itemId+" | "+audience);
            var destination=reviewAnchor ? contentRoot.InverseTransformPoint(reviewAnchor.position) : new Vector3(0,1.65f,2.4f);
            Move(selected,destination,Quaternion.identity,Vector3.one,.65f,()=>
            { transitioning=false; Refresh(); recorder.End("complete","simulated publish | "+audience); UpdateSession(); });
            Hint("Story published in this prototype.\nYour composition is complete.");
        }
        void ResetSession()
        {
            if(Holding) return;
            StopAllCoroutines(); motions.Clear(); transitioning=false;
            ResetCollage();
            if(recorder) recorder.End("incomplete","reset before completion");
            foreach(var item in overlays) if(item) Destroy(item.gameObject);
            overlays.Clear(); deletedItem=null; selected=null; lastTouchedText=null; pendingTextEdit=null;
            initialGrabPosition.Clear(); initialGrabRotation.Clear(); initialGrabScale.Clear(); twoHandLogged.Clear();
            foreach(var card in mediaCards)
            { if(!card)continue; card.deleted=false; card.persistentHighlight=false; card.gameObject.SetActive(true); Home(card); edited[card]=false; }
            ClosePanels(); Set(adminPanel,false); stage=Stage.Welcome; audience="Friends"; Refresh();
            Hint("Ready for the next participant.\nPress BEGIN when you are ready.");
        }

        void Refresh()
        {
            Set(welcomePanel,stage==Stage.Welcome);
            Set(toolGroup,(stage==Stage.Browse||stage==Stage.Edit)&&!transitioning);
            Set(navigation,stage==Stage.Browse||stage==Stage.Edit);
            Set(publishObject,stage==Stage.Edit&&CanReview);
            Set(reviewPanel,stage==Stage.Review);
            Set(successPanel,stage==Stage.Published&&!transitioning);
            Set(undoObject,deletedItem&&Time.unscaledTime<undoDeadline&&(stage==Stage.Browse||stage==Stage.Edit));
            if(trashVolume) Set(trashVolume.transform.parent.gameObject,stage==Stage.Browse||stage==Stage.Edit);
            for(int i=0;i<mediaCards.Length;i++)
            {
                var card=mediaCards[i]; if(!card)continue;
                bool visible=!card.deleted&&(stage==Stage.Browse||stage==Stage.Edit||stage==Stage.Review||stage==Stage.Published&&selected==card);
                if (enableCollage && stage==Stage.Review && flatPreviewReady && card==selected) visible=false;
                Set(card.gameObject,visible);
                bool interactive=visible&&!transitioning&&(!Holding||card.Held)&&(stage==Stage.Browse||stage==Stage.Edit&&card==selected);
                if(card.Grab && !card.Held) card.Grab.enabled=interactive;
                card.persistentHighlight=card==selected&&stage!=Stage.Published;
                if(photoButtons!=null&&i<photoButtons.Length&&photoButtons[i]) photoButtons[i].interactable=interactive&&stage==Stage.Browse;
            }
            foreach(var item in overlays)
                if(item&&item.Grab&&!item.Held) item.Grab.enabled=stage==Stage.Edit&&IsOnSelected(item)&&!transitioning&&!Holding;
            if(stageLabel) stageLabel.text=stage==Stage.Welcome ? "SPATIAL STORIES" : stage==Stage.Browse ? "01 / CHOOSE A MEMORY" :
                stage==Stage.Edit ? "02 / MAKE IT YOURS" : stage==Stage.Review ? "03 / READY TO SHARE" : "STORY COMPLETE";
            UpdateSession();
            RefreshCollage();
        }
        void UpdateSession()
        {
            if(sessionLabel&&recorder) sessionLabel.text="Trial: "+(recorder.SessionId??"not started")+"\nStage: "+StageName+
                "\nPrompts: "+recorder.Prompts+"\n"+(recorder.LastError??"Events saved locally; record ratings on your sheet.");
        }
        void LateUpdate()
        {
            if(!recorder)return;
            UpdatePlacementFeedback();
            bool over=false;
            foreach(var item in AllItems())
            {
                if(!item||!item.Held)continue;
                if(InsideBin(item.transform.position))over=true;
                if(item.SelectingHandCount>1&&twoHandLogged.Add(item))Record("two_hand_manipulation",item.itemId);
            }
            if(trashGlow!=null) foreach(var r in trashGlow) if(r)r.sharedMaterial=over?trashActiveMaterial:trashIdleMaterial;
            if(deletedItem)
            {
                if(Time.unscaledTime>=undoDeadline) { deletedItem=null; Set(undoObject,false); }
                else if(undoLabel) undoLabel.text="UNDO  "+Mathf.CeilToInt(undoDeadline-Time.unscaledTime)+"s";
            }
        }
        void Recenter()
        {
            if(!head||!contentRoot||Holding)return;
            var forward=Vector3.ProjectOnPlane(head.transform.forward,Vector3.up);
            if(forward.sqrMagnitude<.01f)forward=Vector3.forward;
            contentRoot.rotation=Quaternion.LookRotation(forward.normalized,Vector3.up);
            contentRoot.position=new Vector3(head.transform.position.x,head.transform.position.y-1.65f,head.transform.position.z);
            Record("recenter");
        }

        /// <summary>Editor-only preview; actual Play always starts at Welcome.</summary>
        public void ShowDesign(int mode)
        {
            if(Application.isPlaying)return;
            editorPreviewMode = mode;
            Set(welcomePanel,false); Set(textPanel,false); Set(stickerPanel,false); Set(reviewPanel,false); Set(successPanel,false); Set(adminPanel,false);
            Set(navigation,true); Set(toolGroup,true); Set(publishObject,false); Set(undoObject,false);
            if(trashVolume)Set(trashVolume.transform.parent.gameObject,true);
            if(mediaCards==null)return;
            foreach(var card in mediaCards){if(!card)continue;Set(card.gameObject,true);Home(card);}
            if(mode==1&&mediaCards.Length>0&&mediaCards[0]&&focusAnchor)
            {var card=mediaCards[0];card.transform.position=focusAnchor.position;card.transform.rotation=focusAnchor.rotation;card.transform.localScale=Vector3.one*inspectScale;}
            if(mode==2)Set(textPanel,true);
            if(mode==3)Set(stickerPanel,true);
            if(stageLabel)stageLabel.text=mode==0?"01 / CHOOSE A MEMORY":"02 / MAKE IT YOURS";
            Hint("Floating memories · T / Text · S / Sticker\nGrip to move. Two grips to resize. Left bin to discard.");
        }
    }
}
