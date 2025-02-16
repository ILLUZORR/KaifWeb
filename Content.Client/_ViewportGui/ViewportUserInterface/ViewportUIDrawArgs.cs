
using Robust.Client.Graphics;

namespace Content.Client._ViewportGui.ViewportUserInterface;

public readonly struct ViewportUIDrawArgs
{
    public readonly IRenderTexture RenderTexture;
    public readonly DrawingHandleScreen ScreenHandle;

    public readonly Vector2i ContentSize;

    public readonly UIBox2 DrawBounds;

    public readonly float DrawScale;

    public ViewportUIDrawArgs(
        IRenderTexture renderTexture,
        Vector2i contentSize,
        UIBox2 drawBounds,
        float drawScale,
        in DrawingHandleScreen handle)
    {
        RenderTexture = renderTexture;
        ContentSize = contentSize;
        DrawBounds = drawBounds;
        DrawScale = drawScale;
        ScreenHandle = handle;
    }
}
