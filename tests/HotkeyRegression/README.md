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
