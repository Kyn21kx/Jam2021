using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(AstarPath))]
public class GameManager : MonoBehaviour {

    #region Variables
    private AstarPath pathSettings;
    private GridGraph grid;
    #endregion

    private void Start() {
        pathSettings = GetComponent<AstarPath>();
        //Make sure this always works
        grid = (GridGraph)pathSettings.graphs[0];
        InvokeRepeating("UpdateGraph", 0.5f, 0.5f);
    }

    private void UpdateGraph() {
        pathSettings.Scan(grid);
    }

}
