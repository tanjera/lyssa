using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class _Tile : MonoBehaviour {

    public bool Spin = true;

    public ParticleSystem Particle_onDrag,
                            Particle_onDestroy;

    public delegate void Tile_Collision_Handler(GameObject sender, GameObject collided);
    public event Tile_Collision_Handler Tile_Collision;

    void Start() {
        if(Spin)
            rigidbody.AddTorque(new Vector3(UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)));
    }

    public void OnCollisionEnter(Collision objStruck) {
        if (Tile_Collision != null && objStruck.gameObject.layer == LayerMask.NameToLayer(__Definitions.layerInteractive))
            Tile_Collision(this.gameObject, objStruck.gameObject);
    }
}
