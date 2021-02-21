using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    #region Variables
    public GameObject prefab;
    public float interval = 1f;
    int n;
    public List<Matching> previous;
    #endregion

    private void Start() {
        n = 0;
        previous = new List<Matching>();
        previous.AddRange(FindObjectsOfType<Matching>());
        InvokeRepeating("Spawn", interval, interval);
    }

    private void Spawn() {
        var instance = Instantiate(prefab, transform.position, Quaternion.identity);
        instance.SetActive(false);
        var matchRef = instance.GetComponent<Matching>();
        matchRef.SetRandValues(previous);
        instance.GetComponent<AI>().ranged = Random.Range(0, 2) == 1;
        previous.Add(matchRef);
        instance.SetActive(true);
        n++;
        if (n >= 5)
            CancelInvoke("Spawn");
    }

}
