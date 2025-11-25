using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.NativeWrapper;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

namespace CongaHelper.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;
    private readonly Conga conga;
    private readonly IGameGui gameGui;

    public MainWindow(Plugin plugin, Conga conga, IGameGui gameGui)
        : base("Conga Helper", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.AlwaysAutoResize)
    {
        this.plugin = plugin;
        this.conga = conga;
        this.gameGui = gameGui;
    }

    public override void PreOpenCheck()
    {
        IsOpen = IsPartyMemberListOpen();
        if (IsOpen)
        {
            Position = GetButtonPosition();
        }
    }

    private bool IsPartyMemberListOpen()
    {
        AtkUnitBasePtr basePtr = gameGui.GetAddonByName("PartyMemberList");
        return basePtr.IsVisible;
    }

    private Vector2 GetButtonPosition()
    {
        AtkUnitBasePtr basePtr = gameGui.GetAddonByName("PartyMemberList");
        return basePtr.Position + new Vector2(0, basePtr.ScaledHeight);
    }

    public void Dispose() { }

    public override void Draw()
    {
        if (ImGui.Button("Conga"))
        {
            conga.DoConga();
        }
    }
}
