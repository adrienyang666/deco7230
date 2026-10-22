# 在本地 Codex 继续这个 Unity 项目

**2026-10-15 本机更新：**已在 `/Users/yangzixu/Downloads/SpatialStories_IP2b_Quest_Project 2` 合入 r4，并安装、验证 CoplayDev MCP for Unity v10.0.0 的 STDIO 连接（实例 `c12fd06b`）。Python 与 uv 依赖检查已通过，无须重新安装或重复导入。当前 Codex 任务的工作目录仍在 Documents/Codex；后续命令需显式指定 Unity 工程。当前验证结果、备份与构建注意事项见 `CODEX_HANDOFF.md` 顶部。下文为原始迁移说明。

## 已经有一个能运行的 Unity 工程

1. 在 Unity 保存场景并退出 Play。找到当前工程根目录，即包含 Assets、Packages、ProjectSettings 的文件夹。
2. 解压 `SpatialStories_Codex_Handoff.zip`，把里面的 AGENTS.md、CODEX_HANDOFF.md、CODEX_START_HERE.md 和 Codex_Handoff_Reference 文件夹放到该根目录。不要放进 Assets。若已有 AGENTS.md，先保留原件，再让 Codex 合并约定。
3. 在 Mac 桌面应用选择 Codex，新建本地项目并选择这个同一个根目录。已有项目可用项目菜单 Edit project → Add folder，并将 Unity 根目录设为 Make primary。
4. 新会话选择 This computer / Local，直接使用当前目录。若希望连接 Unity 当前实例，先不要另开 Worktree，否则 Unity 与 Codex 可能指向不同副本。
5. 在这个本地项目中新建会话，粘贴下面的接手指令。仅打开旧云端聊天不会自动取得 Mac 工程访问权限。

## 第一条消息

```text
这是我正在 Unity 中打开的 Spatial Stories IP2b 工程。请先读取根目录的 AGENTS.md、CODEX_HANDOFF.md 和 IP2b_START_HERE.md。
先确认你访问的是这个本地工程，核对 Unity 版本、当前源码、Git 状态和已有改动。检查主脚本是否包含 Quest 启动修复 r4；若没有，先与 Codex_Handoff_Reference/ImmersiveStory.cs 比较，再保留本地改动合并必要修复。
接下来帮我配置本地 Unity Editor 连接：先检查已安装的 MCP/桥接和实际可用工具，再按当前作者文档配置兼容连接。连接后只读确认项目路径、场景和 Console，确保连的是我当前工程，然后继续验证启动与功能。
请在这个工程内继续修改，保留悬浮照片墙、T/S球、抠图、图层和发布流程。电脑主要流程已由我跑通；r4 的 Unity 编译和 Quest 真机验证尚未确认。不要把源码检查写成真机测试通过。完成后更新交接文件。
```

## 如果希望从完整包重新开始

`SpatialStories_IP2b_Quest_Project.zip` 已包含当前源码、素材、Packages、ProjectSettings、测试材料与交接文件。把它解压到单独目录，在 Unity Hub 添加该目录，并在 Codex 选择相同目录。它是完整可重新导入的项目源码包，不包含 Unity 安装器、可重建的导入缓存或已构建 APK。

如果当前本地工程已有额外修改，优先使用小交接包；不要直接用完整包覆盖它。

## 文件访问、聊天和 Unity 连接

- 本地 Codex 获得项目目录访问后，可以原地编辑脚本；Unity 后续刷新／编译这些改动，无须每次重新下载工程。
- 直接读取场景对象、Console、控制 Play 或运行编辑器任务，还需要可用的 Unity Editor 工具连接。
- 可评估第三方开源 MCP for Unity（CoplayDev/unity-mcp）。作者声明支持 Codex 和 Unity 2021.3 LTS–6.x；这不是 Unity 官方插件，本工程尚未安装／实测。让本地 Codex 阅读当前安装文档并检查已安装配置，再选择合适版本，不要照抄过期安装参数或覆盖其他 MCP 配置。
- 这里没有把聊天逐条导入 Codex 的内部历史或记忆数据库。AGENTS.md 是项目约定，CODEX_HANDOFF.md 是当前上下文。官方说明也建议把必需指导放在项目文档中。
- 如你的版本提供 Settings → Personalization → Enable Codex memories，可开启后续本地记忆；这不等于导入此前 ChatGPT 的所有记忆。

## 已核对的来源（2026-10-15）

- OpenAI Projects and chats：https://learn.chatgpt.com/docs/projects
- OpenAI Codex environments：https://learn.chatgpt.com/docs/environments/modes
- OpenAI Memories：https://learn.chatgpt.com/docs/customization/memories
- 第三方桥接作者仓库：https://github.com/CoplayDev/unity-mcp

界面标签可能随版本变化；核心是“本地执行 + Unity 同一根目录 + 从该项目新开会话”。
