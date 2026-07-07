using UnityEngine;
using UnityEditor;
using TMPro;

public class GridSquareSetupEditor : Editor
{
    [MenuItem("TicTacToe/Setup Grid Squares")]
    public static void SetupGridSquares()
    {
        GridSquare[] gridSquares = FindObjectsOfType<GridSquare>();

        foreach (GridSquare square in gridSquares)
        {
            string name = square.gameObject.name.ToLower();
            
            // Determine Y based on name
            int y = -1;
            if (name.Contains("top")) y = 0;
            else if (name.Contains("middle")) y = 1;
            else if (name.Contains("bottom")) y = 2;

            // Determine X based on name
            int x = -1;
            if (name.Contains("left")) x = 0;
            else if (name.Contains("mid")) x = 1;
            else if (name.Contains("right")) x = 2;

            // Set the X and Y coordinates
            SerializedObject serializedObject = new SerializedObject(square);
            
            if (x != -1) serializedObject.FindProperty("x").intValue = x;
            if (y != -1) serializedObject.FindProperty("y").intValue = y;

            // Find TextMeshProUGUI in children
            TextMeshProUGUI tmpText = square.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null)
            {
                serializedObject.FindProperty("markText").objectReferenceValue = tmpText;
            }

            // Apply changes
            serializedObject.ApplyModifiedProperties();
            
            Debug.Log($"Setup GridSquare {square.gameObject.name}: x={x}, y={y}, tmpro={(tmpText != null)}");
            EditorUtility.SetDirty(square);
        }

        Debug.Log("Grid Squares setup complete.");
    }
}
