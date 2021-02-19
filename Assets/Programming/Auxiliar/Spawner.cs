using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    #region Variables
    public GameObject prefab;
    public float interval = 1f;
    int n;
    #endregion

    private void Start() {
        n = 0;
        InvokeRepeating("Spawn", interval, interval);
    }

    private void Spawn() {
        var instance = Instantiate(prefab, transform.position, Quaternion.identity);
        instance.GetComponent<Matching>().SetRandValues();
        n++;
        if (n >= 4)
            CancelInvoke("Spawn");
    }

}
