using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    [SerializeField] private Collider2D col;

    public void DisableCollider()
    {
        col.enabled = false;
    }
    
    public void EnableCollider()
    {
        col.enabled = true;
    }
}
