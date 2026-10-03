using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using System;
using System.Numerics;

namespace AutoFateSync.Windows;

public class MainWindow : Window, IDisposable
{
	private readonly Plugin plugin;

	public MainWindow(Plugin plugin) : base("AutoFateSync設定###AutoFateSync_MainWindow")
	{
		this.plugin = plugin;
		Size = new Vector2(320, 400);
		SizeCondition = ImGuiCond.FirstUseEver;
	}

	public void Dispose() { }

	public override void Draw()
	{
		using var child = ImRaii.Child("ChildWithAScrollbar", Vector2.Zero, true);

		if (child.Success)
		{
			var enabled = plugin.Configuration.Enabled;
			if (ImGui.Checkbox("自動レベルシンクを有効にする", ref enabled))
			{
				plugin.Configuration.Enabled = enabled;
				plugin.Configuration.Save();
			}

			var notice = plugin.Configuration.ChatNotice;
			if (ImGui.Checkbox("レベルシンク時にチャットへ通知する", ref notice))
			{
				plugin.Configuration.ChatNotice = notice;
				plugin.Configuration.Save();
			}

			var delay = plugin.Configuration.DelayMilliseconds;
			if (ImGui.SliderInt("自動レベルシンクの待ち時間(ms)", ref delay, 0, 3000))
			{
				plugin.Configuration.DelayMilliseconds = delay;
				plugin.Configuration.Save();
			}

			var maxAttempts = plugin.Configuration.MaxAttempts;
			if (ImGui.SliderInt("自動レベルシンクの最大試行回数", ref maxAttempts, 1, 10))
			{
				plugin.Configuration.MaxAttempts = maxAttempts;
				plugin.Configuration.Save();
			}
		}
	}
}
