using System.ComponentModel;

public enum Direction
{
    [field: Description("tileUp")]
    tileUp,

    [field: Description("tileDown")]
    tileDown,

    [field: Description("tileLeft")]
    tileLeft,

    [field: Description("tileRight")]
    tileRight
}