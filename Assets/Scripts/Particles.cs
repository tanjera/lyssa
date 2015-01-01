using UnityEngine;
using System.Collections;

public class Particles : MonoBehaviour {

    ParticleSystem thisParticle;

	void Start () {
        thisParticle = GetComponent<ParticleSystem>();
	}
	
	void Update () {
        if (thisParticle && !thisParticle.IsAlive())
            Destroy(thisParticle.gameObject);
	}
}
