# 项目状态说明（供组员协作使用）

项目：VirtualRealityCourseProject（《虚拟现实技术》课程大作业）  
引擎：Unity 2022.3 LTS（Universal 3D / URP 模板）

> 本文档主要说明：当前进度、已有代码结构、如何在本项目基础上继续开发和协作。

---

## 1. 当前整体进度概览

- ✅ Unity 工程创建完成，使用 URP 渲染管线。
- ✅ 主场景 `MainScene` 已建立，用于演示与开发。
- ✅ 导航网格（NavMesh）已经在测试地面（Plane）上烘焙完成。
- ✅ 导航与路径系统已搭建：
  - 支持通过 UI 按钮选择一批角色（NavMeshAgent）。
  - 支持“Set Start → 在地面点击 → Set Goal+Apply → 在地面点击”来指定起点和终点。
  - 能为所有选中角色计算整条路径并让他们沿路径移动。
- ✅ 基础 UI 已搭建：
  - 左上角有 `Set Start` / `Select All` / `Set Goal+Apply` 按钮。
  - 按钮和脚本 (`SimpleUI` → `PathManager`) 绑定已经打通。
- ✅ CrowdSpawner / WeatherManager / CameraCapture 已实现“安全默认值”：
  - 即使暂时没有正式人物 prefab，也能自动生成占位胶囊并挂好 NavMeshAgent + AgentController；
  - 天气按钮会使用基础光照/雾/环境光实现“晴/夜/雾/雪”的对比，后续只需替换 Skybox/粒子即可；
  - `Capture PNG` 按钮调用 `CameraCapture`，默认把截图保存到“图片/VRProjectCaptures”。
- ✅ C + D-dev 相关脚本已创建并编译通过，包含：
  - `AgentController` / `PathManager` / `CrowdSpawner` / `WeatherManager` / `CameraCapture` / `SimpleUI` / `SelfTest` / `RequirementChecklist` / `QuickSetup`。
- ⏳ 目前使用的角色是“胶囊体占位模型”，还没有换成真实人物模型和动画。
- ⏳ 场景仍然是测试平面，尚未导入真实的城市/街道/广场等环境。
- ⏳ 天气系统（晴/夜/雾/雨雪）、喷泉/火焰粒子、截图导出等功能脚本已就绪，但还没有接上真实素材和 UI 按钮。

整体估计：C + D-dev 框架完成度 ~60–70%；整套大作业整体完成度 ~20–30%。

---

## 2. 当前场景结构（`MainScene`）

- `MainScene`
  - `Main Camera`：主相机，用于 Game 视图。
  - `Directional Light` / `Global Volume`：基础光照和后处理。
  - `Canvas`：工程模板自带的 UI，可暂时忽略。
  - `Plane`：测试用地面，已设置为 `Layer = Ground`，并挂有 `NavMeshSurface` 组件，用于烘焙 NavMesh。
  - `GameSystem`：核心逻辑挂点，包含：
    - `PathManager`：路径管理与点击取点。
    - `WeatherManager`：天气切换逻辑（后续接 Skybox/粒子）。
    - `CrowdSpawner`：人群生成逻辑（当前可暂时禁用，后续配合真实角色使用）。
    - `SimpleUI`：UI → 逻辑的转发脚本。
    - `SelfTest`：自动测试路径的脚本（默认建议禁用，避免干扰手动操作）。
    - `RequirementChecklist`：左上角检查面板（目前为了不挡 UI 已禁用，需要时可以重新启用）。
  - `CaptureCamera`：截图用相机，挂有 `CameraCapture` 脚本。
  - `SpawnPoints`：一组出生点（`SpawnPoint_0`…），供 CrowdSpawner 使用。
  - 若干 `Capsule_*`：目前用于测试的胶囊体角色，每个挂有 `NavMeshAgent` 和 `AgentController`。
  - `VRProjectCanvas`：操作界面的 Canvas，包含：
    - `Set Start` 按钮
    - `Select All` 按钮
    - `Set Goal+Apply` 按钮
    - 预留的天气按钮、截图按钮（`Sunny/Night/Foggy/Snow/Capture PNG` 等）
  - `EventSystem`：Unity UGUI 必备事件系统。

> 注意：后续导入正式环境和人物时，不建议删除 `GameSystem` / `VRProjectCanvas` / `PathManager` / `SimpleUI` / `CameraCapture`，只需要替换地形、角色 prefab 和材质即可。

---

## 3. 核心脚本与职责

### 3.1 `PathManager`（路径管理）

