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
            Size = (6, 8),
            Position = (13, 4)
        };
        AddChild(headButton);

        var neckButton = new HUDButton()
        {
            Size = (6, 4),
            Position = (13, 12)
        };
        AddChild(neckButton);

        var chestButton = new HUDButton()
        {
            Size = (10, 16),
            Position = (11, 14)
        };
        AddChild(chestButton);

        var groinButton = new HUDButton()
        {
            Size = (10, 8),
            Position = (11, 29)
        };
        AddChild(groinButton);

        # endregion

        # region Hands/Arms

        var rightArmButton = new HUDButton()
        {
            Size = (6, 20),
            Position = (6, 14)
        };
        AddChild(rightArmButton);

        var rightHandButton = new HUDButton()
        {
            Size = (6, 6),
            Position = (3, 31)
        };
        AddChild(rightHandButton);

        var leftArmButton = new HUDButton()
        {
            Size = (6, 20),
            Position = (20, 14),
        };
        AddChild(leftArmButton);

        var leftHandButton = new HUDButton()
        {
            Size = (6, 6),
            Position = (23, 31)
        };
        AddChild(leftHandButton);

        # endregion

        # region Legs/Foots

        var leftLegButton = new HUDButton()
        {
            Size = (7, 24),
            Position = (16, 34)
        };
        AddChild(leftLegButton);

        var leftFootButton = new HUDButton()
        {
            Size = (7, 3),
            Position = (16, 57)
        };
        AddChild(leftFootButton);

        var rightLegButton = new HUDButton()
        {
            Size = (7, 24),
            Position = (9, 34)
        };
        AddChild(rightLegButton);

        var rightFootButton = new HUDButton()
        {
            Size = (7, 3),
            Position = (9, 57)
        };
        AddChild(rightFootButton);

        # endregion

        _bodyPartControls = new Dictionary<TargetBodyPart, HUDButton>
        {
            // TODO: ADD EYE AND MOUTH TARGETING
            { TargetBodyPart.Head, headButton },
            { TargetBodyPart.Neck, neckButton },
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

        Background = _uiManager.CurrentTheme.ResolveTexture("TargetDoll/target_doll");
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
            var texture = _uiManager.CurrentTheme.ResolveTexture($"TargetDoll/target_{enumName.ToLowerInvariant()}_hover.png");
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
