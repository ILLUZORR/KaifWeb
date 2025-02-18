
using Robust.Client.Graphics;
using Robust.Client.UserInterface.CustomControls;

namespace Content.Client._ViewportGui.ViewportUserInterface;

public readonly struct ViewportUIDrawArgs
{
    /// <summary>
    /// Where UI should be drawen.
    /// </summary>
    public readonly IRenderTexture RenderTexture;

    /// <summary>
    /// With that shit we can draw any content on screen.
    /// </summary>
    public readonly DrawingHandleScreen ScreenHandle;

    /// <summary>
    /// UI size. Should be used by <seealso cref="HUDControl"/> and his childs.
    /// </summary>
    public readonly Vector2i ContentSize;

    /// <summary>
    /// Global pixel bounds. Use DrawBounds.Left to take local Content.Size.X = 0.
    /// </summary>
    public readonly UIBox2 DrawBounds;

    /// <summary>
    /// Scale UI. Because we can scale world viewbox in viewport by window resizing.
    /// </summary>
    public readonly float DrawScale;

    /// <summary>
    ///     The viewport control that is rendering this viewport.
    ///     Not always available.
    /// </summary>
    public readonly IViewportControl ViewportControl;

    public ViewportUIDrawArgs(
        IRenderTexture renderTexture,
        Vector2i contentSize,
        UIBox2 drawBounds,
        float drawScale,
        IViewportControl viewportControl,
        in DrawingHandleScreen handle)
    {
        RenderTexture = renderTexture;
        ContentSize = contentSize;
        DrawBounds = drawBounds;
        DrawScale = drawScale;
        ViewportControl = viewportControl;
        ScreenHandle = handle;
    }
}
