using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    #region Variables
    public GameObject prefab;
    public float interval = 1f;
    public int forceMatchEveryN;
    int n;
    public List<Matching> previous;
    public Transform borderX;
    public Transform borderY;
    #endregion

    private void Start() {
        n = 0;
        previous = new List<Matching>();
        previous.AddRange(FindObjectsOfType<Matching>());
        Invoke("Spawn", interval);
    }

    private void Spawn() {
        float minX = -borderX.position.x;
        float maxX = borderX.position.x;
        float maxY = borderY.position.y;
        float minY = -borderY.position.y;
        Vector2 nPos = Utilities.GetRandomVector(minX, maxX, minY, maxY);
        var instance = Instantiate(prefab, nPos, Quaternion.identity);
        instance.SetActive(false);
        var matchRef = instance.GetComponent<Matching>();
        bool forceMatch = n % forceMatchEveryN == 0;
        matchRef.SetRandValues(previous, forceMatch);
        instance.GetComponent<AI>().ranged = Random.Range(0, 2) == 1;
        previous.Add(matchRef);
        instance.SetActive(true);
        n++;
        if ((int)interval % 3 == 0) {
            forceMatchEveryN++;
        }
        interval *= 1.05f;
        Invoke("Spawn", interval);
    }

}
