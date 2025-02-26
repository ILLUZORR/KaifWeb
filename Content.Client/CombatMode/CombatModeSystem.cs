using Content.Client.Hands.Systems;
using Content.Client.NPC.HTN;
using Content.Shared.CCVar;
using Content.Shared.CombatMode;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.Audio;
using Robust.Client.ResourceManagement;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio.Sources;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Client.CombatMode;

public sealed class CombatModeSystem : SharedCombatModeSystem
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IEyeManager _eye = default!;
    [Dependency] private readonly IResourceCache _cache = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IAudioManager _audioManager = default!;

    /// <summary>
    /// Raised whenever combat mode changes.
    /// </summary>
    public event Action<bool>? LocalPlayerCombatModeUpdated;

    /// <summary>
    /// Raised whan UpdateHud has been called.
    /// </summary>
    public event Action<bool, bool>? LocalPlayerCombatModeHudUpdate;

    public Action<EntityUid>? LocalPlayerAttached;
    public Action<EntityUid>? LocalPlayerDetached;

    private IAudioSource? _combatOnSource;
    private IAudioSource? _combatOffSource;

    private float _interfaceGain;
    private const float ClickGain = 0.25f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CombatModeComponent, AfterAutoHandleStateEvent>(OnHandleState);
        SubscribeLocalEvent<CombatModeComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<CombatModeComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        _cfg.OnValueChanged(CCVars.InterfaceVolume, SetInterfaceVolume, true);

        Subs.CVar(_cfg, CCVars.CombatModeIndicatorsPointShow, OnShowCombatIndicatorsChanged, true); // FINSTER EDIT
        SetCombatSounds(_cfg.GetCVar(CCVars.UICombatModeOnSound), _cfg.GetCVar(CCVars.UICombatModeOffSound));
    }

    private void SetInterfaceVolume(float obj)
    {
        _interfaceGain = obj;

        if (_combatOnSource != null)
        {
            _combatOnSource.Gain = ClickGain * _interfaceGain;
        }

        if (_combatOffSource != null)
        {
            _combatOffSource.Gain = ClickGain * _interfaceGain;
        }
    }

    private void SetCombatSounds(string combatOnFile, string combatOffFile)
    {
        if (!string.IsNullOrEmpty(combatOnFile) &&
            !string.IsNullOrEmpty(combatOffFile) )
        {
            var resourceCombatOn = _cache.GetResource<AudioResource>(combatOnFile);
            var resourceCombatOff = _cache.GetResource<AudioResource>(combatOffFile);

            var sourceCombatOn =
                _audioManager.CreateAudioSource(resourceCombatOn);
            var sourceCombatOff =
                _audioManager.CreateAudioSource(resourceCombatOff);

            if (sourceCombatOn != null)
            {
                sourceCombatOn.Gain = ClickGain * _interfaceGain;
                sourceCombatOn.Global = true;
            }
            if (sourceCombatOff != null)
            {
                sourceCombatOff.Gain = ClickGain * _interfaceGain;
                sourceCombatOff.Global = true;
            }

            _combatOnSource = sourceCombatOn;
            _combatOffSource = sourceCombatOff;
        }
        else
        {
            _combatOnSource = null;
            _combatOffSource = null;
        }
    }

    private void OnHandleState(EntityUid uid, CombatModeComponent component, ref AfterAutoHandleStateEvent args)
    {
        UpdateHud(uid);
    }

    private void OnPlayerDetached(EntityUid uid, CombatModeComponent component, LocalPlayerDetachedEvent args)
    {
        LocalPlayerAttached?.Invoke(uid);
    }

    private void OnPlayerAttached(EntityUid uid, CombatModeComponent component, LocalPlayerAttachedEvent args)
    {
        LocalPlayerDetached?.Invoke(uid);
    }

    public override void Shutdown()
    {
        _overlayManager.RemoveOverlay<CombatModeIndicatorsOverlay>();

        base.Shutdown();
    }

    public bool IsInCombatMode()
    {
        var entity = _playerManager.LocalEntity;

        if (entity == null)
            return false;

        return IsInCombatMode(entity.Value);
    }

    public void LocalToggleCombatMode()
    {
        var uid = _playerManager.LocalEntity;

        if (uid == null)
            return;

        if (!TryComp(uid, out CombatModeComponent? comp))
            return;

        //PerformAction(uid.Value, comp, uid.Value);
        RaiseNetworkEvent(new ToggleCombatModeEvent());
    }

    public override void SetInCombatMode(EntityUid entity, bool value, CombatModeComponent? component = null)
    {
        base.SetInCombatMode(entity, value, component);
        UpdateHud(entity);
    }

    protected override bool IsNpc(EntityUid uid)
    {
        return HasComp<HTNComponent>(uid);
    }

    private void UpdateHud(EntityUid entity)
    {
        if (entity != _playerManager.LocalEntity)
            return;

        var inCombatMode = IsInCombatMode();
        LocalPlayerCombatModeHudUpdate?.Invoke(inCombatMode, Timing.IsFirstTimePredicted);
        PlayToggleSound(inCombatMode);

        if (!Timing.IsFirstTimePredicted)
            return;

        LocalPlayerCombatModeUpdated?.Invoke(inCombatMode);
    }

    private void PlayToggleSound(bool isToggled)
    {
        if (isToggled)
            _combatOnSource?.Restart();
        else
            _combatOffSource?.Restart();
    }

    private void OnShowCombatIndicatorsChanged(bool isShow)
    {
        if (isShow)
        {
            _overlayManager.AddOverlay(new CombatModeIndicatorsOverlay(
                _inputManager,
                EntityManager,
                _eye,
                this,
                EntityManager.System<HandsSystem>()));
        }
        else
        {
            _overlayManager.RemoveOverlay<CombatModeIndicatorsOverlay>();
        }
    }
}
