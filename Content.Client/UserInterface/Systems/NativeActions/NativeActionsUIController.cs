using System.Numerics;
using Content.Client._ViewportGui.ViewportUserInterface;
using Content.Client._White.Intent;
using Content.Client.CombatMode;
using Content.Client.Gameplay;
using Content.Client.UserInterface.Screens;
using Content.Client.UserInterface.Systems.Alerts.Controls;
using Content.Client.UserInterface.Systems.Gameplay;
using Content.Shared.CombatMode;
using Content.Shared.Input;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;
using static Robust.Shared.Input.Binding.PointerInputCmdHandler;

namespace Content.Client.UserInterface.Systems;

public sealed class NativeActionsUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>, IOnSystemChanged<CombatModeSystem>
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IViewportUserInterfaceManager _vpUIManager = default!; // VPGui edit
    [UISystemDependency] private readonly CombatModeSystem _combatSystem = default!;
    [UISystemDependency] private readonly IntentSystem _intent = default!;

    public override void Initialize()
    {
        base.Initialize();

        var gameplayStateLoad = UIManager.GetUIController<GameplayStateLoadController>();
        gameplayStateLoad.OnScreenLoad += OnScreenLoad;
    }

    private void OnScreenLoad()
    {
    }

    public void OnStateEntered(GameplayState state)
    {
        //bind enable/disable combat mode key to ToggleCombatMenu;
        CommandBinds.Builder
           .Bind(ContentKeyFunctions.ToggleCombatMode, InputCmdHandler.FromDelegate(_ => ToggleCombatMode()))
           .Register<CombatModeSystem>();

        var builder = CommandBinds.Builder;

        // Binds for intents
        var hotbarKeys = ContentKeyFunctions.GetIntentsBoundKeys();
        for (var i = 0; i < hotbarKeys.Length; i++)
        {
            var boundId = i; // This is needed, because the lambda captures it.
            var boundKey = hotbarKeys[i];
            builder = builder.Bind(boundKey, new PointerInputCmdHandler((in PointerInputCmdArgs args) =>
            {
                if (args.State != BoundKeyState.Up)
                    return false;

                TriggerIntent(boundId);
                return true;
            }, false, true));
        }

        builder.Register<IntentSystem>();
    }

    public void OnStateExited(GameplayState state)
    {
        CommandBinds.Unregister<CombatModeSystem>();
        CommandBinds.Unregister<IntentSystem>();
    }

    public void OnPlayerAttached(EntityUid uid)
    {
    }

    public void OnPlayerDetached(EntityUid uid)
    {
    }

    public void TriggerIntent(int id)
    {
        var uid = _playerManager.LocalEntity;
        if (uid == null)
            return;

        if (!_intent.HasIntents((EntityUid) uid))
            return;

        _intent.LocalToggleIntent((Shared._White.Intent.Intent) id);
        _vpUIManager.PlayClickSound();
    }

    public void ToggleCombatMode()
    {
        _combatSystem.LocalToggleCombatMode();
    }

    public void OnSystemLoaded(CombatModeSystem system)
    {
        system.LocalPlayerAttached += OnPlayerAttached;
        system.LocalPlayerDetached += OnPlayerDetached;
    }

    public void OnSystemUnloaded(CombatModeSystem system)
    {
        system.LocalPlayerAttached -= OnPlayerAttached;
        system.LocalPlayerDetached -= OnPlayerDetached;
    }
}
