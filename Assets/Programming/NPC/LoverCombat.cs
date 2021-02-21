using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoverCombat : MonoBehaviour {

    //TODO: REFACTOR THIS PLEASEEEEEEEEEEEEEEEEEEEEEE
    #region Variables
    [SerializeField]
    AI personBuffer;

    AI selfAI;
    public float escapedDistance;
    public float fleeDistance;
    public float loverTime;
    private float auxLoverTime;
    [SerializeField]
    private GameObject healthBars;
    #endregion

    private void Start() {
        selfAI = GetComponent<AI>();
        auxLoverTime = loverTime;
    }

    private void Update() {
        if (selfAI.lover) {
            //Set active the bars
            loverTime -= Time.deltaTime;

            healthBars.SetActive(true);
            float ratio = loverTime / auxLoverTime;
            Transform barToScale = healthBars.transform.GetChild(0);
            barToScale.localScale = new Vector3(ratio, 1f, 1f);

            if (loverTime <= 0f) {
                Utilities.gameManager.lovers.Remove(selfAI);
                Destroy(gameObject);
            }
        }
        else {
            loverTime = auxLoverTime;
            healthBars.SetActive(false);
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, fleeDistance);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, escapedDistance);
    }

}
