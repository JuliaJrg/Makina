using UnityEngine;

[System.Serializable]
public struct Coord
{
    public int x;
    public int y;

    public Coord(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static Coord up = new Coord(0, 1);
    public static Coord down = new Coord(0, -1);
    public static Coord left = new Coord(-1, 0);
    public static Coord right = new Coord(1, 0);
    public static Coord invalid = new Coord(-1, -1);

    public static explicit operator Vector2(Coord coord)
    {
        return new Vector2(coord.x, coord.y);
    }

    public static explicit operator Coord(Vector2 vector)
    {
        return new Coord((int)vector.x, (int)vector.y);
    }

    public static int SqrDistance(Coord a, Coord b)
    {
        int dx = a.x - b.x;
        int dy = a.y - b.y;
        return dx * dx + dy * dy;
    }

    public static bool AreNeighbours(Coord a, Coord b)
    {
        return System.Math.Abs(a.x - b.x) <= 1 && System.Math.Abs(a.y - b.y) <= 1;
    }

    public static Coord operator +(Coord a, Coord b)
    {
        return new Coord(a.x + b.x, a.y + b.y);
    }

    public static Coord operator -(Coord a, Coord b)
    {
        return new Coord(a.x - b.x, a.y - b.y);
    }

    public override string ToString()
    {
        return "(" + x + ", " + y + ")";
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Coord))
        {
            return false;
        }

        Coord coord = (Coord)obj;
        return x == coord.x && y == coord.y;
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() ^ y.GetHashCode();
    }

    public static bool operator ==(Coord a, Coord b)
    {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(Coord a, Coord b)
    {
        return !(a == b);
    }
}