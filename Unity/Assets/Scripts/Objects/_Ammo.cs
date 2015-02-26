using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class _Ammo : MonoBehaviour {
    
    __Game_Handler _Game;

    [HideInInspector]
    public __Ship targetShip;
    
    [HideInInspector]
    public Collider targetShield;

    public __Game_Handler.kinemaOp kinemaOp;
    public Light lightImpact;
    public ParticleSystem particleFlight,
                            particleExplosion;

    void Awake() {
        _Game = GameObject.Find(__Definitions.Object_gameController).GetComponent<__Game_Handler>();
    }

    public void Target(__Ship incTarget) {
        targetShip = incTarget;
        targetShield = targetShip.transform.FindChild(__Definitions.Object_shipShield).GetComponent<Collider>();
    }

    void OnCollisionEnter(Collision incCollision) {
        if (targetShip.shieldsActive() && incCollision.collider == targetShield) {
            lightImpact.enabled = true;
            _Game.kinemaAdd(new __Game_Handler.kinemaOp(lightImpact.transform, Color.red, 10, 5, 1));
        }
    }
}