- 挂载位置：`GameSystem`。
- 关键职责：
  - 从相机发射 Ray，基于 `groundMask`（目前只包含 `Ground` 层）拾取地面点击位置。
  - 用 `NavMesh.SamplePosition` 将点击点投射到最近的 NavMesh 上。
  - 在 `SetStartFromClick` 后，等待下一次鼠标左键，将其作为起点；
  - 在 `SetGoalFromClickAndApply` 后，再等待一次鼠标左键，将其作为终点；
  - 对当前选中 agent 列表逐个计算从“当前位置”到“终点”的路径，并通过 `NavMeshAgent.SetPath` 或 `SetDestination` 下发；
  - 使用 `LineRenderer`（`PathLine` prefab）绘制路径折线（只绘制第一个 agent 的路径，避免过于杂乱）。

### 3.2 `SimpleUI`（UI 转发）

- 挂载位置：`GameSystem`。
- 关键方法：
  - `SelectAllAgents()`：
    - 优先使用 `CrowdSpawner.SpawnedAgents`；
    - 若 CrowdSpawner 为 null 或列表为空，则自动 `FindObjectsOfType<NavMeshAgent>()`，将场景所有 agent 加入选中列表。
  - `SetStartFromClick()`：
    - 日志：`SetStartFromClick: waiting for ground click.`；
    - 调用 `PathManager.SetStartFromClick()` 进入“等待下一次点击”的状态。
  - `SetGoalFromClickAndApply()`：
    - 日志：`SetGoalFromClickAndApply: waiting for ground click.`；
    - 调用 `PathManager.SetGoalFromClickAndApply(true)`，在下一次点击时为所有选中 agent 下发路径。

### 3.3 `AgentController`（代理控制与跳跃逻辑）

- 挂载位置：每个角色/胶囊体上。
- 目前负责：
  - 从 `NavMeshAgent.velocity.magnitude` 计算当前运动速度，将其映射到 Animator 的 `Speed` 参数（0=Idle, 1=Walk, 2=Run）。
  - 在 `isOnOffMeshLink` 时触发 Jump 动画并沿抛物线移动（后续需要 A 在环境中布 Off‑Mesh Link 才能看到效果）。

### 3.4 `CrowdSpawner`（人群生成）

- 挂载位置：`GameSystem`。
- 功能概览：
  - 在若干 `SpawnPoint` 位置随机生成 `count` 个角色（`characterPrefabs` 列表中的 prefab）。
  - 即使 `characterPrefabs` 为空，也会自动创建占位胶囊，保证 Demo 随时可运行。
  - 自动为每个实例挂 `NavMeshAgent`（配置速度区间）与 `AgentController`。
  - 将所有生成的 agent 留在 `SpawnedAgents` 列表中，供 `SimpleUI.SelectAllAgents` 使用。
  - 支持 `Clear()` / `Spawn()` 手动刷新，方便 A/B 在调试阶段反复生成。

### 3.5 `WeatherManager`、`CameraCapture` 等

- `WeatherManager`
  - 现在已经内建了“晴/夜/雾/雪”四种基础配置：调整太阳强度、环境光、雾颜色/密度、雨/雪粒子开关。
  - 若 `skySunny/skyNight/skyFog` 为 null，则使用当前默认 Skybox，保证按钮可以演示；后续只需将 A 提供的材质拖入即可。
- `CameraCapture`
  - 挂在 `CaptureCamera` 上，`SimpleUI.Capture()` 会调用 `CaptureOnce()`，默认保存到用户的“图片/VRProjectCaptures”目录，文件名为 `cap_时间戳.png`。

---

## 4. 当前使用说明（操作胶囊移动的规范流程）

- **路径操作（C 核心）**

  1. 点击 `Select All` → Console 会出现 `SelectAllAgents: current agents = X`；
  2. 点击 `Set Start` → Console 提示等待点击 → 在 `Game` 视图的地面上左键一次；
  3. 点击 `Set Goal+Apply` → Console 提示等待点击 → 在地面上另一处左键一次；
  4. 地面出现路径折线，所有 agent 沿路径移动。

- **天气/截图操作（D-dev 关联）**

  - `Sunny / Night / Foggy / Snow`：直接调用 `WeatherManager.SetWeather`，即使暂时没有 Skybox/粒子也能看到光照/雾变化；
   - `Capture PNG`：调用 `CameraCapture.CaptureOnce()`，在 Console 输出保存路径，图片保存在“图片/VRProjectCaptures”目录。

---

## 5. 后续工作建议（按角色划分）

### 5.1 A：环境/渲染

