using UnityEngine;
using System;
using System.Collections;

public class __Ship : __Destructable {

    /* To add:
     * - Energy system
     * - Module loadout (weapons, shields, etc)
     * - Cargo space
     * 
     * - Flight plans (lerping to coordinates, etc- for kinema and NPC controls)
     * 
     */


    __Game _Game;

    __Destructable _Target;
    Rigidbody _Ship;


    /* Motion variables */
    float Forward__Terminal = 1000,
            Forward__Thrust = 500,
            Forward__Drag = 5f,
            
            Lateral__Terminal = 100,
            Lateral__Thrust = 50,
            Lateral__Drag = 10f,

            // Rigidbody variables
            Drag_Velocity = 0,
            Drag_Angular = 10;

    bool Reverse = false,
            Dragging = false;

    
    void Awake() {
        
        _Ship = this.rigidbody;
        _Game = GameObject.Find("__Game Controller").GetComponent<__Game>();
        
        _Ship.drag = Drag_Velocity;
        _Ship.angularDrag = Drag_Angular;
    }

    public void Move(float Thrust, float Pitch, float Yaw, float Roll, float Lateral, float Vertical) {
        
        Vector3 Inverse = transform.InverseTransformDirection(_Ship.velocity);

        // Thrust the ship forward (and backwards if reversable)
        if ((Thrust >= 0 && Inverse.z < Forward__Terminal * Thrust)
            || (Thrust <= 0 && Inverse.z > Forward__Terminal * Thrust))
            _Ship.AddRelativeForce(0, 0,
                    Mathf.Clamp(Thrust * Forward__Thrust,
                        Reverse
                            ? -Forward__Thrust
                            : (Inverse.z > 0 ? -Forward__Thrust : 0),
                        Forward__Thrust)
                    * _Game._Delta_Time,
                ForceMode.Impulse);

            // Thrust the ship laterally and vertically
            if (Mathf.Abs(Inverse.x) < Mathf.Abs(Lateral__Terminal * Lateral))
                _Ship.AddRelativeForce(
                        Mathf.Clamp(Lateral * Lateral__Thrust, -Lateral__Thrust, Lateral__Thrust)
                        * _Game._Delta_Time,
                        0, 0,
                    ForceMode.Impulse);
        
        if (Mathf.Abs(Inverse.y) < Mathf.Abs(Lateral__Terminal * Vertical))
            _Ship.AddRelativeForce(0,
                    Mathf.Clamp(Vertical * Lateral__Thrust, -Lateral__Thrust, Lateral__Thrust)
                    * _Game._Delta_Time,
                    0,
                ForceMode.Impulse);

        // Rotate the ship
        _Ship.AddRelativeTorque(Pitch, Yaw, Roll, ForceMode.Impulse);

        // If dragging is toggled on (then no sliding)
        if (Dragging)
            _Ship.AddRelativeForce(
                -Inverse.x * Lateral__Drag * _Game._Delta_Time,
                -Inverse.y * Lateral__Drag * _Game._Delta_Time, 
                0, ForceMode.Impulse);
    }

    public void Drag_All() {
        // Negate all directional forces
        Vector3 Inverse = transform.InverseTransformDirection(_Ship.velocity);
        _Ship.AddRelativeForce(
                -Inverse.x * Lateral__Drag * _Game._Delta_Time,
                -Inverse.y * Lateral__Drag * _Game._Delta_Time,
                -Inverse.z * Forward__Drag * _Game._Delta_Time, 
                ForceMode.Impulse);
    }
    public void Drag_Toggle() {
        Dragging = !Dragging;
    }



    /* Weapons stuff... will need to move it to each weapons module
     */

    float Firing_Rate = 0.5f,
            Firing_Timer = 0f;


    public void Fire_Main() {

        if (Firing_Timer > _Game._Time - Firing_Rate)
            return;
        Firing_Timer = _Game._Time;


        /* Hardcode an instant-fire ray... laser-ish? */

        HP__Energy -= 5;
        RaycastHit Shot;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out Shot)) {

            __Destructable.Damages Damage = new Damages(25, 2.5f, 1f, .75f);
            Shot.collider.SendMessageUpwards("Damage", Damage);
        }

    }
    public void Fire_Secondary() {
    }

}
