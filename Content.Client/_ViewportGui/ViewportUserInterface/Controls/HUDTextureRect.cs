using System.Numerics;
using Robust.Client.Graphics;

namespace Content.Client._ViewportGui.ViewportUserInterface.UI;

/// <summary>
/// Root control, what should contain all HUD element in <seealso cref="IViewportUserInterfaceManager"/>
/// </summary>
public class HUDTextureRect : HUDControl
{
    public Texture? Texture { get; set; }
    public override void Draw(in ViewportUIDrawArgs args)
    {
        var handle = args.ScreenHandle;
        var contentSize = args.ContentSize;

        if (Texture is null)
        {
            base.Draw(args);
            return;
        }

        handle.DrawTextureRect(Texture, new UIBox2(Position, Size));

        base.Draw(args);
    }
}
