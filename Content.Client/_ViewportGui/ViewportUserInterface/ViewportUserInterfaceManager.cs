using System.Numerics;
using Content.Client._ViewportGui.ViewportUserInterface.UI;
using Robust.Shared.Timing;

namespace Content.Client._ViewportGui.ViewportUserInterface;

public interface IViewportUserInterfaceManager
{
    /// <summary>
    /// Contains all HUD elements near viewport. And should do only that.
    /// </summary>
    HUDRoot? Root { get; }

    /// <summary>
    /// Can we interact with world objects on screen position.
    /// If VP-GUI element is focused - we should not do any interactions by another systems.
    /// </summary>
    bool CanMouseInteractInWorld { get; }

    void Initialize();
    void FrameUpdate(FrameEventArgs args);
    void Draw(ViewportUIDrawArgs args);
}

public sealed class ViewportUserInterfaceManager : IViewportUserInterfaceManager
{
    public HUDRoot? Root { get; private set; }

    public bool CanMouseInteractInWorld { get; private set; } = true;

    public void Initialize()
    {
        Root = new HUDRoot();
        CanMouseInteractInWorld = true;
    }

    public void FrameUpdate(FrameEventArgs args)
    {
    }

    public void Draw(ViewportUIDrawArgs args)
    {
        var handle = args.ScreenHandle;

        args.ScreenHandle.RenderInRenderTarget(args.RenderTexture, () =>
        {
            Root?.Draw(args);

            // Debug bounds drawing
            //handle.DrawRect(new UIBox2(new Vector2(0, 0), args.ContentSize), Color.Green.WithAlpha(0.5f), true);
        }, Color.Transparent);
    }
}
