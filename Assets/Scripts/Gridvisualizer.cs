using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
//  GridVisualizer
//  Attach to any GameObject in SampleScene.
//  Draws visible grid boxes in Scene view so you can align them to background.
//  Adjust position, cellWidth, cellHeight in Inspector until boxes match background.
//  Then read slot positions and give to Claude.
// ─────────────────────────────────────────────────────────────────────────────
public class GridVisualizer : MonoBehaviour
{
    [Header("Cell Size")]
    public float cellWidth = 0.45f;
    public float cellHeight = 0.70f;

    [Header("Box 1 - top left (2x3)")]
    public Vector2 box1Origin = new Vector2(-3f, 5f);

    [Header("Box 2 - top middle (2x3)")]
    public Vector2 box2Origin = new Vector2(0f, 5f);

    [Header("Box 3 - top right (2x2)")]
    public Vector2 box3Origin = new Vector2(3f, 4f);

    [Header("Box 4 - middle left (2x2)")]
    public Vector2 box4Origin = new Vector2(-3.2f, 0.55f);

    [Header("Box 5 - middle centre (2x2)")]
    public Vector2 box5Origin = new Vector2(-0.45f, 1.1f);

    [Header("Box 6 - middle right (2x2)")]
    public Vector2 box6Origin = new Vector2(2.3f, 1.1f);

    [Header("Box 7 - bottom left (2x2)")]
    public Vector2 box7Origin = new Vector2(-3.4f, -1.8f);

    [Header("Box 8 - bottom middle (2x3)")]
    public Vector2 box8Origin = new Vector2(-0.65f, -1.8f);

    [Header("Box 9 - bottom right (2x2)")]
    public Vector2 box9Origin = new Vector2(2.1f, -1.8f);

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        DrawBox(box1Origin, 2, 3, Color.cyan);
        DrawBox(box2Origin, 2, 3, Color.cyan);
        DrawBox(box3Origin, 2, 2, Color.green);
        DrawBox(box4Origin, 2, 2, Color.green);
        DrawBox(box5Origin, 2, 2, Color.green);
        DrawBox(box6Origin, 2, 2, Color.green);
        DrawBox(box7Origin, 2, 2, Color.green);
        DrawBox(box8Origin, 2, 3, Color.cyan);
        DrawBox(box9Origin, 2, 2, Color.green);
    }

    void DrawBox(Vector2 origin, int cols, int rows, Color color)
    {
        Gizmos.color = color;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                float x = origin.x + col * cellWidth;
                float y = origin.y - row * cellHeight;

                // Draw cell outline
                Vector3 tl = new Vector3(x, y, 0f);
                Vector3 tr = new Vector3(x + cellWidth, y, 0f);
                Vector3 bl = new Vector3(x, y - cellHeight, 0f);
                Vector3 br = new Vector3(x + cellWidth, y - cellHeight, 0f);

                Gizmos.DrawLine(tl, tr);
                Gizmos.DrawLine(tr, br);
                Gizmos.DrawLine(br, bl);
                Gizmos.DrawLine(bl, tl);

                // Draw centre dot
                Vector3 centre = new Vector3(
                    x + cellWidth * 0.5f,
                    y - cellHeight * 0.5f,
                    0f);
                Gizmos.DrawWireSphere(centre, 0.05f);
            }
        }
    }
#endif
}