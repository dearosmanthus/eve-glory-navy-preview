# Hotkey regression harness

Run on Windows with the .NET 8 SDK:

```powershell
dotnet run --project tests/HotkeyRegression/HotkeyRegression.csproj -c Release -p:UseSharedCompilation=false -- C:/path/to/isolated-scratch
```

The console runner returns exit code 0 only when every assertion passes. It uses
temporary profiles, a hidden settings form, and independent Windows threads that
briefly reserve unused Ctrl+Alt+Shift+F17..F24 combinations. It does not enumerate,
activate, minimize, or send keyboard input to any EVE/game window. Reservations
are released in `finally` blocks and thread disposal.

The repeat-prevention check verifies the `MOD_NOREPEAT` native registration flag;
it does not synthesize a physically held key. Actual multi-client foreground
switching remains a manual game test.

For a visible, isolated UI preview populated with fake client names, append
`--preview`. This mode does not construct the thumbnail manager or register any
system shortcuts and can safely run beside an existing EVE-O Preview instance.
Screenshots from regression mode are generated from real, off-screen form handles.

The settings lifecycle suite also runs the real `MainForm.Show` message loop and
real presenter/thumbnail manager in four watchdog-protected child processes. Its
process monitor returns an empty collection and never enumerates the computer's
game processes. It covers all settings tabs, tray restoration, Set/Cancel/record
flows, native modal ownership and Z-order, Apply persistence boundaries, and the
thumbnail DispatcherTimer while the recording dialog is open. The scale factors
100%, 125%, 150% and 200% use `Control.Scale`; they are layout simulations, not a
claim of testing those physical monitor DPI configurations.

To run only these scenarios, append `--settings-only`. A 45-second watchdog kills
only the specific child test process it created, records the last completed step
under `settings-scale-*/lifecycle-output.txt`, and reports a failure. Modal input
is delivered directly to owned test dialogs, never through global keyboard input.

The suite also covers dynamic groups beyond the legacy five: old/canonical JSON
precedence, offline membership, deep rollback, the seventh group's real shortcut
binding, UI add/rename/delete/reorder/drag, and presenter-driven automatic collection
and bidirectional preview visibility. Nested modal checks use native window enabled
state, ownership, topmost band and Z-order, not only WinForms' managed Enabled property.

Character-copy file tests use only scratch files, including backup/lock/reparse-point
failures and cancellation. HTTP is replaced by scripted responses. A separate
watchdog child exercises the real copy dialog with a fake service, explicit network
consent, default-No overwrite confirmation, directory-change invalidation, cancel,
and timeout. It does not scan the user's EVE directories or query real character
names. A second child isolates the presenter group message loop; closing a previous
Application.Run loop must not leak WM_QUIT into subsequent ShowDialog scenarios.
