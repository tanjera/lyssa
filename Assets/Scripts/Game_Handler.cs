using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class Game_Handler : MonoBehaviour {

    public Board Board;
    public Player Player;

    public Text Text_Level, Text_HP, Text_Mana;

    public GameObject proto_Stone_9;
    public GameObject[] proto_Particles = new GameObject[Definitions.Mana_Amount];


    class Transformation {
        public enum Operations {
            Move,
            Scale
        }

        Operations Operation;
        float Time;
        Transform Transform;
        Vector3 Target,
            Velocity = Vector3.zero;

        public Transformation(Operations incOperation, Transform incTransform, Vector3 incTarget, float incTime) {
            Operation = incOperation;
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

        public bool Process() {
            if (Transform == null)
                return true; // If the Transform has been destroyed, remove it from the stack...

            switch (Operation) {
                default:
                case Operations.Move: return Move();
                case Operations.Scale: return Scale();
            }
        }

        bool Move() {
            Transform.position = Vector3.SmoothDamp(Transform.position, Target, ref Velocity, Time);
            return Transform.position == Target;
        }

        bool Scale() {
            Transform.localScale = Vector3.SmoothDamp(Transform.localScale, Target, ref Velocity, Time);
            return Transform.localScale == Target;
        }

        public Transform _Transform { get { return Transform; } }
        public Operations _Operation { get { return Operation; } }
    }

    List<Transformation> Transformation_Buffer = new List<Transformation>();

    void Start() {
        Player = new Player();
    }

    void FixedUpdate() {
        Text_Level.text = String.Format("Lvl {0}", Player.Character.Level);
        Text_HP.text = String.Format("HP {0} / {1}",
            Player.Character.Health_Current,
            Player.Character.Health_Max);
        Text_Mana.text = String.Format("Bk {0} \nBl {1} \nGr {2} \nPu {3} \nRe {4} \nWh {5} \nYe {6}",
            Player.Mana(Definitions.Mana_Colors.Black),
            Player.Mana(Definitions.Mana_Colors.Blue),
            Player.Mana(Definitions.Mana_Colors.Green),
            Player.Mana(Definitions.Mana_Colors.Purple),
            Player.Mana(Definitions.Mana_Colors.Red),
            Player.Mana(Definitions.Mana_Colors.White),
            Player.Mana(Definitions.Mana_Colors.Yellow));

        Transformation_Buffer.ForEach(obj => { if (obj.Process()) Transformation_Buffer.Remove(obj); });
    }

    public void Move(Transform incTransform, Vector3 incTarget, float incTime) 
        { Operate(Transformation.Operations.Move, incTransform, incTarget, incTime); }

    public void Scale (Transform incTransform, Vector3 incTarget, float incTime) 
        { Operate(Transformation.Operations.Scale, incTransform, incTarget, incTime); }

    void Operate(Transformation.Operations incOperation, Transform incTransform, Vector3 incTarget, float incTime) {
        if (Transformation_Buffer.Find(obj => obj._Operation == incOperation && obj._Transform == incTransform) == null)
            Transformation_Buffer.Add(new Transformation(incOperation, incTransform, incTarget, incTime));
        else
            Transformation_Buffer.Find(obj => obj._Operation == incOperation && obj._Transform == incTransform).Modify(incTarget, incTime);
    }
}
