using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pendulo : MonoBehaviour
{
    [SerializeField] private Transform holder = null;
    [SerializeField] private Transform weight = null;

    private float angle;

    private void LateUpdate() 
    {
        Vector2 direction = new Vector2(
            holder.position.x - weight.position.x, 
            holder.position.y - weight.position.y 
        );

        holder.up = direction; 
    }
}
