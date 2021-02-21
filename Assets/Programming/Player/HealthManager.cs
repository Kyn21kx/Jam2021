using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class HealthManager : MonoBehaviour {

    #region Variables
    [SerializeField]
    private byte health;
    public byte Health { get { return health; } }
    public bool Invincible { get; private set; }
    [SerializeField]
    private float invTime;
    private float auxInvTime;
    [SerializeField]
    private TextMeshPro txt;
    #endregion

    private void Start() {
        //Save the value of the invTime
        auxInvTime = invTime;
    }

    private void Update() {
        txt.SetText("x" + health);
        if (Invincible) {
            //Timer down and set it to false
            invTime -= Time.deltaTime;
            if (invTime <= 0f) {
                Invincible = false;
                invTime = auxInvTime;
            }
        }
        if (health <= 0)
            Die();
    }

    private void Die() {
        Time.timeScale = 0.5f;
        ScoreManager.highScore = Mathf.Max(ScoreManager.highScore, Utilities.scoreManager.score);
        SceneManager.LoadScene(1);
    }

    public void Damage() {
        if (!Invincible) {
            health--;
            DmgIndicatorIn();
            Invincible = true;
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
