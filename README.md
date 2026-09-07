# EVE-O Preview · 荣耀海军定制版

<p align="center">
  <img src="src/Eve-O-Preview/glory-navy-icon.png" width="160" alt="荣耀海军徽记">
</p>

面向荣耀海军成员定制的 EVE-O Preview Windows 多开窗口预览与快捷切换工具。

当前版本：`8.0.2.28-glory-navy-final`

## 下载与使用

请从仓库右侧的 **Releases** 下载最新版 Windows 压缩包，完整解压后运行：

`EVE-O-Preview-Glory-Navy.exe`

不要直接在压缩包内运行，也不建议解压到 `Program Files`。首次启动若遇到 Windows SmartScreen，请先核对发布页提供的 SHA256 校验值。

## 定制功能

- 缩略图最低可调至 `64 × 36`，宽高支持 1 像素步进和尺寸预设。
- `Ctrl + Alt + ←/→` 快速切换 EVE 客户端。
- `Ctrl + Alt + H` 快速隐藏或显示全部预览。
- 支持为单个客户端设置独立缩略图尺寸。
- 支持命名保存、加载和覆盖多套布局。
- 窗口吸附与更灵活的预览位置调整。
- 实验性后台恢复机制：尽量维持已最小化客户端的 DWM 预览刷新，但效果仍取决于 Windows、显卡驱动和游戏渲染状态。
- 重新分类的中文设置界面与荣耀海军品牌图标。

完整的中文使用说明见 [README-cn.md](README-cn.md)，本版变更摘要见 [GLORY-NAVY-FINAL-CHANGES-zh-CN.txt](GLORY-NAVY-FINAL-CHANGES-zh-CN.txt)。

## 安全与合规说明

本程序用于显示各 EVE 客户端的实时缩略图，并将选中的客户端切换到前台。它不广播键盘或鼠标输入，也不修改 EVE Online 客户端或游戏界面。

使用者仍需自行遵守 EVE Online 当前的 EULA、服务条款和当地法律。本项目与 CCP Games 无隶属或官方认可关系；EVE Online 及相关标识归其权利人所有。

## 从源码构建

要求 Windows 和 .NET 8 SDK：

```powershell
dotnet publish src\Eve-O-Preview\Eve-O-Preview.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true
```

## 上游与许可证

本定制版基于 EVE-O Preview，上游基线提交：

`487b4b2c49afb021f40647eb551a3d039d21451f`

原项目资料和论坛链接保留在中文使用说明中。源码遵循仓库内的 [MIT License](LICENSE)，原作者版权声明完整保留。

## 校验文件

正式发布包会同时提供 `.sha256.txt` 文件。可在 PowerShell 中校验：

```powershell
Get-FileHash .\EVE-O-Preview-Glory-Navy-Final-win-x64.zip -Algorithm SHA256
```
