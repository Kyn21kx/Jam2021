using UnityEngine;

public abstract class Utilities {

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


}
