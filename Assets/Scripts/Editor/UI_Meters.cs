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

        Undo.RegisterCreatedObjectUndo(donut, "Create " + donut.name);
        Selection.activeObject = donut;
    }
}
