using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class _Tile : MonoBehaviour {

    public delegate void Tile_Collision_Handler(GameObject sender, GameObject collided);
    public event Tile_Collision_Handler Tile_Collision;

    public void OnCollisionEnter(Collision objStruck) {
        if (Tile_Collision != null && objStruck.gameObject.layer == LayerMask.NameToLayer(__Definitions.Layer__Interactive))
            Tile_Collision(this.gameObject, objStruck.gameObject);
    }
}
