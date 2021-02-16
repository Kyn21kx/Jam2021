using UnityEngine;

public class HealthManager : MonoBehaviour {

    #region Variables
    private byte health;
    public byte Health { get { return health; } }
    public bool Invincible { get; private set; }
    [SerializeField]
    private float invTime;
    private float auxInvTime;
    #endregion

    private void Start() {
        //Save the value of the invTime
        auxInvTime = invTime;
    }

    private void Update() {
        if (Invincible) {
            //Timer down and set it to false
        }
    }

    public void Damage() {
        if (!Invincible) {
            health--;
            DmgIndicatorIn();
        }
    }

    private void DmgIndicatorIn () {
        GetComponent<SpriteRenderer>().color = Color.red;
        Invoke("DmgIndicatorOut", 0.1f);
    }

    private void DmgIndicatorOut() {
        GetComponent<SpriteRenderer>().color = Color.white;
    }

}
