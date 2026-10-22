# Spatial Stories — IP2a Full Concept Restoration
# Spatial Stories — IP2a 完整构思复现

**Zixu Yang · 48939559 · DECO7230**

This iteration restores the spatial interactions in the original Concept Report and refines their clarity using the IP1 findings. The original design was a curved floating album, directly manipulated media, two separate T/S tool spheres, a left-side bin, and a prominent final publishing action.
本次依据原始 Concept Report 复现空间交互，再用 IP1 反馈改善提示和状态。原始设计包含弧形悬浮相册、可直接操控的媒体、独立的 T/S 工具球、左侧垃圾桶，以及醒目的最终发布操作。

## What is preserved and what changes / 保留与修改

| Original idea / 原始构思 | IP1-informed response / 根据 IP1 反馈修改 |
| --- | --- |
| Curved floating photo wall / 弧形悬浮照片墙 | Retain the actual photographs and spatial layout; strengthen hover and yellow selection feedback. / 保留真实照片与空间布局，加强悬停和黄色选中反馈。 |
| Grab, enlarge, inspect and return / 抓取、放大、查看与返回 | Retain direct manipulation and provide clearer selection, inspection and return states. / 保留直接操控，明确选择、查看和返回状态。 |
| Separate T and S tool spheres / 独立的文字与贴纸球 | Keep both spatial tools; add readable names and sticker previews. / 保留两个空间工具，增加可读名称与贴纸预览。 |
| Move, rotate and scale content / 移动、旋转和缩放内容 | Keep these core interactions and test them with XR input. / 保留核心操控，在 XR 输入下继续测试。 |
| Left-side spatial trash bin / 左侧空间垃圾桶 | Use red drop feedback and provide recovery from deletion. / 加入红色投放反馈及删除恢复。 |
| Large final Publish action / 大型最终发布控件 | Use a non-destructive colour, reveal after meaningful editing, and show clear completion feedback. / 改用非破坏性色，在有意义的编辑后出现，并提供明确完成反馈。 |
| Browse-to-edit transition / 浏览到编辑的过渡 | Separate photo movement from tool reveal; add contextual hints and Back/Deselect. / 分开照片移动和工具出现，加入阶段提示及返回／取消选择。 |

IP1 supported retaining floating photos and rotation. Four of five participants completed the full workflow, but all five required prompts. The iteration therefore improves discoverability while preserving the concept.
IP1 对悬浮照片及旋转给予正面反馈。5 人中 4 人完整完成，但所有人都需要提示，因此本次在保留构思的基础上改善功能的可发现性。

**Evidence limits / 证据限制:** IP1 used desktop input. It did not establish Quest comfort, real hand-tracking accuracy or gesture naturalness. P02 Delete/Publish were not recorded; only P01 had a completion time. No new IP2a participant results are included.
IP1 使用电脑输入，不能证明 Quest 舒适度、真实手部追踪精度或手势自然度。P02 的删除／发布未记录，只有 P01 记录了完成时间。本包不包含虚构的 IP2a 参与者结果。

## Import into the current project / 导入当前项目

The update adds new scenes and assets to the existing project. Keep using the project in which Android/OpenXR was already configured. The original scenes remain available for comparison.
更新在现有项目中增加新场景和资源。继续使用已经配置好 Android/OpenXR 的项目，原场景可保留用于对照。

1. Exit Play Mode and save the current scene. / 退出 Play 并保存当前场景。
2. Import the complete package with all included files selected. / 导入整个包，保留所有随包文件的勾选。
3. Enable the built-in **Video** module in **Window → Package Management → Package Manager → Built-in packages → Video → Enable**, if it is not already enabled. Its package ID is `com.unity.modules.video` (version `1.0.0`). The video component needs this module even when only photographs are assigned. Wait for compilation to finish. / 若尚未启用，请在 **Window → Package Management → Package Manager → Built-in packages → Video → Enable** 启用内置 Video 模块。即使暂时只放照片，视频组件编译也需要这个模块。等待编译结束。
4. If Console shows a red error, inspect it before continuing; do not combine selected scripts from different patch versions. / 如果 Console 有红色报错，先处理报错；不要混用不同补丁版本的部分脚本。

