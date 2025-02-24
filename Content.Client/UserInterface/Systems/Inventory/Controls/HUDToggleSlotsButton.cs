
using Content.Client._ViewportGui.ViewportUserInterface;
using Content.Client._ViewportGui.ViewportUserInterface.UI;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;

namespace Content.Client.UserInterface.Systems.Inventory.Controls;

public class HUDToggleSlotsButton : HUDButton
{
    [Dependency] private readonly IUserInterfaceManager _UIManager = default!;

    public Texture? ButtonTexture { get; set; }

    public HUDToggleSlotsButton()
    {
        IoCManager.InjectDependencies(this);

        Size = (8, 32); // TODO: Should it use texture's size?
        ButtonTexture = _UIManager.CurrentTheme.ResolveTexture("slots_toggle"); // TODO: Use VPGui theme manager
    }

    public override void Draw(in ViewportUIDrawArgs args)
    {
        var handle = args.ScreenHandle;

        if (ButtonTexture is null || !VisibleInTree)
        {
            base.Draw(args);
            return;
        }

        handle.DrawTextureRect(ButtonTexture, new UIBox2(GlobalPosition, GlobalPosition + Size));
        base.Draw(args);
    }
}
