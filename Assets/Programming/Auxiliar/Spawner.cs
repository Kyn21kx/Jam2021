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
    #endregion

    private void Start() {
        n = 0;
        previous = new List<Matching>();
        previous.AddRange(FindObjectsOfType<Matching>());
        Invoke("Spawn", interval);
    }

    private void Spawn() {
        var instance = Instantiate(prefab, transform.position, Quaternion.identity);
        instance.SetActive(false);
        var matchRef = instance.GetComponent<Matching>();
        bool forceMatch = n % forceMatchEveryN == 0;
        matchRef.SetRandValues(previous, forceMatch);
        instance.GetComponent<AI>().ranged = Random.Range(0, 2) == 1;
        previous.Add(matchRef);
        instance.SetActive(true);
        n++;
        interval *= 1.7f;
        Invoke("Spawn", interval);
    }

}
