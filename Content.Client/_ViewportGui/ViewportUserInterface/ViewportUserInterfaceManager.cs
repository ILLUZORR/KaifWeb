using System.Numerics;
using Content.Client._ViewportGui.ViewportUserInterface.UI;
using Content.Client.Resources;
using Content.KayMisaZlevels.Client;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Client._ViewportGui.ViewportUserInterface;

public interface IViewportUserInterfaceManager
{
    /// <summary>
    /// Contain position, size, content size and another useful info.
    /// </summary>
    ViewportDrawingInfo? DrawingInfo { get; set; }

    /// <summary>
    /// Viewport control. Should be set up by Overlay/Initialize logic.
    /// Should be used for KeyBind function for mouse buttons.
    /// </summary>
    ZScalingViewport? Viewport { get; set; }

    /// <summary>
    /// Contains all HUD elements near viewport. And should do only that.
    /// </summary>
    HUDRoot? Root { get; }

    /// <summary>
    /// Can we interact with world objects on screen position.
    /// If VP-GUI element is focused - we should not do any interactions by another systems.
    /// </summary>
    bool CanMouseInteractInWorld { get; }

    void Initialize();
    void FrameUpdate(FrameEventArgs args);
    void Draw(ViewportUIDrawArgs args);

    ViewportDrawBounds? GetDrawingBounds();
}

public sealed class ViewportUserInterfaceManager : IViewportUserInterfaceManager
{
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    private ZScalingViewport? _viewport;

    public ViewportDrawingInfo? DrawingInfo { get; set; }

    public ZScalingViewport? Viewport
    {
        get => _viewport;
        set
        {
            _viewport = value;
            ResolveKeyBinds();
        }
    }

    public HUDRoot? Root { get; private set; }

    public bool CanMouseInteractInWorld { get; private set; } = true;

    public void Initialize()
    {
        Root = new HUDRoot();
        CanMouseInteractInWorld = true;

        // Testing
        var textRect = new HUDTextureRect();
        Root.AddChild(textRect);
        textRect.Size = (96, 480);
        textRect.Position = (0, 0);
        textRect.Texture = _resourceCache.GetTexture("/Textures/Interface/LoraAshen/left_panel_background_full.png");
    }

    public void FrameUpdate(FrameEventArgs args)
    {
    }

    public void Draw(ViewportUIDrawArgs args)
    {
        var handle = args.ScreenHandle;

        CheckWorldInteraction(args);

        args.ScreenHandle.RenderInRenderTarget(args.RenderTexture, () =>
        {
            Root?.Draw(args);

            // Debug bounds drawing
            //handle.DrawRect(new UIBox2(new Vector2(0, 0), args.ContentSize), Color.Green.WithAlpha(0.5f), true);
        }, Color.Transparent);
    }

    public ViewportDrawBounds? GetDrawingBounds()
    {
        if (_viewport is null || DrawingInfo is null)
            return null;

        var drawBox = _viewport.GetDrawBox();
        var drawBoxGlobal = drawBox.Translated(_viewport.GlobalPixelPosition);
        var drawBoxGlobalWidth = (drawBoxGlobal.Right - drawBoxGlobal.Left) + 0.0f;
        var drawBoxScale = drawBoxGlobalWidth / (DrawingInfo.Value.ViewportSize.X * EyeManager.PixelsPerMeter);

        var boundPositionTopLeft = new Vector2(
            drawBoxGlobal.Left + ((DrawingInfo.Value.ViewportPosition.X * EyeManager.PixelsPerMeter) * drawBoxScale),
            drawBoxGlobal.Top + ((DrawingInfo.Value.ViewportPosition.Y * EyeManager.PixelsPerMeter) * drawBoxScale));
        var boundPositionBottomRight = new Vector2(
            boundPositionTopLeft.X + (DrawingInfo.Value.ContentSize.X * drawBoxScale),
            boundPositionTopLeft.Y + (DrawingInfo.Value.ContentSize.Y * drawBoxScale));

        var boundsSize = new UIBox2(boundPositionTopLeft, boundPositionBottomRight);

        return new ViewportDrawBounds(boundsSize, drawBoxScale);
    }

    private void OnKeyBindDown(GUIBoundKeyEventArgs args)
    {
    }

    private void OnKeyBindUp(GUIBoundKeyEventArgs args)
    {
    }

    private void ResolveKeyBinds()
    {
        if (_viewport is null)
            return;

        _viewport.OnKeyBindDown += OnKeyBindDown;
        _viewport.OnKeyBindUp += OnKeyBindUp;
    }

    private void CheckWorldInteraction(ViewportUIDrawArgs args)
    {
        if (Root is null)
            return;

        var mouseScreenPos = _inputManager.MouseScreenPosition;
        // TODO: FUCK NO!!! Firstly we need translate mouseScreenPosition to local viewport's ContentSize
        CanMouseInteractInWorld = !DoCheckControlsBounds(Root, new Vector2i((int) mouseScreenPos.X, (int) mouseScreenPos.Y));
    }

    private bool DoCheckControlsBounds(HUDControl uicontrol, Vector2i mousePos)
    {
        if (InControlBounds(uicontrol.Position, uicontrol.Size, mousePos))
            return true;

        foreach (var control in uicontrol.Children)
        {
            if (InControlBounds(control.Position, control.Size, mousePos))
                return true;
        }

        return false;
    }

    private bool InControlBounds(Vector2i controlPos, Vector2i controlSize, Vector2i pos)
    {
        var bounds = new UIBox2i(controlPos, controlPos + controlSize);
        var mouseBounds = new UIBox2i(pos, pos);

        return bounds.Intersects(mouseBounds);
    }
}

public struct ViewportDrawBounds
{
    public UIBox2 DrawBox { get; set; }
    public float Scale { get; set; }

    public ViewportDrawBounds(UIBox2 drawBox, float scale)
    {
        DrawBox = drawBox;
        Scale = scale;
    }
}

public struct ViewportDrawingInfo
{
    public Vector2i ViewportPosition { get; set; }
    public Vector2i ViewportSize { get; set; }
    public Vector2i ContentSize { get; set; }

    public ViewportDrawingInfo(
        Vector2i viewportPosition,
        Vector2i viewportSize,
        Vector2i contentSize)
    {
        ViewportPosition = viewportPosition;
        ViewportSize = viewportSize;
        ContentSize = contentSize;
    }
}
