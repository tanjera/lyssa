using UnityEngine;
using System.Collections;

public class _Particle : MonoBehaviour {

    ParticleSystem thisParticle;

	void Start () {
        thisParticle = GetComponent<ParticleSystem>();
	}
	
	void Update () {
        if (thisParticle && !thisParticle.IsAlive() && transform.parent == null)
            Destroy(thisParticle.gameObject);
	}
}
