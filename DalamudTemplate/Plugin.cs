using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using DalamudTemplate.Windows;
using System;

namespace DalamudTemplate;

public sealed class Plugin : IDalamudPlugin
{
	[PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
	[PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
	[PluginService] internal static IPluginLog Log { get; private set; } = null!;

	private const string CommandName = "/dalamudtemplate";

	public Configuration Configuration { get; init; }

	public readonly WindowSystem WindowSystem = new("DalamudTemplate");
	private MainWindow MainWindow { get; init; }

	public Plugin()
	{
		Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
		MainWindow = new MainWindow(this);
		WindowSystem.AddWindow(MainWindow);

		CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
		{
			HelpMessage = "DalamudTemplateのメインウィンドウを表示/非表示します。"
		});

		PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
		PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

		Log.Information($"{PluginInterface.Manifest.Name} loaded.");
	}

	public void Dispose()
	{
		PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
		PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;

		WindowSystem.RemoveAllWindows();
		((IDisposable)MainWindow).Dispose();

		CommandManager.RemoveHandler(CommandName);
	}

	private void OnCommand(string command, string args)
	{
		MainWindow.Toggle();
	}

	public void ToggleMainUi() => MainWindow.Toggle();
}
