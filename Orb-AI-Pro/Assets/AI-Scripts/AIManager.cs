using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public GameObject player;



    #region Helpers

    private void IncreaseWon()
    {
        GameManager.Shared().IncreaseWon();
    }
    
    private void IncreaseLost()
    {
        GameManager.Shared().IncreaseLost();
    }

    #endregion
}
