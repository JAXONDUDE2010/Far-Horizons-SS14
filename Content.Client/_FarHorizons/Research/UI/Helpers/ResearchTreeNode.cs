using System.Numerics;
using Content.Shared._FarHorizons.Research;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;

namespace Content.Client._FarHorizons.Research.UI.Helpers;

public struct DrawResearchNode
    (
        ProtoId<ResearchTreeNodePrototype> proto,
        ProtoId<ResearchTreeTierPrototype> tier,
        string name,
        Vector2? pos = null,
        Vector2? size = null,
        Font? font = null,
        List<string>? text = null,
        int textSize = 0,
        float? progress = null,
        float fontScale = 0.5f,
        bool hovered = false,
        bool selected = false,
        bool unlocked = false,
        bool completed = false
    )
{
    public ProtoId<ResearchTreeNodePrototype> Proto = proto;
    public ProtoId<ResearchTreeTierPrototype> Tier = tier;
    public string Name = name;
    public List<string> Text = text ?? [];
    public int TextSize = textSize;
    public float? Progress = progress;
    public bool Unlocked = unlocked;
    public bool Completed = completed;
    public bool Highlight = hovered;
    public bool Selected = selected;
    private Vector2? _position = pos;
    public Vector2 Position
    {
        get => _position ?? (Vector2)(_position = Vector2.Zero);
        set => _position = value;
    }
    private Vector2? _size = size;
    public readonly Vector2 Size
    {
        get
        {
            var size = _size ?? Vector2.Zero;

            var width = size.X;
            var height = size.Y + TextSize;

            if (Progress != null && !Completed)
                height += ProgressHeight / 2;

            return new(width, height);
        }
    }
    public UIBox2 Box => 
        new(Position, Position + Size);
    public Vector2 Center => 
        Position + (Size / 2);
    public Font? Font = font;
    public float FontScale = fontScale;

    public readonly Color ForegroundColor =>
        !Unlocked ? LockedColor :
        !Completed ? 
            UnlockedColor : ResearchedColor;

    private const int NodeMargin = 2;
    private const int SelectedMargin = 2;
    private const int ProgressHeight = 8;

    private readonly Color BackgroundColor = Color.Black;
    private readonly Color LockedColor = Color.Gray;
    private readonly Color UnlockedColor = Color.White;
    private readonly Color ResearchedColor = Color.DarkGreen;
    private readonly Color ResearchingColor = Color.LightBlue;
    private readonly Color SelectionColor = Color.Yellow;

    public DrawResearchNode(DrawResearchNode other) 
        : this(
            other.Proto, 
            other.Tier,
            other.Name,
            other.Position,
            other._size,
            other.Font,
            other.Text,
            other.TextSize,
            other.Progress,
            other.FontScale,
            other.Highlight,
            other.Selected,
            other.Unlocked,
            other.Completed){}
    
    public DrawResearchNode WrapName(DrawingHandleScreen handle)
    {
        if (Text.Count != 0 || Font == null)
            return this;
        
        List<string> text = [];

        var dimensions = handle.GetDimensions(Font!, Name, FontScale);
        if (dimensions.X < Size.X - (NodeMargin * 2))
            text.Add(Name);
        else
        {
            var line = "";
            foreach (var word in Name.Split(' '))
            {
                var newLine = line == "" ? word : line + " " + word;
                var newDimensions = handle.GetDimensions(Font!, newLine, FontScale);
                if (newDimensions.X >= Size.X - (NodeMargin * 2))
                {
                    text.Add(line);
                    line = word;
                } else {
                    line = newLine;
                }
            }
            text.Add(line);
        }

        return new(this)
        {
            Text = text,
            TextSize = (int)dimensions.Y * (text.Count - 1),
        };
    }

    public DrawResearchNode Translate(Vector2 offset) =>
        new(this)
        {
            Position = Position + offset,
        };

    public DrawResearchNode Hovered(Vector2 mousePos) =>
        new(this)
        {
            Highlight = IsHovering(mousePos),
        };

    public DrawResearchNode Select(ProtoId<ResearchTreeNodePrototype>? target) =>
        new(this)
        {
            Selected = target == Proto,
        };

    public DrawResearchNode Researching(Dictionary<ProtoId<ResearchTreeNodePrototype>, float> researching) =>
        new(this)
        {
            Progress = researching.TryGetValue(Proto, out var value) ? value : null,
        };
    
    public DrawResearchNode Unlock(HashSet<ProtoId<ResearchTreeTierPrototype>> unlockedTiers, HashSet<ProtoId<ResearchTreeNodePrototype>> unlockedNodes) =>
        new(this)
        {
            Unlocked = unlockedTiers.Contains(Tier) && unlockedNodes.Contains(Proto),
        };

    public DrawResearchNode Research(HashSet<ProtoId<ResearchTreeNodePrototype>> allResearched) =>
        new(this)
        {
            Completed = allResearched.Contains(Proto),
        };

    public bool IsHovering(Vector2 mousePos) =>
        mousePos.X > Position.X && mousePos.Y > Position.Y && mousePos.X < Position.X + Size.X && mousePos.Y < Position.Y + Size.Y;

    public void Draw(DrawingHandleScreen handle)
    {
        if (Selected)
            handle.DrawRect(new(Box.TopLeft - (Vector2.One * SelectedMargin), Box.BottomRight + (Vector2.One * SelectedMargin)), SelectionColor, true);

        handle.DrawRect(Box, ForegroundColor, true);

        if (!Highlight)
            handle.DrawRect(new(Box.TopLeft + (Vector2.One * NodeMargin), Box.BottomRight - (Vector2.One * NodeMargin)), BackgroundColor, true);

        if (Progress != null && !Completed)
        {
            var progressBarWidth = (Size.X - (NodeMargin * 2)) * Progress!.Value;
            handle.DrawRect(new(new(Box.BottomLeft.X + NodeMargin, Box.BottomLeft.Y - ProgressHeight - NodeMargin), new(Box.BottomLeft.X + NodeMargin + progressBarWidth, Box.BottomRight.Y - NodeMargin)), Color.Green);
        }

        if (Font != null && Text.Count > 0)
        {
            for (var i = 0; i < Text.Count; i++)
            {
                var dimensions = handle.GetDimensions(Font, Text[i], FontScale);
                Vector2 pos = new(Center.X - (dimensions.X / 2), Position.Y + (dimensions.Y * i) + (dimensions.Y / 2));
                handle.DrawString(Font, pos, Text[i], FontScale, Highlight ? BackgroundColor : ForegroundColor);
            }
        }
    }
}