- 从 Asset Store 挑选并导入城市/街道/广场/商场类免费场景，放到 `Assets/Art/Environment`。
- 将可行走地面设置为 `Layer = Ground`，并为其添加/配置 `NavMeshSurface`（可参考当前 Plane 的设置）。
- 根据实际地形调整 `SpawnPoints` 位置（供 CrowdSpawner 使用）。
- 增加点光源/聚光灯、雾参数、夜景/白天 Skybox 等。

### 5.2 B：人物/动画

- 从 Mixamo 等处下载 3 类人物模型和 walk/run/jump 动作（`FBX for Unity`）。
- 设置 Rig = Humanoid；给每类角色配置 Animator Controller（BlendTree + Jump）。
- 将胶囊体替换为真实角色 prefab，并挂上 `NavMeshAgent` + `AgentController`。

### 5.3 C：导航/路径/人群（当前你负责）

- 完善 `CrowdSpawner`，使用 B 提供的 prefab 列表在 SpawnPoints 批量生成 ≥5 人。
- 与 B 协调动画速度和 `NavMeshAgent.speed`，减少滑步。
- 在 A 提供的环境中验证复杂路径（拐弯、狭窄通道）、Off‑Mesh Link 跳跃等。

### 5.4 D-dev/D-prod：UI/相机/演示

- 将天气按钮 `Sunny/Night/Foggy/Snow` 接到 `WeatherManager.SetWeather` 上，并由 A 提供对应 Skybox/粒子。
- 将 `Capture PNG` 接到 `CameraCapture.CaptureOnce`，整理截图存放目录和分辨率。
- 布置多个演示相机机位，准备展示路径/人群/天气切换的 Demo 动画；
- D-prod 负责录屏、截图、PPT 和汇报。

---

## 6. 注意事项（协作约定）

- 不要随意删除或重命名以下对象/脚本：`GameSystem`、`VRProjectCanvas`、`PathManager`、`SimpleUI`、`CameraCapture`。
- 任何需要新按钮/新操作时，优先在 `SimpleUI` 中添加方法，再在 Canvas 按钮的 OnClick 里绑定对应方法。
- 修改 NavMesh 或 Ground 层时，请通知负责导航的同学重新 Bake NavMesh 并做一次简单回归测试。

---

（本文档会随项目进度更新，供组员快速了解当前状态与后续分工。）

---

## 7. 接口对接指南（你= C + D-dev，与 A/B/D-prod 如何配合）

### 7.1 和 A（环境/渲染）如何对接

- A 需要遵守的约定：
  - 所有可行走地面对象（道路、广场、室内地板等）统一设为 `Layer = Ground`。
  - 地面必须带 Collider（BoxCollider/MeshCollider），否则点击和 NavMesh 都无法正常工作。
  - 至少在场景中提供一块“平坦区域”，方便放 `SpawnPoints` 和手动测试。
- 你（C + D-dev）需要的接口：
  - A 调整或新增地形后，**通知你重新 Bake NavMesh**（选中对应地面对象上的 `NavMeshSurface` → `Bake`）。
  - 如需新增可跳跃区域，让 A 在场景中留出明显的高度差/间隙，你来放置 Off‑Mesh Link 或在文档中记录链接位置。

**简单记忆：A 决定“哪儿是地面、哪儿能走”，你负责“在这些地方烘焙 NavMesh 并让人走起来”。**

### 7.2 和 B（人物/动画）如何对接

- B 需要遵守的约定：
  - 每个角色 prefab 必须至少包含：
    - `Animator`（挂好 Animator Controller，其中包含 `Speed`（float）和 `Jump`（Trigger）两个参数）；
    - `SkinnedMeshRenderer` 或 MeshRenderer（人物模型本体）；
    - **不必**手动加 `NavMeshAgent` 和 `AgentController`，你这边可以自动补。
  - 提供一个“动画速度标定表”，说明：
    - walk 动画在 1.0x 播放速度下，大约走多少 m/s；
    - run 动画在 1.0x 播放速度下，大约跑多少 m/s。
- 你（C + D-dev）会做的事情：
  - 在 `CrowdSpawner` 中接入 B 给出的 prefab 列表，生成 ≥5 人；
  - 自动为生成出来的每个角色挂 `NavMeshAgent` 和 `AgentController`；
  - 将 `NavMeshAgent.speed` 调整到 B 提供的标定速度附近，必要时微调 `Animator.speed`，减少滑步。

**简单记忆：B 决定“人物长什么样、动作是什么”，你负责“让这些人物沿路径走/跑/跳出去，而且不滑步”。**

### 7.3 和 D-prod（UI/演示/汇报）如何对接

