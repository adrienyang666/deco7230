# 编辑模式可见场景补丁

基于你上传的“归档.zip”修改。导入到你现在已经设好 Android 的同一个项目，继续使用当前项目。

## 安装

1. 在当前 Unity 项目中退出 Play，按 ⌘S 保存当前场景。
2. 选择 Assets → Import Package → Custom Package，打开 `SpatialStories_Editable_Scene_Patch.unitypackage`。
3. 保持补丁内容全选，点击 Import，等待脚本编译完成。若 Unity 提示重新加载已更新场景，选择 Reload。
4. 点击 Tools → Spatial Stories → Open Quest scene。现在不按 Play 也能看到编辑工作台。
5. 如果视角没有对准，点击 Tools → Spatial Stories → Frame Workspace。

此补丁只有 `Assets/SpatialStoriesVR/` 中的场景、脚本、材质、贴纸预览模型及说明。不含 ProjectSettings、Packages、Assets/XR、Assets/XRI，也不替换 XR Rig 或输入映射。导入不会主动切换构建平台。

官方导入说明：https://docs.unity3d.com/6000.3/Documentation/Manual/AssetPackagesImport.html

## 不戴 Quest，查看和编辑场景

左侧 Hierarchy → 展开 **Spatial Stories - EDITABLE WORKSPACE**。

- **Room - editable geometry**：地面、墙面。
- **Story workspace - editable layout**：桌子、相册、照片编辑板、文字和贴纸工具、托盘、垃圾箱。

选中 **Spatial Stories - EDITABLE WORKSPACE**，Inspector 里有：

- **Show Welcome / 欢迎页**
- **Show Photo Album / 相册**
- **Show Editing Desk / 编辑工作台**
- **Frame Workspace / 聚焦工作台**

默认保存的是编辑工作台视图。不同阶段的对象都已在场景中保存，未显示的对象会在 Hierarchy 中呈灰色；可用这些按钮切换查看。

移动、旋转或缩放各个子物体后按 ⌘S，可以保存布局。材质在 `Assets/SpatialStoriesVR/SceneAssets/`，可在 Inspector 修改颜色。修改工具面板等物体时，优先移动其父对象，使文字和按钮一起移动。

## 运行时的行为

按 Play 后始终从欢迎页开始。程序直接使用这些保存好的场景对象，不再重新创建一套工作台覆盖它们。编辑板的动画终点和托盘生成点会跟随你保存的布局。

在电脑上运行：Tools → Spatial Stories → Open computer simulator → Play。工作台模型已保存，但参与者新增的文字和贴纸仍在任务进行时创建；这是测试内容，不是场景丢失。

连接 Quest 后仍按原来的 Android → Run Device → Build And Run 流程。更新场景后需要重新构建才能在头显里看到变化。现有 Android/OpenXR 配置保留。

调整照片板整体 Transform 的位置、旋转和比例会作用于放置区域。单独更改照片网格本身的宽高、垃圾箱内部边框或新增工具，需要同步调整交互范围／逻辑；单纯改模型不会自动改业务规则。

## 已完成的检查与限制

- 对比上传版本与修改版本的 ProjectSettings、Packages、Assets/XR、Assets/XRI 文件字节，保持一致。
- 检查两个场景的内部引用、资源 GUID、材质、按钮永久绑定与父子结构。
- 检查新改 C# 文件语法。
- 本环境未执行 Unity 导入、Unity 编译、运行或 Quest 实机测试。此前截图中的 Build Succeeded 对应补丁前的版本，不代表本补丁已通过构建。

导入后检查 Console 是否有红色错误，再测试：BEGIN → 选照片 → 添加文字／贴纸 → 抓取放置 → 删除／撤销 → 预览／发布 → 重置。

本补丁的代码、场景序列化、编辑工具和验证脚本由 ChatGPT/Codex 协助编写；没有生成或修改参与者测试结果。
