using UnityEngine;
using System;
using System.Collections;

public class Board_Item : MonoBehaviour {

	[HideInInspector]
	public Board Board;
	public Items Item;

	public class Items {
		public enum Types {
			Stone,
			Consumable,
			Equipment
		}

        public enum States {
            Resting,
            Dragging
        }

		public Types Type;
        public States State;
        public Definitions.Mana_Colors Color;
		public GameObject Object;
		
		public int Column;
		public int Row;
	}

    public void OnCollisionEnter(Collision infoCollision) {
        if (infoCollision.gameObject.GetComponent<Board_Item>() != null)
            if (infoCollision.gameObject.GetComponent<Drag_Item>() != null)
                if (infoCollision.gameObject.GetComponent<Drag_Item>().Is_Dragging == false)
                    Board.Destroy_Item(infoCollision.gameObject.GetComponent<Board_Item>().Item);
    }
}
