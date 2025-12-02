using System.Linq;
using System.Numerics;
using Content.Shared._FarHorizons.Research;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;

namespace Content.Client._FarHorizons.Research.UI.Helpers;

public struct DrawResearchEdge(Vector2 a, Vector2 b, bool highlight = false, List<ProtoId<ResearchTreeNodePrototype>>? linked = null, bool researched = false)
{
    public Vector2 A = a;
    public Vector2 B = b;

    public bool Highlight = highlight;
    public bool Researched = researched;

    public readonly Color LineColor =>
        Highlight ? HighlightColor :
        Researched ? ResearchedColor :
        NotReserachedColor;

    private readonly Color NotReserachedColor = Color.Gray;
    private readonly Color ResearchedColor = Color.DarkGreen;
    private readonly Color HighlightColor = Color.White;

    public List<ProtoId<ResearchTreeNodePrototype>> Linked = linked ?? [];

    public DrawResearchEdge(DrawResearchEdge other)
        : this(other.A, other.B, other.Highlight, other.Linked, other.Researched){}

    public DrawResearchEdge Translate(Vector2 offset) =>
        new(this)
        {
            A = A + offset,
            B = B + offset,
        };

    public DrawResearchEdge Hovered(ProtoId<ResearchTreeNodePrototype>? hovered) =>
        new(this)
        {
            Highlight = hovered != null && Linked.Contains(hovered.Value),
        };
    
    public DrawResearchEdge Research(HashSet<ProtoId<ResearchTreeNodePrototype>> allResearched) =>
        new(this)
        {
            Researched = Linked.Any(allResearched.Contains),
        };

    public readonly void Draw(DrawingHandleScreen handle) => 
        handle.DrawLine(A, B, LineColor);
}