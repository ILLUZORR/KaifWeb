using System.Numerics;
using Content.Client.Examine;
using Content.Client.Gameplay;
using Content.KayMisaZlevels.Client;
using Content.Shared.Maps;
using Content.Shared.Parallax.Biomes;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Shared.Enums;
using Robust.Shared.Map;

namespace Content.Client._Finster.ViewportGui;

public sealed class VPGuiOverlay : Overlay
{
    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IResourceCache _cache = default!;
    [Dependency] private readonly IStateManager _stateManager = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefManager = default!;

    // private BiomeSystem _biomes;
    private SharedMapSystem _maps;
    private TileSystem _tile;
    private SharedTransformSystem _xform;

    private Font _font;
    private int _fontScale = 16;

    public VPGuiOverlay()
    {
        IoCManager.InjectDependencies(this);

        //_biomes = _entManager.System<BiomeSystem>();
        _maps = _entManager.System<SharedMapSystem>();
        _tile = _entManager.System<TileSystem>();
        _xform = _entManager.System<SharedTransformSystem>();

        _font = new VectorFont(_cache.GetResource<FontResource>("/Fonts/bettervcr.ttf"), _fontScale);
    }

    /*
    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        var mapUid = _mapManager.GetMapEntityId(args.MapId);

        return _entManager.HasComponent<BiomeComponent>(mapUid);
    }
    */

    protected override void Draw(in OverlayDrawArgs args)
    {
        var viewport = (args.ViewportControl as ZScalingViewport);
        var uiScale = (args.ViewportControl as ZScalingViewport)?.UIScale ?? 1f;

        var handle = args.ScreenHandle;

        var mouseScreenPos = _inputManager.MouseScreenPosition;
        var mousePos = _eyeManager.ScreenToMap(mouseScreenPos);

        if (viewport is null)
            return;

        var drawBox = viewport.GetDrawBox();
        var drawBoxGlobal = drawBox.Translated(viewport.GlobalPixelPosition);
        //var center = viewport.PixelSizeBox.Center; // drawBoxGlobal.Center;
        var center = drawBoxGlobal.Right - ((drawBoxGlobal.Right - drawBoxGlobal.Left) / 2);
        var bottom = viewport.PixelSizeBox.Bottom;

        // Left
        handle.DrawRect(new UIBox2(
            new Vector2(drawBoxGlobal.Left - (EyeManager.PixelsPerMeter * 3), drawBoxGlobal.Top), new Vector2(drawBoxGlobal.Left, drawBoxGlobal.Bottom)
        ), Color.Red, true);

        // Right
        handle.DrawRect(new UIBox2(
            new Vector2(drawBoxGlobal.Right, drawBoxGlobal.Top), new Vector2(drawBoxGlobal.Right + EyeManager.PixelsPerMeter, drawBoxGlobal.Bottom)
        ), Color.Green, true);
    }
}
