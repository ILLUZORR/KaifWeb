using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Utility;
using Robust.Shared.Graphics.RSI;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client._ViewportGui.ViewportUserInterface.UI;

public sealed class HUDAnimatedTextureRect : HUDTextureRect
{
    private IRsiStateLike? _state;
    private int _curFrame;
    private float _curFrameTime;

    public RsiDirection RsiDirection { get; } = RsiDirection.South;

    public HUDAnimatedTextureRect()
    {
        IoCManager.InjectDependencies(this);
    }

    public void SetFromSpriteSpecifier(SpriteSpecifier specifier)
    {
        _curFrame = 0;
        _state = specifier.RsiStateLike();
        _curFrameTime = _state.GetDelay(0);
        Texture = _state.GetFrame(RsiDirection, 0);
    }

    public override void FrameUpdate(FrameEventArgs args)
    {
        if (_state == null || !_state.IsAnimated)
            return;

        var oldFrame = _curFrame;

        _curFrameTime -= args.DeltaSeconds;
        while (_curFrameTime < _state.GetDelay(_curFrame))
        {
            _curFrame = (_curFrame + 1) % _state.AnimationFrameCount;
            _curFrameTime += _state.GetDelay(_curFrame);
        }

        if (_curFrame != oldFrame)
        {
            Texture = _state.GetFrame(RsiDirection, _curFrame);
        }
    }
}
