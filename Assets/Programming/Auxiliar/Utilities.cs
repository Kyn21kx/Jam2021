using UnityEngine;

public abstract class Utilities {

    public static Transform playerRef;
    public static ScoreManager scoreManager;
    public static Spawner spawner;

    public static Vector2 GetRandomVector(float xMin, float xMax, float yMin, float yMax) {
        float x = Random.Range(xMin, xMax);
        float y = Random.Range(yMin, yMax);
        return new Vector2(x, y);
    }

    public static Vector2 GetRandomVector(float min, float max) {
        float x = Random.Range(min, max);
        float y = Random.Range(min, max);
        return new Vector2(x, y);
    }

    public static void OffsetVectorByAngle(ref Vector2 v, float a) {
        //Convert to polar
        float magnitude = v.magnitude;
        float opToRad = (v.y / magnitude) * Mathf.Deg2Rad;
        float angle = Mathf.Asin(opToRad);
        angle = angle + (a * Mathf.Deg2Rad);
        v.x = magnitude * Mathf.Cos(angle);
        v.y = magnitude * Mathf.Sin(angle);
    }

}
