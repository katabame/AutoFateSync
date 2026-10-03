using Dalamud.Configuration;
using System;

namespace AutoFateSync;

[Serializable]
public class Configuration : IPluginConfiguration
{
	public int Version { get; set; } = 1;

	public bool Enabled { get; set; } = true;

	public int DelayMilliseconds { get; set; } = 500;

	public int MaxAttempts { get; set; } = 3;

	public bool ChatNotice { get; set; } = true;

	public void Save()
	{
		Plugin.PluginInterface.SavePluginConfig(this);
	}
}
