using UnityEngine;

public static class Vector3Extend
{

    public static Vector2Int RoundToVector2Int(this Vector3 vector)
    {
        return new Vector2Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y));
    }
}
