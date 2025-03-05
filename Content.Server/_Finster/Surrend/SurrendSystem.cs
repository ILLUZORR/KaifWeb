using Content.Server._White.Notice;
using Content.Server.Chat.Managers;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Shared.ActionBlocker;
using Content.Shared.Alert;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Robust.Shared.Prototypes;

namespace Content.Server._Finster.Surrend;

public sealed class SurrendSystem : EntitySystem
{
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    private ProtoId<AlertPrototype> SurrendAlert = "Surrend";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SurrenderComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<SurrenderComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<SurrenderComponent> ent, ref ComponentStartup args)
    {
        _alerts.ShowAlert(ent, SurrendAlert);
    }

    private void OnShutdown(Entity<SurrenderComponent> ent, ref ComponentShutdown args)
    {
        _alerts.ClearAlert(ent, SurrendAlert);
    }

    public void TrySurrend(EntityUid ent, SurrenderComponent? comp = null, MetaDataComponent? metaData = null)
    {
        if (!Resolve(ent, ref comp) ||
            !Resolve(ent, ref metaData))
            return;

        if (!comp.IsStunable ||
            !_actionBlocker.CanInteract(ent, ent) ||
            _statusEffects.HasStatusEffect(ent, "Stun"))
            return;

        _stun.TryParalyze(ent, TimeSpan.FromSeconds(comp.StunDuration), true);
        _popup.PopupEntity(Loc.GetString("surrend-message", ("name", metaData.EntityName)), ent, PopupType.LargeCaution);
    }
}

[DataDefinition]
public sealed partial class TrySurrend : IAlertClick
{
    public void AlertClicked(EntityUid uid)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        entityManager.System<SurrendSystem>().TrySurrend(uid);
    }
}
