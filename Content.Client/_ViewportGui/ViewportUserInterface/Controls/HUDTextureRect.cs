using System.Numerics;
using Robust.Client.Graphics;

namespace Content.Client._ViewportGui.ViewportUserInterface.UI;

/// <summary>
/// Root control, what should contain all HUD element in <seealso cref="IViewportUserInterfaceManager"/>
/// </summary>
public class HUDTextureRect : HUDControl
{
    /// <summary>
    /// Drawing texture. For setting size or position use <seealso cref="HUDTextureRect"/>'s Size and Position variables.
    /// </summary>
    public Texture? Texture { get; set; }

    public override void Draw(in ViewportUIDrawArgs args)
    {
        var handle = args.ScreenHandle;

        if (Texture is null || !VisibleInTree)
        {
            base.Draw(args);
            return;
        }

        handle.DrawTextureRect(Texture, new UIBox2(GlobalPosition, GlobalPosition + Size));
        base.Draw(args);
    }
}
