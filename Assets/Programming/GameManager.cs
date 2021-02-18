using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(AstarPath))]
public class GameManager : MonoBehaviour {

    #region Variables
    private AstarPath pathSettings;
    public GridGraph PathGrid { get; private set; }
    #endregion

    private void Start() {
        Utilities.playerRef = GameObject.FindGameObjectWithTag("Player").transform;
        pathSettings = GetComponent<AstarPath>();
        //Make sure this always works
        PathGrid = (GridGraph)pathSettings.graphs[0];
        InvokeRepeating("UpdateGraph", 0.5f, 0.5f);
    }

    private void UpdateGraph() {
        pathSettings.Scan(PathGrid);
    }

}
