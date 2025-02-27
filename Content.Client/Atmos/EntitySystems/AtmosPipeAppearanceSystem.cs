using Content.Client.SubFloor;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Atmos.Piping;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.ResourceManagement;

namespace Content.Client.Atmos.EntitySystems;

[UsedImplicitly]
public sealed class AtmosPipeAppearanceSystem : EntitySystem
{
    [Dependency] private readonly IResourceCache _resCache = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PipeAppearanceComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<PipeAppearanceComponent, AppearanceChangeEvent>(OnAppearanceChanged, after: new[] { typeof(SubFloorHideSystem) });
    }

    private void OnInit(EntityUid uid, PipeAppearanceComponent component, ComponentInit args)
    {
        if (!TryComp(uid, out SpriteComponent? sprite))
            return;

        if (!_resCache.TryGetResource(SharedSpriteComponent.TextureRoot / component.RsiPath, out RSIResource? rsi))
        {
            Logger.Error($"{nameof(AtmosPipeAppearanceSystem)} could not load to load RSI {component.RsiPath}.");
            return;
        }

        foreach (PipeConnectionLayer layerKey in Enum.GetValues(typeof(PipeConnectionLayer)))
        {
            sprite.LayerMapReserveBlank(layerKey);
            var layer = sprite.LayerMapGet(layerKey);
            sprite.LayerSetRSI(layer, rsi.RSI);
            var layerState = component.BaseState + (PipeDirection) layerKey;
            sprite.LayerSetState(layer, layerState);
        }
    }

    private void OnAppearanceChanged(EntityUid uid, PipeAppearanceComponent component, ref AppearanceChangeEvent args)
    {
        if (!TryComp(uid, out SpriteComponent? sprite))
            return;

        if (!args.Component.TryGetData(PipeColorVisuals.Color, out Color color))
            color = Color.White;

        if (!args.Component.TryGetData(PipeVisuals.VisualState, out PipeDirection connectedDirections))
            return;

        var rotation = Transform(uid).LocalRotation;

        foreach (PipeConnectionLayer layerKey in Enum.GetValues(typeof(PipeConnectionLayer)))
        {
            if (!sprite.LayerMapTryGet(layerKey, out var key))
                continue;

            var layer = sprite[key];
            var dir = (PipeDirection) layerKey;
            var visible = connectedDirections.HasDirection(dir);

            layer.Visible &= visible;

            if (!visible) continue;

            layer.Rotation = -rotation;
            layer.Color = color;
        }
    }

    private enum PipeConnectionLayer : byte
    {
        NorthConnection = PipeDirection.North,
        SouthConnection = PipeDirection.South,
        EastConnection = PipeDirection.East,
        WestConnection = PipeDirection.West,
    }
}
