
using Content.Client._ViewportGui.ViewportUserInterface.Overlays;
using Robust.Client.Graphics;

namespace Content.Client._ViewportGui.ViewportUserInterface.Systems;
public sealed class ViewportUserInterfaceSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;

    public override void Initialize()
    {
        _overlay.AddOverlay(new ViewportUserInterfaceOverlay());
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlay.RemoveOverlay<ViewportUserInterfaceOverlay>();
    }
}
