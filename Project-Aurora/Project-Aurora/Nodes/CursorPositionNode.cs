using System.Windows.Forms;

namespace AuroraRgb.Nodes;

public class CursorPositionNode : Node
{
    public static float X => Cursor.Position.X;
    public static float Y => Cursor.Position.Y;
}