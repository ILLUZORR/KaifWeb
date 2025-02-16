namespace Content.Client._ViewportGui.ViewportUserInterface;

public interface IViewportUserInterfaceManager
{
    void Initialize();
    void Draw(in ViewportUIDrawArgs args);
}

public sealed class ViewportUserInterfaceManager : IViewportUserInterfaceManager
{
    public void Initialize()
    {
    }

    public void Draw(in ViewportUIDrawArgs args)
    {
        args.OverlayDrawArgs.ScreenHandle.DrawRect(args.DrawBounds, Color.Green, false);
    }
}
