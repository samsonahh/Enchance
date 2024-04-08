using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGizmos
{
    public static float GIZMO_DISK_THICKNESS = 0.01f;
    public static float GIZMO_COLOR_ALPHA = 0.25f;

    public static void DrawWireDisk(Vector3 position, float radius, Color color)
    {
        Color oldColor = Gizmos.color;
        Gizmos.color = color;
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(position, Quaternion.identity, new Vector3(1, GIZMO_DISK_THICKNESS, 1));
        Gizmos.DrawWireSphere(Vector3.zero, radius);
        Gizmos.matrix = oldMatrix;
        Gizmos.color = oldColor;
    }

    public static void DrawDisk(Vector3 position, float radius, Color color)
    {
        color = new Color(color.r, color.g, color.b, GIZMO_COLOR_ALPHA);

        Color oldColor = Gizmos.color;
        Gizmos.color = color;
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(position, Quaternion.identity, new Vector3(1, GIZMO_DISK_THICKNESS, 1));
        Gizmos.DrawSphere(Vector3.zero, radius);
        Gizmos.matrix = oldMatrix;
        Gizmos.color = oldColor;
    }
}
