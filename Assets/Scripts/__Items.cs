using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class __Items : MonoBehaviour {

    public delegate void Item_Collision_Handler(GameObject sender, GameObject collided);
    public event Item_Collision_Handler Item_Collision;

    public void OnTriggerEnter(Collider objStruck) {
        if (Item_Collision != null && objStruck.gameObject.layer == LayerMask.NameToLayer(Game.Layer__Board_Item))
            Item_Collision(this.gameObject, objStruck.gameObject);
    }
}