The previous missing-method error came from incompatible script versions. This update must be imported as a complete set. It does not require redoing Android/OpenXR configuration.
此前的缺失方法报错来自脚本版本不一致，本次请完整导入。无需重新配置 Android/OpenXR。

**2026-09-15 compile fix:** `ImmersiveStory` now explicitly uses the `IXRSelectInteractable` cancellation overload, avoiding the removed `XRBaseInteractable` overload in XRI 3.6. Reimport this revised package into the same project; it updates the script without overwriting scenes or XR settings. Source/API and syntax checks passed; Unity compilation still requires verification in the editor.
**2026-09-15 编译修复：** `ImmersiveStory` 已明确调用 `IXRSelectInteractable` 取消抓取接口，避免 XRI 3.6 已停用的旧接口。将修正版重新导入同一项目即可更新脚本，不覆盖场景或 XR 设置。已核对接口和语法；仍需在 Unity 中确认实际编译。

New scene paths / 新场景路径:

- `Assets/SpatialStoriesImmersive/Scenes/SpatialStories_Full_Quest.unity`
- `Assets/SpatialStoriesImmersive/Scenes/SpatialStories_Full_Simulator.unity`

Open **Tools → Spatial Stories Full Concept → 1 - Create or open full scenes** after compilation. This creates both native scenes and opens Full Quest; it preserves any full scenes that already exist.
编译结束后，点击 **Tools → Spatial Stories Full Concept → 1 - Create or open full scenes**。此命令创建两份原生场景并打开 Full Quest；已存在的完整场景会保留。

| Menu command / 菜单项 | Purpose / 用途 |
| --- | --- |
| `2 - Open full Quest scene` | Open the Quest scene. / 打开 Quest 场景。 |
| `3 - Open full Simulator scene` | Open the desktop XR simulator scene. / 打开电脑 XR 模拟场景。 |
| `4 - Build full Quest APK` | Build this Quest scene using the existing active Android settings. / 使用现有已启用的 Android 设置构建此 Quest 场景。 |
| `5 - Frame full workspace` | Focus the Scene view. / 聚焦 Scene 视角。 |

The dedicated build explicitly selects the new Quest scene without rewriting the existing Build Profile scene list. / 专用构建明确选择新的 Quest 场景，不重写现有 Build Profile 的场景列表。

## Check the workflow in stages / 分阶段检查流程

**Edit mode / 编辑模式:** inspect the saved floating wall, tool spheres, bin and publishing controls in the scene. Save layout edits outside Play Mode.
在场景中查看已保存的悬浮墙、工具球、垃圾桶和发布控件。在 Play 以外保存布局修改。

Select the new **ImmersiveStory** controller to access **Show full floating album / 悬浮相册**, **Show close editing / 放大编辑**, and **Frame full workspace / 聚焦场景** in its Inspector. / 选中新的 **ImmersiveStory** 控制对象，在 Inspector 使用悬浮相册、放大编辑及聚焦场景按钮。

To rearrange album cards, show the full album, move the cards, click **Capture current album positions / 保存相册排列**, then save the scene. This records their new return positions. / 调整相册排列时，先显示完整相册，再移动卡片，点击 **Capture current album positions / 保存相册排列**，最后保存场景；这样照片返回相册时会使用新位置。

**Desktop simulation / 电脑模拟:** verify the functional sequence: choose a photo → inspect/edit → add text and sticker → move/rotate/scale → delete and recover → publish → reset. Use the simulator’s visible bindings for selecting a controller, moving its ray, trigger and grip; this checks workflow, not real hand comfort.
检查功能顺序：选择照片 → 查看／编辑 → 添加文字和贴纸 → 移动／旋转／缩放 → 删除并恢复 → 发布 → 重置。根据模拟器面板上的实际绑定选择控制器、移动射线、使用扳机与握持键。此阶段检查流程，不能替代真实手感测试。

