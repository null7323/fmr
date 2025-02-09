# fmr: 可扩展 Midi 渲染器

目前已经可用的模块：QQS.Legacy，DominoRenderer、NoteCounter。

Textured与Scripted未完工。

## 依赖

- SDL 3（已包含在仓库Dependencies文件夹下）

- OpenTk（为GLRender提供支持）

Direct3D 和 Vulkan 渲染后端可能在更新计划中。

## 语言

C# 13 或更新的版本。需要 .NET 9 或更高版本。

## 构建

请注意检查生成后事件。生成后事件默认写入主程序x64 Release的特定文件夹。如果模块生成失败，请新建相关文件夹。渲染模块会被复制到 Modules 子文件夹，输出模块会被复制到 Export 子文件夹。

## 源代码引用

- 使用了来自 arduano 的 Zenith-MIDI 项目的 NumberSelect 控件。

- 使用了来自 flibitijibibo 的 SDL3-CS 项目的文件。

在此一并表示感谢。
