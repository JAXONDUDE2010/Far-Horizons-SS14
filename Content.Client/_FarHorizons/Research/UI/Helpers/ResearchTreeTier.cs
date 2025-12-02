using System.Numerics;
using Robust.Client.Graphics;

namespace Content.Client._FarHorizons.Research.UI.Helpers;

public struct DrawResearchTier (string name, Font? font, int? left, int? right, Color bgColor, Vector2? pos = null)
{
    public string Name = name;
    public Font? Font = font;
    public int? Left = left;
    public int? Right = right;

    public Color BgColor = bgColor;

    private Vector2? _position = pos;
    public Vector2 Position
    {
        get => _position ?? (Vector2)(_position = Vector2.Zero);
        set => _position = value;
    }

    public UIBox2 Box => 
        new(new(Left == null ? 0 : Left.Value + Position.X, 0), 
            new(Right == null ? 3000 : Right.Value + Position.X, 3000)); //Imma just hardcode some large nubmers here and assume you don't have a screen large enough. Surely nobody will have 3k pixels in a single window, right?

    public DrawResearchTier (DrawResearchTier other) : this(other.Name, other.Font, other.Left, other.Right, other.BgColor, other.Position){}

    public DrawResearchTier Translate(Vector2 offset) =>
        new(this)
        {
            Position = Position + offset,
        };

    public DrawResearchTier RemoveRight() =>
        new(this)
        {
            Right = null,
        };

    public void DrawBg(DrawingHandleScreen handle) => 
        handle.DrawRect(Box, BgColor, true);

    public void DrawHeader(DrawingHandleScreen handle)
    {
        if (Font != null)
        {
            var dimensions = handle.GetDimensions(Font, Name, 0.8f);
            Vector2 pos = new(Math.Max(0, Box.Left), 0);

            handle.DrawRect(new(Box.TopLeft, new(Box.Right, Box.Top + dimensions.Y)), Color.Black, true);

            var size = Box.Right - Math.Max(0, Box.Left);
            if (size > dimensions.X)
                handle.DrawString(Font, pos, Name, 0.8f, Color.White);
        }
    }
}