using System.Numerics;

namespace Content.Client._ViewportGui.ViewportUserInterface;

public interface IViewportUserInterfaceManager
{
    void Initialize();
    void Draw(ViewportUIDrawArgs args);
}

public sealed class ViewportUserInterfaceManager : IViewportUserInterfaceManager
{
    int y = 0;

    public void Initialize()
    {
    }

    public void Draw(ViewportUIDrawArgs args)
    {
        var handle = args.ScreenHandle;

        args.ScreenHandle.RenderInRenderTarget(args.RenderTexture, () =>
        {
            handle.DrawRect(new UIBox2(new Vector2(0, y), new Vector2(32, y + 32)), Color.Green.WithAlpha(0.5f), true);

            y++;
            if (y >= 240)
                y = 0;

            // Debug bounds drawing
            //handle.DrawRect(new UIBox2(new Vector2(0, 0), args.ContentSize), Color.Green.WithAlpha(0.5f), true);
        }, Color.Transparent);
    }
}