**Quest / Quest 实机:** test controller ray selection, grip manipulation, two-controller resizing, spatial reach, accidental actions, readable labels, and recovery. Publishing is a local prototype confirmation, not an Instagram upload.
测试控制器射线选择、握持操控、双控制器缩放、空间可达性、误操作、标签可读性及恢复。发布为原型内确认，不会上传 Instagram。

Simulator bindings / 电脑模拟器按键:

| Key / 按键 | Function / 作用 |
| --- | --- |
| `]` / `[` | Select right / left controller. / 选择右／左控制器。 |
| `H` | Select simulated head. / 选择模拟头部。 |
| `Tab` | Cycle selected input. / 轮换当前控制对象。 |
| Arrow keys / 方向键 | Rotate selected input. / 转动当前控制对象。 |
| `W A S D` | Move selected input. / 移动当前控制对象。 |
| `Q E` | Adjust height. / 调整高度。 |
| `T` | Trigger: point at a button and activate it. / 扳机：对准按钮并触发。 |
| Hold `G` / 按住 G | Grip: grab; release G to let go. / 握持：抓取，松开 G 放手。 |
| `X` / `Y` | Toggle controller-actions / input-selection panels. / 展开控制器动作／输入选择面板。 |

Click the Game view and use an English keyboard layout first. Changing the selected input changes which simulated device receives movement and buttons. The virtual text keyboard is operated by the controller pointer and trigger.
先点击 Game 窗口并切换英文输入法。切换控制对象会改变移动与按键作用的模拟设备。虚拟文字键盘使用控制器指向和扳机操作。

## Five-minute participant test / 五分钟参与者测试

**Task / 任务:** “Choose a photo for a Story, inspect it, add text and a sticker, adjust the content, remove one unwanted item, and publish when you are satisfied.”
“选择一张照片制作 Story，查看照片，添加文字和贴纸，调整内容，删除一个不需要的元素，满意后发布。”

| Time / 时间 | Procedure / 流程 |
| --- | --- |
| 0:00–0:30 | Explain pointing, trigger and grip only; begin from reset. / 只说明指向、扳机与握持，使用重置后的状态。 |
| 0:30–3:30 | Give the task and observe. Record wrong actions, hesitation and the exact step where help is needed. / 给任务并观察，记录误操作、犹豫及需要帮助的具体步骤。 |
| 3:30–4:15 | Observe deletion, recovery if needed, and publishing. Record complete/partial/failed plus time. / 观察删除、必要时的恢复及发布，记录完整／部分／失败和用时。 |
| 4:15–5:00 | Ask ease and naturalness (1–5), most confusing action, and one proposed change; reset for next participant. / 询问易用性与自然度（1–5）、最困惑的操作及一项建议，然后重置。 |

Use the existing 10-second help rule: wait for ten seconds without progress, give a neutral hint, then record it using the facilitator prompt control and notes. A functional log supports observation; it does not replace participant ratings or comments. Record simulation and Quest sessions separately. Keep all result fields blank until a real session happens.
沿用 10 秒帮助规则：无进展满 10 秒再给中性提示，并通过主持人提示记录控件及笔记记录。功能日志辅助观察，不能替代参与者评分和原话。电脑模拟与 Quest 测试分开记录，真实测试前不填写结果。

## Scope and verification / 范围与验证