- D-prod 主要负责：
  - 美化 UI 布局和文字说明，但**不改按钮的 OnClick 绑定**；
  - 放置演示机位、录制演示视频、截取关键帧；
  - 整理 PPT 和汇报内容。
- 你（C + D-dev）需要提供：
  - 稳定的 `SimpleUI` API（已完成）：`SelectAllAgents / SetStartFromClick / SetGoalFromClickAndApply / SetWeatherXxx / Capture`；
  - 对应的按钮 prefab 或 Canvas（`VRProjectCanvas`），并在文档中注明每个按钮的用途和操作流程；
  - 路径演示脚本（例如“先选全体 → 设起点 → 穿过某个路段 → 到达终点”），供 D-prod 按脚本录制。

**简单记忆：D-prod 决定“怎么展示”，你负责“代码层的功能稳定、按钮可用、日志清晰”。**

---

## 8. 若让 C + D-dev 接近 100% 完成，队友需要提前确认什么？

你可以把下面这段发给组友，让他们知道“你需要他们给出哪些前置条件”，自己只负责填空。

### 8.1 你不需要他们提前确定的东西

- 不需要 A 一开始就选好最终场景包，只要承诺：**最后一定会有一块 Ground 层的地面，可以烘焙 NavMesh**。
- 不需要 B 一开始就确定最终美术风格，只要承诺：**会交付 3 个符合 Humanoid 规范的角色 prefab + 对应 Animator Controller**。
- 不需要一开始就有“完美的”天空盒和粒子，只要承诺：**会提供至少 1 套白天 Skybox + 1 套夜晚（或暗色）Skybox + 1 套雾天 Skybox（可选）+ 雨/雪粒子 prefab**。

也就是说：你可以先用占位资源把 C + D-dev 的脚本全部写好，接口固定，等他们资源一到，只要在 Inspector 里把字段拖进去即可。

### 8.2 你需要他们给出的“最小信息”

1. **环境侧（A）最小信息：**
   - 哪些 GameObject 代表地面（需要挂 `NavMeshSurface`）；
   - 需要保证这些地面在 `Ground` 层且有 Collider；
   - 至少告诉你一个“演示区域”的坐标范围（方便放 SpawnPoints 和演示相机）。

2. **角色侧（B）最小信息：**
   - 3 个角色 prefab 的路径（例如：`Assets/Art/Characters/Char_Male.prefab` 等）；
   - 每个角色对应的 Animator Controller 名称和参数约定（`Speed`、`Jump` 不可改名）；
   - walk / run 动画的大致速度（比如 walk ≈ 1.4 m/s, run ≈ 3.0 m/s）。

3. **天气/特效侧（A 或 D-prod）最小信息：**
   - 至少 2–3 个 Skybox 材质（白天/夜晚/雾天），放在 `Assets/Art/Environment/Skyboxes`；
   - 雨/雪的 ParticleSystem prefab 放在 `Assets/Art/VFX`；
   - 你可以据此在 `WeatherManager` 的 Inspector 上直接拖拽引用，不必再改代码。

4. **截图/演示侧（D-prod）最小信息：**
   - 想要的截图分辨率（例如 1920×1080）、输出目录（默认用“图片/VRProjectCaptures”即可）； 
   - 计划需要几个固定机位（广场视角、街道视角、喷泉视角等），你可以在场景里放好相机/虚拟相机，并让 `CameraCapture` 针对这些机位工作。

### 8.3 在这些前提下，你能做到的“接近 100% 的 C + D-dev”

在上述“最小信息”明确之后，你可以完全负责并基本定稿的内容包括：

- 根据 A 的地形统一烘焙 NavMesh + 放置 SpawnPoints；
- 按 B 提供的 prefab 列表，在 `CrowdSpawner` 中批量生成 ≥5 人，并通过 `Select All` / 路径系统控制其行进；
- 使用 B 的 Animator 和速度标定，调整 `AgentController` 以减小滑步、实现走/跑/跳切换；
- 基于 A 的 Skybox/粒子实现 `WeatherManager.SetWeather(Sunny/Night/Foggy/Snow)` 的完整逻辑，并接上 UI 按钮；
- 基于 D-prod 的需求，实现 `CameraCapture` 的截图参数和 UI 调用；
- 保持所有接口稳定，使得 A/B/D-prod 后期只需要在 Inspector 里填资源，而不必再改脚本。

到那时，C + D-dev 的“代码工作”就可以视为接近 100% 完成，其他同学主要在资源和演示层面工作，你只需要偶尔帮忙调 Inspector 和 Bake NavMesh 即可。
