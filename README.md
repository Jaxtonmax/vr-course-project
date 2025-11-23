# VR Course Project · Team Guide

Unity 2022.3 LTS · URP  
Repo: https://github.com/Jaxtonmax/vr-course-project

主要分工：A=环境/渲染 · B=人物/动画 · C=导航/路径/UI · D-prod=UI/演示/PPT。  
详细实时进度见 `Assets/Docs/ProjectStatus.md`，本文告诉所有组员“文件在哪、接口怎么对、如何协作提交”。

---

## 1. 目录结构 & 文件用途

| 路径 | 说明 |
|------|------|
| `Assets/Scenes/MainScene.unity` | 集成场景：Plane 地面、NavMesh、GameSystem、UI。所有演示/联调在此进行。 |
| `Assets/Scripts/Core/*` | 核心脚本（PathManager、CrowdSpawner、AgentController、WeatherManager、CameraCapture、SimpleUI 等）。不要随意改接口。 |
| `Assets/Scripts/Editor/QuickSetup.cs` | 一键接线工具。若场景乱了，运行 `Tools → VRProject → Quick Setup` 重建。 |
| `Assets/Docs/ProjectStatus.md` | 实时文档，记录进度/接口/TODO。更新工作优先写这里。 |
| `Assets/Art/Environment` | A 的环境资源，地面必须 `Layer=Ground` 并有 Collider。 |
| `Assets/Art/Characters` | B 的角色/动画资源，prefab 要配好 Animator（`Speed/Jump`）。 |
| `Assets/Art/VFX` | 天气/粒子等资源。 |
| `Assets/Prefabs` | 统一存放 prefab（PathLine、角色、UI 等）。 |

---

## 2. 角色操作指南

### A · 环境/渲染
1. 资源导入 `Assets/Art/Environment`，可行走地面设 Layer=Ground 并加 Collider。  
2. 在地面对象上挂 `NavMeshSurface` 并 Bake（或通知 C）。  
3. 需要跳跃演示时，预留高度差/缝隙并告知 C。  
4. 提供晴/夜/雾 Skybox 及雨/雪粒子，C 在 WeatherManager Inspector 中引用。

### B · 人物/动画
1. 交付 3 类 Humanoid 角色 prefab（含材质/贴图），放 `Assets/Art/Characters`。  
2. Animator Controller 参数固定：`Speed`(float)、`Jump`(Trigger)。  
3. 记录 walk/run 动画速度（m/s），写入 ProjectStatus。  
4. 将 prefab 路径告知 C，CrowdSpawner 自动批量生成并挂 NavMeshAgent/AgentController。

### C · 导航/路径/UI
1. 得到 A/B 资源后替换地面/角色 → Bake NavMesh → 用 “Select All → Set Start → Set Goal+Apply” 验证。  
2. 场景出问题可运行 Quick Setup 重新接线。  
3. CrowdSpawner / Weather / CameraCapture 拥有默认逻辑，没资源也能演示；后续只需拖入新资源。  
4. 接口细节见 ProjectStatus 第 7/8 节。

### D-prod · UI/演示
1. 可调整 UI 布局，但按钮 OnClick 必须调用 SimpleUI。  
2. 根据 ProjectStatus 提供的脚本布置相机、录屏、截图。  
3. `Capture PNG` 按钮会输出到 “图片/VRProjectCaptures”。  
4. 新展示需求请先和 C 对齐。

---

## 3. Git & GitHub 协作

### 3.1 克隆
```bash
git clone https://github.com/Jaxtonmax/vr-course-project.git
cd vr-course-project/VirtualRealityCourseProject
```
> Unity 版本：2022.3 LTS（URP）。

### 3.2 日常提交
```bash
git pull
git status
git add <文件>  # 或 git add .
git commit -m "描述改动"
git push
```

### 3.3 资源较大时
- 已启用 Git LFS，fbx/png/prefab/unity/anim/controller 等会自动走 LFS。  
- 若单个包 >1~2GB，导出 unitypackage + 网盘链接，并在 ProjectStatus 写明。

### 3.4 分支 / 合并
1. 新需求建议开分支：
   ```bash
   git checkout -b feature-env-street
   # 修改…
   git commit -m "feat(env): import new street pack"
   git push origin feature-env-street
   ```
2. 在 GitHub 点击 “Compare & pull request”，写改动说明并 @ 相关同学。  
3. 通过后 Merge；本地执行 `git checkout master && git pull`。  
4. 如遇冲突：先 `git pull origin master`，解决后 `git add` → `git commit` → `git push origin feature-xxx --force-with-lease`。

### 3.5 约定
- 修改 `MainScene` 前务必 `git pull`，避免覆盖他人。  
- Quick Setup 重建接线后，确认运行正常再提交。  
- 不要提交 Library/Temp/Builds 等目录（.gitignore 已忽略）。

---

## 4. 进度同步
1. 每次阶段性工作完成后更新 `Assets/Docs/ProjectStatus.md`。  
2. Commit message 明确，例如 `feat(character): add female prefab`、`feat(weather): hook sunny/night buttons`。  
3. Push 后在群里说明“做了什么 + 是否需要 Bake”。

---

## 5. 常见问题
- 按钮无响应：顺序为 `Select All → Set Start → (Game 视图点地面) → Set Goal+Apply → (再点地面)`。  
- Agent 不动：检查地面 Layer、NavMesh 是否 Bake，Console 是否提示 `failed to pick NavMesh point`。  
- CrowdSpawner 提示 spawn point 不存在：手动放 SpawnPoint_* 或删除 SpawnPoints 让脚本自动创建。  
- Git push 需要 Token：用 GitHub Personal Access Token；若文件过大，确认 LFS 生效或改成 unitypackage+外链。

---

只要遵循以上流程，团队成员就能专注在各自的资源/功能上，C 的底层接口已准备妥当，拖入资源即可使用。如需修改接口或增加功能，请先在 `ProjectStatus.md` 和群里同步。
