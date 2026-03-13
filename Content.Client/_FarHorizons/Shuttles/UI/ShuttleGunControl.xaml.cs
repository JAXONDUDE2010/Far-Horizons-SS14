using System.Numerics;
using Content.Client.Shuttles.UI;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Collections;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Client._FarHorizons.Shuttles.UI;

public sealed class ShuttleGunControl : ShuttleNavControl
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IInputManager _inputs = default!;
    private readonly SharedTransformSystem _xformSystem;
    
    private EntityUid? _shuttleEntity;
    
    public ShuttleGunControl() : base()
    {
        RobustXamlLoader.Load(this);
        _xformSystem = EntManager.System<SharedTransformSystem>();
    }
    
    public void SetMap(Vector2 offset, bool recentering = false)
    {
        TargetOffset = offset;
        Recentering = recentering;
    }

    public void SetShuttle(EntityUid? entity) => _shuttleEntity = entity;
    

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);
        var controlLocalBounds = PixelRect;
        var realTime = _timing.RealTime;

        var mousePos = _inputs.MouseScreenPosition;
        var mouseLocalPos = GetLocalPosition(mousePos);

        // Draw dotted line from our own shuttle entity to mouse.
        if (mousePos.Window != WindowId.Invalid)
        {
            // If mouse inbounds then draw it.
            if (_shuttleEntity != null && controlLocalBounds.Contains(mouseLocalPos.Floored()) &&
                EntManager.TryGetComponent(_shuttleEntity, out TransformComponent? shuttleXform) &&
                shuttleXform.MapID != MapId.Nullspace)
            {
                var color = Color.Red;

                // Draw line from our shuttle to target
                // Might need to clip the line if it's too far? But my brain wasn't working so F.
                handle.DrawDottedLine(MidPointVector, mouseLocalPos, color, (float) realTime.TotalSeconds * 30f);

                // Draw shuttle pre-vis
                var mouseVerts = GetMapObject(mouseLocalPos, Angle.Zero, scale: MinimapScale);

                handle.DrawPrimitives(DrawPrimitiveTopology.TriangleFan, mouseVerts.Span, color.WithAlpha(0.05f));
                handle.DrawPrimitives(DrawPrimitiveTopology.LineLoop, mouseVerts.Span, color);
                
                // Draw the coordinates
                var mapOffset = MidPointVector;

                if (mousePos.Window != WindowId.Invalid &&
                    controlLocalBounds.Contains(mouseLocalPos.Floored()))
                {
                    mapOffset = mouseLocalPos;
                }

                var shuttlePos = _xformSystem.GetMapCoordinates(shuttleXform).Position; // This can and should be the position of the gun rather than the shuttle
                //shuttlePos = shuttlePos with {Y = -shuttlePos.Y};

                mapOffset = InverseMapPosition(mapOffset) + Offset + shuttlePos;
                var coordsText = $"{mapOffset.X:0.0}, {mapOffset.Y:0.0}";
                var coordsDimensions = handle.GetDimensions(Font, coordsText, 0.7f);
                var coordUiPosition = mouseLocalPos - new Vector2(coordsDimensions.X / 2, coordsDimensions.Y + 10);
                handle.DrawString(Font, coordUiPosition, coordsText, 0.7f, color);
            }
        }
    }

    private float GetMapObjectRadius(float scale = 1f) => WorldRange / 40f * scale;

    private ValueList<Vector2> GetMapObject(Vector2 localPos, Angle angle, float scale = 1f, bool scalePosition = false)
    {
        // Constant size diamonds
        var diamondRadius = GetMapObjectRadius();

        var mapObj = new ValueList<Vector2>(4)
        {
            localPos + angle.RotateVec(new Vector2(0f, -diamondRadius)) * scale,
            localPos + angle.RotateVec(new Vector2(diamondRadius, 0f)) * scale,
            localPos + angle.RotateVec(new Vector2(0f, diamondRadius)) * scale,
            localPos + angle.RotateVec(new Vector2(-diamondRadius, 0f)) * scale,
        };

        if (scalePosition)
        {
            for (var i = 0; i < mapObj.Count; i++)
            {
                mapObj[i] = ScalePosition(mapObj[i]);
            }
        }

        return mapObj;
    }
}