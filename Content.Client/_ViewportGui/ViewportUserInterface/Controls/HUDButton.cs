using System.Numerics;
using Content.Shared.Input;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Shared.Input;

namespace Content.Client._ViewportGui.ViewportUserInterface.UI;

/// <summary>
/// Buttons. Like <seealso cref="HUDControl"/>, but it has OnPressed what can be emited by UIClick or UIRightClick.
/// </summary>
public class HUDButton : HUDControl
{
    [Dependency] private readonly IViewportUserInterfaceManager _vpUIManager = default!;

    public HUDButtonClickType ButtonClickType { get; set; } = HUDButtonClickType.OnUp;

    public event Action<GUIBoundKeyEventArgs>? OnPressed;

    public HUDButton()
    {
        IoCManager.InjectDependencies(this);

        // Idk how to set it by default for buttons
        MouseFilter = HUDMouseFilterMode.Stop;
    }

    public override void KeyBindDown(GUIBoundKeyEventArgs args)
    {
        base.KeyBindDown(args);

        if (!VisibleInTree)
            return;

        TryPressButton(args, HUDButtonClickType.OnDown);
    }

    public override void KeyBindUp(GUIBoundKeyEventArgs args)
    {
        base.KeyBindUp(args);

        if (!VisibleInTree)
            return;

        TryPressButton(args, HUDButtonClickType.OnUp);
    }

    private void TryPressButton(GUIBoundKeyEventArgs args, HUDButtonClickType clickType)
    {
        if (ButtonClickType == clickType &&
            (args.Function == EngineKeyFunctions.UIClick ||
            args.Function == ContentKeyFunctions.MoveStoredItem || // TODO: By some reasons UIClick doesn't work with viewport
            args.Function == EngineKeyFunctions.UIRightClick))
        {
            _vpUIManager.PlayClickSound();
            OnPressed?.Invoke(args);
        }
    }
}

public enum HUDButtonClickType
{
    /// <summary>
    /// Should button clicked on KeyBindDown.
    /// </summary>
    OnDown = 0,

    /// <summary>
    /// Should button clicked on KeyBindUp.
    /// </summary>
    OnUp = 1
}
