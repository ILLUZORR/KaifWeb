using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Shared.Input;

namespace Content.Client._ViewportGui.ViewportUserInterface.UI;

/// <summary>
/// Buttons. Like <seealso cref="HUDControl"/>, but it has OnPressed what can be emited by UIClick or UIRightClick.
/// </summary>
public class HUDButton : HUDControl
{
    public HUDButtonClickType ButtonClickType { get; set; } = HUDButtonClickType.OnUp;

    public event Action<GUIBoundKeyEventArgs>? OnPressed;

    public HUDButton()
    {
        // Idk how to set it by default for buttons
        MouseFilter = HUDMouseFilterMode.Stop;
    }

    public override void KeyBindDown(GUIBoundKeyEventArgs args)
    {
        base.KeyBindDown(args);

        if (ButtonClickType == HUDButtonClickType.OnDown &&
            (args.Function == EngineKeyFunctions.UIClick ||
            args.Function == EngineKeyFunctions.UIRightClick))
            OnPressed?.Invoke(args);
    }

    public override void KeyBindUp(GUIBoundKeyEventArgs args)
    {
        base.KeyBindUp(args);

        if (ButtonClickType == HUDButtonClickType.OnUp &&
            (args.Function == EngineKeyFunctions.UIClick ||
            args.Function == EngineKeyFunctions.UIRightClick))
            OnPressed?.Invoke(args);
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
