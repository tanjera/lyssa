using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Game_Handler : MonoBehaviour {

	public Board _Board;
	public Player _Player;


    class Movement {
        float Time;
        Transform Transform;
        Vector3 Target,
            Velocity = Vector3.zero;

        public Movement(Transform incTransform, Vector3 incTarget, float incTime) {
            Transform = incTransform;
            Target = incTarget;
            Time = incTime;
        }

        public void Modify(Vector3 incTarget, float incTime) {
            if (Target == incTarget && Time == incTime)
                return;

            Target = incTarget;
            Time = incTime;
        }

        public bool Move_Smooth() {
            if (Transform == null) 
                return true; // If the Transform has been destroyed, remove it from the stack...
            
            Transform.position = Vector3.SmoothDamp(Transform.position, Target, ref Velocity, Time);
            return Transform.position == Target;
        }

        public Transform _Transform 
        { get { return Transform; } }
    }


    List<Movement> Movement_Buffer = new List<Movement>();

    void Update() {
        //Process the movement buffer for items needing translation over time, remove if complete
        Movement_Buffer.ForEach(obj => { if (obj.Move_Smooth()) Movement_Buffer.Remove(obj); });
    }

    public void Move_Smooth(Transform incTransform, Vector3 incTarget, float incTime) {
        // If this object is *not* already being moved...
        if (Movement_Buffer.Find(obj => obj._Transform == incTransform) == null)
            Movement_Buffer.Add(new Movement(incTransform, incTarget, incTime)); // Then move it!
        else
            Movement_Buffer.Find(obj => obj._Transform == incTransform).Modify(incTarget, incTime);

    }
}
