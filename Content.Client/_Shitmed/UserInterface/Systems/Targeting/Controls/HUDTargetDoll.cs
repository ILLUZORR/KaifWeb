using Content.Client._ViewportGui.ViewportUserInterface;
using Content.Client._ViewportGui.ViewportUserInterface.UI;
using Content.Shared._Shitmed.Targeting;
using Robust.Client.Graphics;

namespace Content.Client._Shitmed.UserInterface.Systems.Targeting.Controls;

public class HUDTargetDoll : HUDTextureRect
{
    public Dictionary<TargetBodyPart, Texture?>? BodyPartTexturesHovered;
    public Texture? TextureHovered;
    public Texture? TextureFocused;

    public HUDTargetDoll()
    {
        Name = "TargetDoll";
    }

    public override void Draw(in ViewportUIDrawArgs args)
    {
        var handle = args.ScreenHandle;

        // Draw hovered
        if (TextureHovered != null)
            DrawTexture(handle, TextureHovered);

        // Draw hovered
        if (TextureFocused != null)
            DrawTexture(handle, TextureFocused);

        base.Draw(args);
    }
/*
    private void CreateButtons()
    {
        # region Hands/Arms

        var RightArmButton;
        var RightHandButton;

        var LeftArmButton;
        var LeftHandButton;

        # endregion

        # region Body

        var HeadButton;
        var ChestButton;
        var GroinButton;

        # endregion

        # region Legs/Foots



        # endregion

        var LeftLegButton;
        var LeftFootButton;
        var RightLegButton;
        var RightFootButton;
    }
*/
    public void DrawTexture(DrawingHandleScreen handle, Texture tex, Color? color = null)
    {
        handle.DrawTextureRect(tex, new UIBox2(GlobalPosition, GlobalPosition + Size), color);
    }
}