- The original vision targets hand-tracked MR and mentions both photos and videos. The IP1 test used six photographs; it excluded a full keyboard and large sticker catalogue. / 原始愿景是手部追踪 MR，并提到照片与视频；IP1 测试使用六张照片，未包含完整键盘和大型贴纸库。
- The restored version adds a virtual text keyboard as an implementation enhancement; it was not validated in IP1. The delivered controls and media must be assessed from the included implementation. / 复现版本新增虚拟文字键盘，属于本次实现改进，未经 IP1 验证；以本包实际实现的输入与媒体为准。
- The scene builder reuses the six original photographs already in your project. To add your own local video, import it and assign it to the card’s **Media Video Surface → Clip** field. Video decoding and playback require a separate device check. / 场景生成器复用当前项目里的六张原有照片。需要添加自己的本地视频时，导入后指定到卡片的 **Media Video Surface → Clip** 字段；视频解码与播放需单独实机检查。
- The current implementation uses tracked Quest controllers. Hand-tracked pinch input and MR passthrough remain future adaptations of the original vision. / 当前实现使用追踪的 Quest 控制器，手部追踪捏合输入与 MR 透视属于原始愿景的后续适配。
- Static source and asset checks are separate from Unity import, compilation, Play Mode and Quest testing. Manual Unity and Quest verification has not been performed in this development environment. / 源码与资源静态检查不同于 Unity 导入、编译、Play 和 Quest 实测。本开发环境没有完成 Unity 或 Quest 人工验证。

## Source basis and AI acknowledgement / 依据与 AI 使用说明

Source documents: the original Spatial Stories Concept Design Report, IP1 Testing Plan, IP1 Key Findings, and Design Evaluation 1. Preserve their participant data and distinguish observed findings from new implementation choices.
依据文件：Spatial Stories 原始 Concept Design Report、IP1 Testing Plan、IP1 Key Findings 及 Design Evaluation 1。保留原有参与者数据，并区分观察结果与本次新增的实现决策。

ChatGPT/Codex assisted with reviewing the supplied concept and evaluation, designing and writing the Unity restoration, diagnosing earlier script/asset problems, and producing these notes. The student should review and run the project, understand the interactions and acknowledge this assistance accurately in the course submission.
ChatGPT/Codex 协助重读构思与评估、设计并编写 Unity 复现、排查此前脚本与资源问题，以及整理本说明。学生需检查并运行项目、理解交互，并在课程提交中如实说明这些协助。


## Reading and editing captions / 再次编辑文字

After placing a caption, grip it and release it, then open the T sphere to change that caption. The text panel's NEW CAPTION button starts a separate caption. Alternatively, press Trigger while gripping a caption and release the grip to reopen its editor.
放好文字后，抓取再松开该文字，再打开 T 球即可修改这段文字。面板中的 NEW CAPTION 用于新建另一段文字。也可以抓着文字时按扳机，松开 Grip 后打开编辑面板。

## Save both scenes / 两份场景分别保存

Quest and Simulator are separate generated scene assets. Scene layout edits made in one do not automatically change the other. The shared runtime scripts, text/sticker prefabs, and materials are common to both. Both new scenes are generated with the same initial full concept.
Quest 和 Simulator 是两份场景：在其中一份调整布局，不会自动同步到另一份；运行脚本、文字／贴纸预制件和材质则共用。第一次生成时，两份场景使用相同的完整构思。

## Validation in this delivery / 本次检查

C# syntax and cross-script field/method contracts were checked; keyboard action mappings, original photo aspect ratios, API names against installed XRI 3.6 source, archive timestamps/payloads, and preservation of configuration files were checked. Independent code review addressed photo UI vs grip targeting, re-editing captions, simultaneous competing grabs, queued pose restoration and two-hand scaling handover.
已检查 C# 语法和脚本之间的字段／方法引用、键盘动作映射、原照片比例、XRI 3.6 源码接口、压缩包时间戳与内容，以及配置文件的保留。独立代码审查处理了照片点击与抓取目标冲突、文字再次编辑、同时抓取不同对象、坐标恢复和双手缩放接管等问题。

Unity import/compilation, native scene generation, Play Mode and Quest device validation remain to be run in Unity. The scene generator reports errors instead of replacing your original scenes. These source checks do not establish classroom readiness by themselves.
Unity 导入／编译、原生场景生成、Play 和 Quest 实机验证仍需在 Unity 中执行。生成失败时会报告错误，原场景会保留。仅靠源码检查不能认定已经可以直接进行课堂测试。
