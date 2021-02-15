using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour {

    #region Variables
    [SerializeField]
    private GameObject projectile;
    [SerializeField]
    private float shootingSpeed;
    private Camera cam;
    [SerializeField]
    private AI personBuffer;
    private Matching[] matches;
    #endregion

    private void Start() {
        cam = Camera.main;
        personBuffer = null;
        matches = FindObjectsOfType<Matching>();
        Debug.Log(matches[0].Match(matches[1]));
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0))
            Shoot();
    }

    private void Shoot() {
        //Instantiate the prefab to the mouse's position
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = mousePos - (Vector2)transform.position;
        var instance = Instantiate(projectile, transform.position, Quaternion.identity);
        Projectile projInstance = instance.GetComponent<Projectile>();
        projInstance.Initiate(dir.normalized, shootingSpeed);
    }

}
