using System;
using UnityEngine;
public enum SlotType
{
    Tableau,
    Freecell,
    Foundation,
    Temp
}
public enum MoveKind
{
    None,
    TableauToTableau,
    TableauToCell,
    CellToTableau,
    TableauToFoundation,
    CellToFoundation,
    FoundationToTableau,
    FoundationToCell,
}
public class Move
{
    public MoveKind Kind;
    public int From;
    public int To;
    public int Count;

    public Move(MoveKind kind, int from, int to, int count = 1)
    {
        Kind = kind;
        From = from;
        To = to;
        Count = count;
    }
    public override string ToString() => $"[{Kind}] {From} => {To}{(Count > 1 ?$", {Count} cards.":string.Empty)}";
    public override bool Equals(object obj)
    {
        try
        {
            var om = (Move)obj;
            return Kind == om.Kind && From == om.From && To == om.To && Count == om.Count;
        }
        catch{ }
        return false;
        
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(Kind, From, To, Count);
    }
}


public static class MoveClassifier
{
    internal static MoveKind Classify(SlotModel fromModel, SlotModel toModel)
    {
        var f = fromModel.Type;
        var t = toModel.Type;
        if(f == SlotType.Tableau && t == SlotType.Tableau) return MoveKind.TableauToTableau;
        else if (f == SlotType.Tableau && t == SlotType.Freecell) return MoveKind.TableauToCell;
        else if (f == SlotType.Tableau && t == SlotType.Foundation) return MoveKind.TableauToFoundation;
        else if (f == SlotType.Tableau && t == SlotType.Tableau) return MoveKind.TableauToTableau;
        else if (f == SlotType.Freecell && t == SlotType.Foundation) return MoveKind.CellToFoundation;
        else if (f == SlotType.Freecell && t == SlotType.Tableau) return MoveKind.CellToTableau;
        else if (f == SlotType.Foundation && t == SlotType.Freecell) return MoveKind.FoundationToCell;
        else if (f == SlotType.Foundation && t == SlotType.Tableau) return MoveKind.FoundationToTableau;
        else return MoveKind.None;
        // else throw new ArgumentException($"[{f} to {t}] is invaild move kind.");
    }
}
