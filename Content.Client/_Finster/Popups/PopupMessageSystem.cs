using Content.Client.UserInterface.Systems.Chat;
using Content.Shared.CCVar;
using Content.Shared.Chat;
using Content.Shared.Examine;
using Content.Shared.Popups;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Shared.Configuration;

namespace Content.Client._Finster.Popups;

public sealed class PopupMessageSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IUserInterfaceManager _ui = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly ExamineSystemShared _examine = default!;

    private bool _enabledIcons;

    private const string FontSize = "10";

    private const string CautionColor = "c62828";
    private const string BaseColor = "aeabc4";

    private readonly Dictionary<PopupType, string> _fontSizeDict = new ()
    {
        { PopupType.Medium, "12" },
        { PopupType.MediumCaution, "12" },
        { PopupType.Large, "15" },
        { PopupType.LargeCaution, "15" }
    };


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MetaDataComponent, EntityIconPopupedEvent>(OnIconPopup);

        _cfg.OnValueChanged(CCVars.ChatPointingVisuals, b => _enabledIcons = b, true);
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _cfg.UnsubValueChanged(CCVars.ChatPointingVisuals, b => _enabledIcons = b);
    }

    public void DoMessage(string message, PopupType type, string? tags = null, bool ignoreChatStack = false)
    {
        var fontsize = _fontSizeDict.GetValueOrDefault(type, FontSize);
        var fontcolor = type is PopupType.LargeCaution or PopupType.MediumCaution or PopupType.SmallCaution
            ? CautionColor
            : BaseColor;

        var wrappedMessage = $"[font size={fontsize}][color=#{fontcolor}]{message + tags}[/color][/font]";

        var chatMsg = new ChatMessage(ChatChannel.Emotes,
            message,
            wrappedMessage,
            NetEntity.Invalid,
            null);
        chatMsg.IgnoreChatStack = ignoreChatStack;

        _ui.GetUIController<ChatUIController>().ProcessChatMessage(chatMsg);
    }

    private void OnIconPopup(Entity<MetaDataComponent> ent, ref EntityIconPopupedEvent args)
    {
        DoIconMessage(ent, args.Message, args.Type);
    }

    public void DoIconMessage(EntityUid ent, string message, PopupType type)
    {
        if (!_enabledIcons)
            return;

        if (!TryComp<MetaDataComponent>(ent, out var metaData))
            return;

        /*
        if (_player.LocalEntity == null)
            return;

        if (!_examine.InRangeUnOccluded(_player.LocalEntity.Value, Transform(ent).Coordinates, 10))
            return;
        */

        var fontsize = _fontSizeDict.GetValueOrDefault(type, FontSize);
        var tag = Loc.GetString("ent-texture-tag", ("id", ent.Id), ("size", fontsize));
        DoMessage(message, type, tag, true);
    }
}

public sealed class EntityIconPopupedEvent : EntityEventArgs
{
    public readonly string Message;
    public readonly PopupType Type;

    public EntityIconPopupedEvent(string message, PopupType type)
    {
        Message = message;
        Type = type;
    }
}
