using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CongaHelper.Windows;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;

namespace CongaHelper;

public sealed class Plugin : IAsyncDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IGameGui GameGui { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static IPartyList PartyList { get; private set; } = null!;
    [PluginService] internal static IPlayerState PlayerState { get; private set; } = null!;

    private const string CommandName = "/conga";

    public readonly WindowSystem WindowSystem = new("CongaHelper");
    private MainWindow MainWindow { get; set; }
    private Conga Conga { get; set; }

    public Task LoadAsync(CancellationToken cancellationToken)
    {
        Conga = new Conga(ClientState, GameGui, ObjectTable, PartyList, PlayerState);
        MainWindow = new MainWindow(this, Conga, GameGui);

        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Arrange party list in left-to-right screen order"
        });

        // Tell the UI system that we want our windows to be drawn through the window system
        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        
        return Task.CompletedTask;
    }
    
    public ValueTask DisposeAsync()
    {
        // Unregister all actions to not leak anythign during disposal of plugin
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        
        WindowSystem.RemoveAllWindows();

        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);

        return ValueTask.CompletedTask;
    }

    private void OnCommand(string command, string args)
    {
        Conga.DoConga();
    }
    
}
