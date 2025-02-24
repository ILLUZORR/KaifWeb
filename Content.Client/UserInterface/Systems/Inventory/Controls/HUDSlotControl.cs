using Content.Client._ViewportGui.ViewportUserInterface;
using Content.Client._ViewportGui.ViewportUserInterface.UI;
using Content.Client.UserInterface.Systems.Inventory.Controls;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;

namespace Content.Client.UserInterface.Systems.Inventory.Controls;

/// <summary>
/// Used for inventory and (maybe) another systems.
/// I HATE THAK INV CODE! I HATE THAK INV CODE! I HATE THAK INV CODE!
/// I HATE THAK INV CODE! I HATE THAK INV CODE! I HATE THAK INV CODE!
/// I HATE THAK INV CODE! I HATE THAK INV CODE! I HATE THAK INV CODE!
/// WizDen can go fuck himself
/// </summary>
public class HUDSlotControl : HUDButton, IEntityControl
{
    [Dependency] private readonly IUserInterfaceManager _UIManager = default!;

    private Texture? _buttonTexture;

    public static int DefaultButtonSize = 32;

    public HUDTextureRect BlockedRect { get; }
    public HUDTextureRect HighlightRect { get; }

    public EntityUid? Entity;
    EntityUid? IEntityControl.UiEntity => Entity;

    private bool _slotNameSet;

    private string _slotName = "";
    public string SlotName
    {
        get => _slotName;
        set
        {
            //this auto registers the button with it's parent container when it's set
            if (_slotNameSet)
            {
                Logger.Warning("Tried to set slotName after init for:" + Name);
                return;
            }
            _slotNameSet = true;
            Name = "SlotButton_" + value;
            _slotName = value;
        }
    }

    public bool Highlight { get => HighlightRect.Visible; set => HighlightRect.Visible = value; }

    public bool Blocked { get => BlockedRect.Visible; set => BlockedRect.Visible = value; }

    private string? _blockedTexturePath;
    public string? BlockedTexturePath
    {
        get => _blockedTexturePath;
        set
        {
            _blockedTexturePath = value;
            BlockedRect.Texture = _UIManager.CurrentTheme.ResolveTextureOrNull(_blockedTexturePath)?.Texture;
        }
    }

    private string? _buttonTexturePath;
    public string? ButtonTexturePath
    {
        get => _buttonTexturePath;
        set
        {
            _buttonTexturePath = value;
            UpdateButtonTexture();
        }
    }

    private string? _fullButtonTexturePath;
    public string? FullButtonTexturePath
    {
        get => _fullButtonTexturePath;
        set
        {
            _fullButtonTexturePath = value;
            UpdateButtonTexture();
        }
    }

    private string? _highlightTexturePath;
    public string? HighlightTexturePath
    {
        get => _highlightTexturePath;
        set
        {
            _highlightTexturePath = value;
            HighlightRect.Texture = _UIManager.CurrentTheme.ResolveTextureOrNull(_highlightTexturePath)?.Texture;
        }
    }

    public event Action<GUIBoundKeyEventArgs, HUDSlotControl>? Pressed;
    public event Action<GUIBoundKeyEventArgs, HUDSlotControl>? Unpressed;
    public event Action<GUIBoundKeyEventArgs, HUDSlotControl>? Hover;

    public bool MouseIsHovering;

    public HUDSlotControl()
    {
        IoCManager.InjectDependencies(this);
        Name = "SlotButton_null";
        Size = (DefaultButtonSize, DefaultButtonSize);

        HighlightTexturePath = "slot_highlight_back";
        BlockedTexturePath = "blocked";

        OnKeyBindDown += OnButtonPressed;
        OnKeyBindUp += OnButtonUnpressed;

        AddChild(BlockedRect = new HUDTextureRect
        {
            Visible = false,
            Size = (DefaultButtonSize, DefaultButtonSize)
        });

        AddChild(HighlightRect = new HUDTextureRect
        {
            Visible = false,
            Size = (DefaultButtonSize, DefaultButtonSize)
        });
    }

    public override void Draw(in ViewportUIDrawArgs args)
    {
        var handle = args.ScreenHandle;

        if (_buttonTexture is null || !VisibleInTree)
        {
            base.Draw(args);
            return;
        }

        handle.DrawTextureRect(_buttonTexture, new UIBox2(GlobalPosition, GlobalPosition + Size));

        base.Draw(args);
    }

    public void SetEntity(EntityUid? ent)
    {
        Entity = ent;
        UpdateButtonTexture();
    }

    private void UpdateButtonTexture()
    {
        var fullTexture = _UIManager.CurrentTheme.ResolveTextureOrNull(_fullButtonTexturePath);
        var texture = Entity.HasValue && fullTexture != null
            ? fullTexture.Texture
            : _UIManager.CurrentTheme.ResolveTextureOrNull(_buttonTexturePath)?.Texture;
        _buttonTexture = texture;
    }

    private void OnButtonPressed(GUIBoundKeyEventArgs args)
    {
        Pressed?.Invoke(args, this);
    }

    private void OnButtonUnpressed(GUIBoundKeyEventArgs args)
    {
        Unpressed?.Invoke(args, this);
    }

    private void OnButtonHover(GUIBoundKeyEventArgs args)
    {
        Hover?.Invoke(args, this);
    }

    public void ClearHover()
    {
    }
}
