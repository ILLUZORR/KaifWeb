
using Robust.Client.Graphics;

namespace Content.Client._ViewportGui.ViewportUserInterface;

public readonly ref struct ViewportUIDrawArgs
{
    public readonly OverlayDrawArgs OverlayDrawArgs;

    public readonly Vector2i ContentSize;

    public readonly UIBox2 DrawBounds;

    public ViewportUIDrawArgs(
        Vector2i contentSize,
        UIBox2 drawBounds,
        in OverlayDrawArgs overlayDrawArgs)
    {
        ContentSize = contentSize;
        DrawBounds = drawBounds;
        OverlayDrawArgs = overlayDrawArgs;
    }
}
