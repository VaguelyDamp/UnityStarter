using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class UI_Meters : MonoBehaviour
{
    [MenuItem("GameObject/UI/Meters/Donut Meter")]
    public static void CreateDonutMeter()
    {
        GameObject donut = new GameObject("Donut Meter");
        donut.transform.parent = Selection.activeTransform;

        Image image = donut.AddComponent<Image>();
        image.sprite = (Sprite)AssetDatabase.LoadAssetAtPath("Assets/Sprites/UI/UI_DonutBar.png", typeof(Sprite));
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Radial360;
        image.fillOrigin = 2;

        FillMeter fill = donut.AddComponent<FillMeter>();
        fill.bar = image;
        fill.minVal = 0;
        fill.maxVal = 1;

        Undo.RegisterCreatedObjectUndo(donut, "Create " + donut.name);
        Selection.activeObject = donut;
    }

    [MenuItem("GameObject/UI/Meters/Bar Meter")]
    public static void CreateBarMeter()
    {
        GameObject bar = new GameObject("Bar Meter");
        bar.transform.parent = Selection.activeTransform;

        Image image = bar.AddComponent<Image>();
        image.sprite = (Sprite)AssetDatabase.LoadAssetAtPath("Assets/Sprites/UI/UI_BasicBox.png", typeof(Sprite));
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Horizontal;
        image.fillOrigin = 0;

        FillMeter fill = bar.AddComponent<FillMeter>();
        fill.bar = image;
        fill.minVal = 0;
        fill.maxVal = 1;

        Undo.RegisterCreatedObjectUndo(bar, "Create " + bar.name);
        Selection.activeObject = bar;
    }

    [MenuItem("GameObject/UI/Meters/Chunk Meter")]
    public static void CreateChunkMeter()
    {
        GameObject bar = new GameObject("Chunk Meter");
        bar.transform.parent = Selection.activeTransform;

        ChunkMeter meter = bar.AddComponent<ChunkMeter>();
        meter.chunkSprite = (Sprite)AssetDatabase.LoadAssetAtPath("Assets/Sprites/Branding/dampboi.png", typeof(Sprite));
        meter.chunkSize = new Vector3(1, 1, 1);
        meter.chunkSpacing = new Vector3(80, 0, 1);

        Undo.RegisterCreatedObjectUndo(bar, "Create " + bar.name);
        Selection.activeObject = bar;
    }
}
