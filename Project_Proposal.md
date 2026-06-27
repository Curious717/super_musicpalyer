# Super Music Player — 项目方案文档

> **项目地址**: https://github.com/Curious717/super_musicpalyer  
> **开发模式**: RDD（README驱动开发）+ TDD（测试驱动开发）  
> **AI 辅助工具**: Claude Code

---

## 1. 项目背景

在日常学习和工作中，人们经常需要在本地播放 MP3 音乐文件。市面上的音乐播放器功能虽然丰富，但往往体积庞大、启动缓慢，且包含大量非核心功能（如社交、直播、广告等）。本项目旨在开发一款**轻量级、高颜值、专注于本地 MP3 播放**的 Windows 桌面应用程序，提供简洁流畅的音乐播放体验。

本项目是《Windows程序设计》课程的期末综合作业，同时也是**AI 辅助编程（Claude Code）实践**的载体，用于探索人机协作开发的新范式。

---

## 2. 功能需求分析

### 2.1 核心功能

| 编号 | 功能 | 描述 | 优先级 |
|------|------|------|--------|
| F1 | 文件导入 | 支持打开单个/多个 MP3 文件，支持导入整个文件夹 | P0 |
| F2 | 拖拽添加 | 支持拖拽 MP3 文件到播放列表 | P1 |
| F3 | 音频播放 | 播放/暂停/停止 MP3 音频文件 | P0 |
| F4 | 进度控制 | 显示播放进度条，支持拖拽定位 | P0 |
| F5 | 音量控制 | 音量滑块调节（0%~100%） | P0 |
| F6 | 播放列表 | 显示歌曲列表（序号、标题、歌手、时长） | P0 |
| F7 | 列表管理 | 移除选中歌曲、清空列表、双击播放 | P0 |
| F8 | 播放模式 | 顺序播放、随机播放、单曲循环、列表循环 | P1 |
| F9 | 元数据读取 | 读取 MP3 文件的 ID3 标签（标题、歌手、专辑） | P0 |
| F10 | 键盘快捷键 | 空格键播放/暂停，左右方向键切歌 | P2 |

### 2.2 非功能需求

- **性能**：播放响应延迟 < 200ms，UI 操作流畅不卡顿
- **兼容性**：支持 Windows 10/11，.NET 9.0 运行时
- **可用性**：界面简洁直观，符合用户操作习惯
- **美观性**：采用 Tangerine Capital 设计风格，暖橘色调

---

## 3. 技术选型理由

| 技术 | 选型 | 理由 |
|------|------|------|
| 开发框架 | .NET 9.0 + Windows Forms | 课程教学框架，成熟稳定，适合桌面工具快速开发 |
| 音频播放 | NAudio 2.3.0 | 开源、轻量、API 设计优秀，支持 MP3/WAV/FLAC 等多种格式 |
| ID3 标签 | TagLibSharp 2.3.0 | 业界标准的音频元数据读取库，支持 ID3v1/v2 |
| UI 设计 | Tangerine Capital | 暖橘色 + 奶油白配色，杂志排版风格，区别于传统冷色调播放器 |
| 版本控制 | Git + GitHub | 代码托管与版本管理 |
| AI 辅助 | Claude Code | 代码生成、调试、文档撰写 |

### 技术对比分析

**NAudio vs Windows Media Player COM 组件**：
- NAudio 是纯 .NET 实现，无需依赖 COM 组件，部署简单
- NAudio 提供更精细的音频控制（逐字节音量调节、位置跳转等）
- NAudio 社区活跃，文档完善，且完全开源

**WinForm vs WPF**：
- WinForm 学习曲线平缓，与课程教学内容一致
- 对于音频播放器这类功能型应用，WinForm 完全满足需求
- WinForm 支持 GDI+ 自定义绘制，可实现美观的 UI

---

## 4. 系统架构设计

### 4.1 整体架构

```
┌─────────────────────────────────────────────┐
│                  MainForm                    │  ← UI 层
│         (Tangerine Capital 风格界面)          │
├─────────────────────────────────────────────┤
│  PlaylistManager    │    AudioPlayer         │  ← 业务逻辑层
│  (播放列表管理)      │    (NAudio 封装)       │
├─────────────────────────────────────────────┤
│  Track              │    MetadataReader      │  ← 数据模型层
│  (歌曲模型)          │    (ID3 标签读取)       │
├─────────────────────────────────────────────┤
│  NAudio 2.3.0       │    TagLibSharp 2.3.0   │  ← 第三方库层
└─────────────────────────────────────────────┘
```

### 4.2 核心类职责

| 类名 | 职责 | 设计模式 |
|------|------|---------|
| `Track` | 歌曲数据模型，封装文件路径、标题、歌手、时长等 | 数据模型 |
| `MetadataReader` | 读取 ID3 标签，构造 Track 对象 | 工具类 |
| `PlaylistManager` | 管理播放列表的增删改查与播放导航 | 观察者模式（事件通知） |
| `AudioPlayer` | 封装 NAudio，提供播放/暂停/停止/定位/音量控制 | 外观模式 |
| `MainForm` | 主窗体 UI，协调用户交互与业务逻辑 | MVC-Controller |
| `RoundedButton` | 自定义圆角按钮控件（Tangerine Capital 风格） | 自定义控件 |

### 4.3 设计原则

- **单一职责**：AudioPlayer 只负责音频播放，PlaylistManager 只负责列表管理
- **依赖倒置**：MainForm 依赖抽象事件接口，而非具体实现
- **开闭原则**：通过事件机制扩展功能，不修改已有类

---

## 5. 开发环境说明

| 项目 | 版本/信息 |
|------|----------|
| 操作系统 | Windows 11 Home China |
| IDE | Visual Studio 2022 / Cursor |
| .NET SDK | 9.0.312 |
| 语言 | C# 13.0 |
| NuGet 包 | NAudio 2.3.0, TagLibSharp 2.3.0 |
| AI 工具 | Claude Code（Anthropic） |
| 版本控制 | Git + GitHub |

---

## 6. 项目结构

```
super_musicpalyer/
├── SuperMusicPlayer.sln
├── SuperMusicPlayer/
│   ├── SuperMusicPlayer.csproj
│   ├── Program.cs                 # 程序入口
│   ├── MainForm.cs                # 主窗体 UI
│   ├── Track.cs                   # 歌曲数据模型
│   ├── MetadataReader.cs          # ID3 标签读取
│   ├── PlaylistManager.cs         # 播放列表管理
│   ├── AudioPlayer.cs             # NAudio 音频播放封装
│   └── RoundedButton.cs           # 自定义圆角按钮
├── Project_Proposal.md            # 本文档
└── Testing_Report.md              # 测试报告
```

---

## 7. 项目仓库

- **GitHub**: https://github.com/Curious717/super_musicpalyer
- **开发分支**: master
