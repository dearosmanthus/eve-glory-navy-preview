# 实施与验收

- [x] T1 对照参考包类型、调用边和资源，核对新增功能及授权边界（R1–R8）。证据见 requirements.md；静态分析 PASS，参考程序运行 NOT RUN。
- [x] T2 实现动态组与旧配置迁移、失败回滚（R1、R4、R5）。旧五组导入、八组保存重载、第七组真实 Windows 注册、重复／占用回滚 PASS。
- [x] T3 实现动态分组界面和在线／隐藏状态（R2、R3、R4、R8）。增删改名、调序／跨组移动、离线保留、显示隐藏、录键／嵌套模态层级 PASS。
- [x] T4 实现角色配置同步（R6、R7）。隔离文件备份／替换、备份失败、文件锁、静态重解析点拒绝、取消、假 HTTP 解析、目录变更失效与显式默认 No 确认 PASS；真实游戏文件 NOT RUN。
- [x] T5 集成 Presenter、中文说明、原有全部回归与新增验收（R1–R8）。构建、消息循环、自动收纳／排除／双向隐藏状态、旧布局保留 PASS。真实 EVE 与物理混合 DPI NOT RUN。
- [x] T6 打包独立版本 8.1.0.0，发布目录仅含 EXE、语言包、MIT 许可和两份中文说明，无个人 JSON／日志／参考程序 DLL（R8）。源码采用 Git 跟踪文件归档，校验值单独生成。GitHub 发布沿用用户明确授权的仓库，不覆盖历史资产。

## 回归证据

2026-10-20（北京时间），工作区 `work/legato-feature-regression-fourth/test-output.txt` 及最终 `work/legato-feature-regression-release/test-output.txt` 两次完整执行：主套件均 **302 PASS / 0 FAIL**；独立子进程依次 **105、100、100、100、86、37 PASS，均 0 FAIL**。前四项为 100／125／150／200% 模拟布局，后两项为角色复制界面及真实分组 Presenter 集成。中英及最小窗口截图已人工查看，内容可见、底部操作可达。自包含 Windows x64 发布构建成功，仅保留 WindowManager 的三个既有未用字段警告。

前轮问题已定位：嵌套模态不能用托管 `Enabled` 代替原生 `IsWindowEnabled`；不同 Application.Run 生命周期必须分进程隔离 WM_QUIT。修正测试后仍严格验证原生禁用、Owner、置顶和 Z 序，没有放宽行为要求。

边界：未运行参考程序，未复制其实现；未读写真实玩家配置，未用真实名称访问 ESI；未覆盖恶意并发替换路径的 TOCTOU。原 `ConfigurationStorage.Save()` 对 IOException 的吞错行为属于既有风险，本轮未重构，应使用可写目录并保留升级前备份。
