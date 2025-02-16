using System.Numerics;
using Content.Client.Examine;
using Content.Client.Gameplay;
using Content.Client.UserInterface.Systems.Viewport;
using Content.KayMisaZlevels.Client;
using Content.Shared.CCVar;
using Content.Shared.Maps;
using Content.Shared.Parallax.Biomes;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Map;

namespace Content.Client._ViewportGui.ViewportUserInterface.Overlays;

public sealed class ViewportUserInterfaceOverlay : Overlay
{
    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
    [Dependency] private readonly IViewportUserInterfaceManager _vpUIManager = default!;
    [Dependency] private readonly IClyde _clyde = default!;

    private ViewportUIController _viewportUIController;

    private Vector2i _viewportPosition;
    private Vector2i _viewportSize;
    private Vector2i _contentSize;

    private IRenderTexture _buffer;

    public ViewportUserInterfaceOverlay()
    {
        IoCManager.InjectDependencies(this);

        _viewportUIController = _uiManager.GetUIController<ViewportUIController>();
        _viewportSize = new Vector2i(_cfg.GetCVar(CCVars.ViewportWidth), ViewportUIController.ViewportHeight);
        // TODO: Move position definition into CVar
        // Or into prototype, instead of using xaml or avalonia
        _viewportPosition = new Vector2i(-3, 0);
        _contentSize = new Vector2i((_viewportSize.X + 4) * EyeManager.PixelsPerMeter, _viewportSize.Y * EyeManager.PixelsPerMeter);

        _cfg.OnValueChanged(CCVars.ViewportWidth, (newValue) =>
        {
            _viewportSize.X = newValue;
            RestoreBuffer();
        });

        _buffer = _clyde.CreateRenderTarget(
            _contentSize,
            RenderTargetColorFormat.Rgba8Srgb);
    }

    private void RestoreBuffer()
    {
        _buffer.Dispose();
        _buffer = _clyde.CreateRenderTarget(
            _contentSize,
            RenderTargetColorFormat.Rgba8Srgb);
    }

    protected override void DisposeBehavior()
    {
        base.DisposeBehavior();

        _buffer.Dispose();
    }

    /*
    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        return true;
    }
    */

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.ScreenHandle;
        var viewport = (args.ViewportControl as ZScalingViewport);
        var uiScale = (args.ViewportControl as ZScalingViewport)?.UIScale ?? 1f;
        var mouseScreenPos = _inputManager.MouseScreenPosition;

        if (viewport is null)
            return;

        var drawBox = viewport.GetDrawBox();
        var drawBoxGlobal = drawBox.Translated(viewport.GlobalPixelPosition);
        var drawBoxGlobalWidth = (drawBoxGlobal.Right - drawBoxGlobal.Left) + 0.0f;
        var drawBoxScale = drawBoxGlobalWidth / (_viewportSize.X * EyeManager.PixelsPerMeter);

        var boundPositionTopLeft = new Vector2(
            drawBoxGlobal.Left + ((_viewportPosition.X * EyeManager.PixelsPerMeter) * drawBoxScale),
            drawBoxGlobal.Top + ((_viewportPosition.Y * EyeManager.PixelsPerMeter) * drawBoxScale));
        var boundPositionBottomRight = new Vector2(
            boundPositionTopLeft.X + (_contentSize.X * drawBoxScale),
            boundPositionTopLeft.Y + (_contentSize.Y * drawBoxScale));

        var boundSize = new UIBox2(boundPositionTopLeft, boundPositionBottomRight);

        var drawingArgs = new ViewportUIDrawArgs(_buffer, _contentSize, boundSize, drawBoxScale, handle);
        _vpUIManager.Draw(drawingArgs);

        handle.DrawTextureRect(_buffer.Texture, boundSize);
    }
}
