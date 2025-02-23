using Content.Client._ViewportGui.ViewportUserInterface.UI;
using Content.Shared.Alert;
using Robust.Shared.Timing;

namespace Content.Client.UserInterface.Systems.Alerts.Controls;

public class HUDAlertControl : HUDButton
{
    public AlertPrototype Alert { get; }

    private (TimeSpan Start, TimeSpan End)? _cooldown;

    private HUDAnimatedTextureRect _textureRect;

    private short? _severity;

    public HUDAlertControl(AlertPrototype alert, short? severity)
    {
        Alert = alert;
        _severity = severity;
        _textureRect = new HUDAnimatedTextureRect();
        AddChild(_textureRect);

        Name = alert.Name;

        var icon = Alert.GetIcon(_severity);
        _textureRect.SetFromSpriteSpecifier(icon);

        Size = _textureRect.Size;
        if (!alert.IsGeneric)
            Position = (alert.HudPositionX, alert.HudPositionY);
    }

    /// <summary>
    /// Change the alert severity, changing the displayed icon
    /// </summary>
    public void SetSeverity(short? severity)
    {
        if (_severity == severity)
            return;
        _severity = severity;

        var icon = Alert.GetIcon(_severity);
        _textureRect.SetFromSpriteSpecifier(icon);
    }
}

public enum AlertVisualLayers : byte
{
    Base
}
