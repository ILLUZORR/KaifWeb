using Content.Client._ViewportGui.ViewportUserInterface;
using Content.Client._ViewportGui.ViewportUserInterface.UI;
using Content.Shared._Shitmed.Targeting;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;

namespace Content.Client._Shitmed.UserInterface.Systems.Targeting.Controls;

public class HUDTargetDoll : HUDTextureRect
{
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

    private readonly TargetingUIController _controller;

    public Dictionary<TargetBodyPart, Texture?>? BodyPartTexturesHovered;
    public Texture? Background;
    public Texture? TextureHovered;
    public Texture? TextureFocused;

    private readonly Dictionary<TargetBodyPart, HUDButton> _bodyPartControls;

    public HUDTargetDoll(TargetingUIController controller)
    {
        IoCManager.InjectDependencies(this);

        Name = "TargetDoll";
        Size = (32, 64);
        Position = (0, 32);
        _controller = controller;

        # region Body

        var headButton = new HUDButton()
        {
            Size = (6, 11),
            Position = (13, 0)
        };
        AddChild(headButton);

        var chestButton = new HUDButton()
        {
            Size = (8, 13),
            Position = (12, 10)
        };
        AddChild(chestButton);

        var groinButton = new HUDButton()
        {
            Size = (12, 12),
            Position = (10, 23)
        };
        AddChild(groinButton);

        # endregion

        # region Hands/Arms

        var rightArmButton = new HUDButton()
        {
            Size = (8, 23),
            Position = (4, 10)
        };
        AddChild(rightArmButton);

        var rightHandButton = new HUDButton()
        {
            Size = (6, 10),
            Position = (2, 34)
        };
        AddChild(rightHandButton);

        var leftArmButton = new HUDButton()
        {
            Size = (8, 23),
            Position = (20, 10),
        };
        AddChild(leftArmButton);

        var leftHandButton = new HUDButton()
        {
            Size = (6, 10),
            Position = (24, 34)
        };
        AddChild(leftHandButton);

        # endregion

        # region Legs/Foots

        var leftLegButton = new HUDButton()
        {
            Size = (5, 28),
            Position = (17, 31)
        };
        AddChild(leftLegButton);

        var leftFootButton = new HUDButton()
        {
            Size = (8, 5),
            Position = (17, 58)
        };
        AddChild(leftFootButton);

        var rightLegButton = new HUDButton()
        {
            Size = (5, 28),
            Position = (10, 31)
        };
        AddChild(rightLegButton);

        var rightFootButton = new HUDButton()
        {
            Size = (8, 5),
            Position = (8, 58)
        };
        AddChild(rightFootButton);

        # endregion

        _bodyPartControls = new Dictionary<TargetBodyPart, HUDButton>
        {
            // TODO: ADD EYE AND MOUTH TARGETING
            { TargetBodyPart.Head, headButton },
            { TargetBodyPart.Torso, chestButton },
            { TargetBodyPart.Groin, groinButton },
            { TargetBodyPart.LeftArm, leftArmButton },
            { TargetBodyPart.LeftHand, leftHandButton },
            { TargetBodyPart.RightArm, rightArmButton },
            { TargetBodyPart.RightHand, rightHandButton },
            { TargetBodyPart.LeftLeg, leftLegButton },
            { TargetBodyPart.LeftFoot, leftFootButton },
            { TargetBodyPart.RightLeg, rightLegButton },
            { TargetBodyPart.RightFoot, rightFootButton },
        };

        // Load hover textures
        LoadHoverTextures();

        // And set up some events on buttons
        foreach (var bodyPartButton in _bodyPartControls)
        {
            bodyPartButton.Value.MouseFilter = HUDMouseFilterMode.Stop;
            bodyPartButton.Value.OnPressed += _ => SetActiveBodyPart(bodyPartButton.Key);
            /*bodyPartButton.Value.OnMouseEntered += _ =>
            {
                if (_control.BodyPartTexturesHovered != null)
                    _control.TextureFocused = _control.BodyPartTexturesHovered[bodyPartButton.Key];
            };
            bodyPartButton.Value.OnMouseExited += _ =>
            {
                if (_control.BodyPartTexturesHovered != null)
                    _control.TextureFocused = null;
            };
            */
        }

        Background = _uiManager.CurrentTheme.ResolveTexture("target_doll");
    }

    public override void Draw(in ViewportUIDrawArgs args)
    {
        var handle = args.ScreenHandle;

        if (!VisibleInTree)
            return;

        // Background
        if (Background != null)
            DrawTexture(handle, Background);

        // Draw hovered
        if (TextureHovered != null)
            DrawTexture(handle, TextureHovered);

        // Draw hovered
        if (TextureFocused != null)
            DrawTexture(handle, TextureFocused);

        base.Draw(args);
    }

    private void LoadHoverTextures()
    {
        BodyPartTexturesHovered = new Dictionary<TargetBodyPart, Texture?>();
        foreach (var item in _bodyPartControls)
        {
            string enumName = Enum.GetName(typeof(TargetBodyPart), item.Key) ?? "Unknown";
            var texture = _uiManager.CurrentTheme.ResolveTexture($"target_{enumName.ToLowerInvariant()}_hover.png");
            BodyPartTexturesHovered[item.Key] = texture;
        }
    }

    public void SetBodyPartsVisible(TargetBodyPart bodyPart)
    {
        if (BodyPartTexturesHovered == null)
            return;

        foreach (var item in BodyPartTexturesHovered)
        {
            if (item.Key == bodyPart)
                TextureHovered = item.Value;
        }
    }

    private void SetActiveBodyPart(TargetBodyPart bodyPart) => _controller.CycleTarget(bodyPart);

    public void SetTargetDollVisible(bool visible) => Visible = visible;

    public void DrawTexture(DrawingHandleScreen handle, Texture tex, Color? color = null)
    {
        handle.DrawTextureRect(tex, new UIBox2(GlobalPosition, GlobalPosition + Size), color);
    }
}
