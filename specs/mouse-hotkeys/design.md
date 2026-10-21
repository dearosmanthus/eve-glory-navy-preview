# 鼠标快捷键：增量设计

沿用字符串配置与 WinForms.Keys，不迁移现有 JSON。MButton、XButton1、XButton2 使用 Windows 标准键值；F13/F14 仍走键盘注册，只是明确提供给额外物理侧键的驱动映射。键盘依旧 RegisterHotKey，鼠标单独使用系统 WH_MOUSE_LL，按类型分流而非向键盘 API 传入鼠标值。

`HotkeyGesture.IsMouseGesture(Keys)` 统一识别三种原生鼠标键。`MouseInputListener(Func<Keys,bool> accepts, Action<Keys> pressed)` 提供 Start(out int error) 与幂等 Dispose。低级回调在专用消息线程快速解码/匹配，实际操作异步投递到创建者 UI 线程，代次/存活检查作废旧队列。正常注册共享单个监听，只在有鼠标绑定或正在录制时安装。

按键状态记住按下时的匹配结果，配对处理抬起与修饰键变化，避免长按重复和只吞半个点击。未知按钮、左右键、滚动及鼠标移动不执行任何动作。不读取账号、不记录轨迹、不发送键鼠、不注入其他进程。

独立复核确认的边界：最后解绑/停止会立即移除监听，若此时物理键仍按住，后续孤立抬起会透传；新监听不会继承旧按下状态或派发旧动作。配对保证限定于同一监听有效期间。应用设置/停止前应松开按键，不通过延迟移除全局钩子掩盖此边界。

录制窗口保留键盘直接录入；鼠标输入与五个预设先形成候选，再显式确认。预设明确区分原生侧键/中键与 F13/F14 驱动映射。菜单、全局、分组和角色入口共用同一录制窗口，沿用嵌套设置窗口暂停/恢复机制。

资源生命周期检查包括完整停止后释放键盘注册/鼠标监听、再次启动恢复、停止期间不意外重注册，及录制中断后旧动作失效。独立子进程运行 UI 验收，防止不同 Application.Run 的 WM_QUIT 相互污染。测试只使用自有窗口/假游戏边界，不向玩家游戏发出输入。

所有权：后端代理仅 Hotkeys/ 与 MouseBackendRegression.cs；界面代理仅 HotkeyCaptureDialog.cs 与 MouseCaptureUiRegression.cs；主代理负责 Manager、共享集成、配置调用点、完整测试、版本、文档和发布。构建与发布串行。
