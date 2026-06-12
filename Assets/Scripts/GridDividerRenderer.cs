using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
//  GridDividerRenderer
//
//  Draws visible black lines for every 2x2 block:
//    - 4 outer border lines (the block outline)
//    - 1 vertical centre divider (between left and right cells)
//    - 1 horizontal centre divider (between top and bottom cells)
//
//  Attach to GameManager. Match all 4 layout values to your UnitSpawner.
// ─────────────────────────────────────────────────────────────────────────────
public class GridDividerRenderer : MonoBehaviour
{
    [Header("Must match your UnitSpawner values exactly")]
    public Vector2 boardTopLeft = new Vector2(-2.7f, 3f);
    public float cellSize = 0.55f;
    public float cellGap = 0.2f;
    public float laneWidth = 1.0f;

    [Header("Line Appearance")]
    public Color lineColor = Color.black;
    public float lineWidth = 0.04f;
    public int sortingOrder = 5;

    void Start()
    {
        DrawAllGridLines();
    }

    void DrawAllGridLines()
    {
        float blockStride = (cellSize * 2f) + cellGap + laneWidth;

        for (int blockRow = 0; blockRow < 3; blockRow++)
        {
            for (int blockCol = 0; blockCol < 3; blockCol++)
            {
                // Centre positions of the 4 cells in this block
                // These exactly match what UnitSpawner.BuildGrid() produces
                float originX = boardTopLeft.x + (blockCol * blockStride);
                float originY = boardTopLeft.y - (blockRow * blockStride);

                float cell1X = originX;                      // top-left  cell centre X
                float cell2X = originX + cellSize + cellGap; // top-right cell centre X
                float cell1Y = originY;                      // top cell centre Y
                float cell3Y = originY - cellSize - cellGap; // bottom cell centre Y

                // Block outer bounds (half a cell out from each edge centre)
                float left = cell1X - cellSize * 0.5f;
                float right = cell2X + cellSize * 0.5f;
                float top = cell1Y + cellSize * 0.5f;
                float bottom = cell3Y - cellSize * 0.5f;

                // Exact midpoint between the two cell columns
                float midX = (cell1X + cellSize * 0.5f + cell2X - cellSize * 0.5f) * 0.5f;
                // Exact midpoint between the two cell rows
                float midY = (cell1Y - cellSize * 0.5f + cell3Y + cellSize * 0.5f) * 0.5f;

                string b = $"{blockRow}_{blockCol}";

                // Outer border
                CreateLine("Top_" + b, left, top, right, top);
                CreateLine("Bot_" + b, left, bottom, right, bottom);
                CreateLine("Lft_" + b, left, top, left, bottom);
                CreateLine("Rgt_" + b, right, top, right, bottom);

                // Inner cross — perfectly centred
                CreateLine("VDiv_" + b, midX, top, midX, bottom);
                CreateLine("HDiv_" + b, left, midY, right, midY);
            }
        }
    }

    void CreateLine(string lineName, float x1, float y1, float x2, float y2)
    {
        GameObject obj = new GameObject(lineName);
        obj.transform.SetParent(transform);

        LineRenderer lr = obj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, new Vector3(x1, y1, 0f));
        lr.SetPosition(1, new Vector3(x2, y2, 0f));

        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.startColor = lineColor;
        lr.endColor = lineColor;
        lr.useWorldSpace = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.material.color = lineColor;
        lr.sortingOrder = sortingOrder;
    }
}