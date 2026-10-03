using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using AutoFateSync.Windows;
using System;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;

namespace AutoFateSync;

public sealed unsafe class Plugin : IDalamudPlugin
{
	[PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
	[PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
	[PluginService] internal static IPluginLog Log { get; private set; } = null!;
	[PluginService] internal static IFramework Framework { get; private set; } = null!;
	[PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
	[PluginService] internal static IChatGui Chat { get; private set; } = null!;

	private const string CommandName = "/afs";

	public Configuration Configuration { get; init; }

	public readonly WindowSystem WindowSystem = new("AutoFateSync");
	private MainWindow MainWindow { get; init; }

	private ushort trackedFateId;
	private DateTime enteredAt;
	private int attempts;

	public Plugin()
	{
		Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
		MainWindow = new MainWindow(this);
		WindowSystem.AddWindow(MainWindow);

		CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
		{
			HelpMessage = "設定画面を開く / 「/afs on」「/afs off」で有効・無効を切り替え"
		});

		PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
		PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

		Framework.Update += OnUpdate;

		Log.Information($"{PluginInterface.Manifest.Name} loaded.");
	}

	public void Dispose()
	{
		PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
		PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
		Framework.Update -= OnUpdate;

		WindowSystem.RemoveAllWindows();
		((IDisposable)MainWindow).Dispose();

		CommandManager.RemoveHandler(CommandName);
	}

	private void OnCommand(string command, string args)
	{
		switch (args.Trim().ToLowerInvariant())
		{
			case "on":
				Configuration.Enabled = true;
				Configuration.Save();
				Chat.Print("[AutoFateSync] 有効にしました。");
				break;
			case "off":
				Configuration.Enabled = false;
				Configuration.Save();
				Chat.Print("[AutoFateSync] 無効にしました。");
				break;
			default:
				MainWindow.Toggle();
				break;
		}
	}

	private void OnUpdate(IFramework framework)
	{
		if (!Configuration.Enabled) return;
		var player = ObjectTable.LocalPlayer;
		if (player == null) return;
		var manager = FateManager.Instance();
		if (manager == null) return;

		var fate = manager->CurrentFate;
		if (fate == null)
		{
			trackedFateId = 0;
			attempts = 0;
			return;
		}

		// Entered a new FATE
		if (fate->FateId != trackedFateId)
		{
			trackedFateId = fate->FateId;
			enteredAt = DateTime.UtcNow;
			attempts = 0;
		}

		// If the FATE is already synced, do nothing
		if (manager->SyncedFateId == fate->FateId) return;

		// If the FATE is not in a state that can be synced, do nothing
		if (fate->State is not (FateState.Preparing or FateState.Running)) return;

		// If the player is not in the level range for this FATE, do nothing
		if (player.Level <= fate->MaxLevel) return;

		if ((DateTime.UtcNow - enteredAt).TotalMilliseconds < Configuration.DelayMilliseconds) return;
		if (attempts >= Configuration.MaxAttempts) return;

		attempts++;
		enteredAt = DateTime.UtcNow;

		try
		{
			manager->LevelSync();
			Log.Information("LevelSync requested. FateId={0}, MaxLevel={1}, attempt={2}",
								fate->FateId, fate->MaxLevel, attempts);
			if (Configuration.ChatNotice)
			{
				Chat.Print($"[AutoFateSync] FATE「{fate->Name}」にレベルシンクしました。");
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex, "LevelSync failed.");
		}
	}

	public void ToggleMainUi() => MainWindow.Toggle();
}
