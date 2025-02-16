using Content.Client._ViewportGui.ViewportUserInterface;

namespace Content.Client._ViewportGui.ViewportUserInterface.UI;

public interface IHUDControl
{
    Vector2i Position { get; set; }
    Vector2i Size { get; set; }

    void Draw(in ViewportUIDrawArgs args);
}

public class HUDControl : IHUDControl, IDisposable
{
    public Vector2i Position { get; set; }
    public Vector2i Size { get; set; }

    public virtual void Draw(in ViewportUIDrawArgs args)
    {
    }

    public virtual void Dispose()
    {
    }
}
