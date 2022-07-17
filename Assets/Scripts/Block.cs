public static class Block
{
    public const int BlockCount = 6;
    public const int SideCount = 6;

    public enum Id
    {
        Air,
        WhiteTile,
        BlackTile,
        Die,
        PositiveDie,
        NegativeDie
    }

    public static int GetTextureId(Id blockId, Direction.Axis sideId)
    {
        return (int)sideId + (int)blockId * SideCount;
    }
}
