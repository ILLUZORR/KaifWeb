using System.Numerics;
using Content.Client.Examine;
using Content.Client.Gameplay;
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

namespace Content.Client._Finster.Lookup;

public sealed class LookupOverlay : Overlay
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
    private ExamineSystem _examine;

    private Font _font;
    private int _fontScale = 16;

    public LookupOverlay()
    {
        IoCManager.InjectDependencies(this);

        //_biomes = _entManager.System<BiomeSystem>();
        _maps = _entManager.System<SharedMapSystem>();
        _tile = _entManager.System<TileSystem>();
        _xform = _entManager.System<SharedTransformSystem>();
        _examine = _entManager.System<ExamineSystem>();

        _font = new VectorFont(_cache.GetResource<FontResource>("/Fonts/bettervcr.ttf"), _fontScale);
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        var mapUid = _mapManager.GetMapEntityId(args.MapId);

        return _entManager.HasComponent<BiomeComponent>(mapUid);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var viewport = (args.ViewportControl as Control);
        var uiScale = (args.ViewportControl as Control)?.UIScale ?? 1f;

        var mouseScreenPos = _inputManager.MouseScreenPosition;
        var mousePos = _eyeManager.ScreenToMap(mouseScreenPos);

        EntityCoordinates mouseGridPos;
        TileRef? tile = null;

        if (mousePos.MapId == MapId.Nullspace || mousePos.MapId != args.MapId)
            return;

        var mapUid = _mapManager.GetMapEntityId(args.MapId);

        var strContent = "";
        //var nodePos = _maps.WorldToTile(mapUid, grid, mousePos.Position);

        if (mousePos != MapCoordinates.Nullspace)
        {
            if (_mapManager.TryFindGridAt(mousePos, out var mouseGridUid, out var mouseGrid))
            {
                mouseGridPos = _maps.MapToGrid(mouseGridUid, mousePos);
                tile = _maps.GetTileRef(mouseGridUid, mouseGrid, mouseGridPos);
            }
            else
            {
                mouseGridPos = new EntityCoordinates(mapUid, mousePos.Position);
                tile = null;
            }
        }

        var currentState = _stateManager.CurrentState;
        if (currentState is not GameplayStateBase screen)
            return;

        var entityToClick = screen.GetClickedEntity(mousePos);

        if (entityToClick is not null &&
            _entManager.TryGetComponent<MetaDataComponent>(entityToClick, out var metaComp))
        {
            if (_player.LocalEntity is not null && _examine.CanExamine(_player.LocalEntity.Value, entityToClick.Value))
                strContent = metaComp.EntityName;
        }
        else if (tile is not null)
        {
            var tileDef = (ContentTileDefinition) _tileDefManager[tile.Value.Tile.TypeId];
            if (tileDef.ID != ContentTileDefinition.SpaceID)
                strContent = $"{tileDef.Name}";
        }

        if (viewport is null)
            return;

        args.ScreenHandle.DrawString(_font, new Vector2(viewport.Size.X - (viewport.Size.X / 2), viewport.Size.Y - (_fontScale * uiScale)), strContent, uiScale, Color.Gray);
    }
}
