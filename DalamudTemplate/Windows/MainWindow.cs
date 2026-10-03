using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using System;
using System.Numerics;

namespace DalamudTemplate.Windows;

public class MainWindow : Window, IDisposable
{
	private readonly Plugin plugin;

	public MainWindow(Plugin plugin) : base("DalamudTemplate###DalamudTemplate_MainWindow")
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
		}
	}
}
