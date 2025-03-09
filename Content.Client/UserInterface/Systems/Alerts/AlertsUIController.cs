using Content.Client._ViewportGui.ViewportUserInterface;
using Content.Client._ViewportGui.ViewportUserInterface.UI;
using Content.Client.Alerts;
using Content.Client.Gameplay;
using Content.Client.UserInterface.Systems.Alerts.Controls;
using Content.Client.UserInterface.Systems.Gameplay;
using Content.Client.UserInterface.Systems.Viewport;
using Content.Shared.Alert;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Prototypes;

namespace Content.Client.UserInterface.Systems.Alerts;

public sealed class AlertsUIController : UIController, IOnStateEntered<GameplayState>, IOnSystemChanged<ClientAlertsSystem>
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IViewportUserInterfaceManager _vpUIManager = default!; // VPGui edit

    [UISystemDependency] private readonly ClientAlertsSystem? _alertsSystem = default;

    // VPGui edit
    /// <summary>
    /// Should be used to attach all right content to the... Right.
    /// Like alerts, buttons and another content
    /// </summary>
    public HUDAlertsPanel? AlertsPanel;
    // VPGui edit end

    public override void Initialize()
    {
        base.Initialize();

        // VPGui edit
        AlertsPanel = new HUDAlertsPanel();
        AlertsPanel.Name = "AlertsPanel";
        AlertsPanel.Texture = _vpUIManager.GetTexturePath("/Textures/Interface/Default/right_panel_background_full.png");
        if (AlertsPanel.Texture is not null)
        {
            AlertsPanel.Size = (AlertsPanel.Texture.Size.X, AlertsPanel.Texture.Size.Y);
            AlertsPanel.Position = (
                EyeManager.PixelsPerMeter * (ViewportUIController.ViewportHeight + 1) -
                AlertsPanel.Texture.Size.X, 0); // fucking calculus
        }

        _vpUIManager.Root.AddChild(AlertsPanel);
        // VPGui edit end

        var gameplayStateLoad = UIManager.GetUIController<GameplayStateLoadController>();
        gameplayStateLoad.OnScreenLoad += OnScreenLoad;
        gameplayStateLoad.OnScreenUnload += OnScreenUnload;
    }

    private void OnScreenUnload()
    {
        var widget = AlertsPanel;
        if (widget != null)
            widget.AlertPressed -= OnAlertPressed;
    }

    private void OnScreenLoad()
    {
        var widget = AlertsPanel;
        if (widget != null)
            widget.AlertPressed += OnAlertPressed;

        SyncAlerts();
    }

    private void OnAlertPressed(object? sender, ProtoId<AlertPrototype> e)
    {
        _alertsSystem?.AlertClicked(e);
    }

    private void SystemOnClearAlerts(object? sender, EventArgs e)
    {
        AlertsPanel?.ClearAllControls();
    }

    private void SystemOnSyncAlerts(object? sender, IReadOnlyDictionary<AlertKey, AlertState> e)
    {
        if (sender is ClientAlertsSystem system)
        {
            AlertsPanel?.SyncControls(system, system.AlertOrder, e);
        }
    }

    public void OnSystemLoaded(ClientAlertsSystem system)
    {
        system.SyncAlerts += SystemOnSyncAlerts;
        system.ClearAlerts += SystemOnClearAlerts;
    }

    public void OnSystemUnloaded(ClientAlertsSystem system)
    {
        system.SyncAlerts -= SystemOnSyncAlerts;
        system.ClearAlerts -= SystemOnClearAlerts;
    }


    public void OnStateEntered(GameplayState state)
    {
        // initially populate the frame if system is available
        SyncAlerts();
    }

    public void SyncAlerts()
    {
        var alerts = _alertsSystem?.ActiveAlerts;
        if (alerts != null)
        {
            SystemOnSyncAlerts(_alertsSystem, alerts);
        }
    }
}
