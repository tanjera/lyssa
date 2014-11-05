using UnityEngine;
using System;
using System.Collections;

public class Board_Item : MonoBehaviour {

	[HideInInspector]
	public Board Board;
	public Item This;

	public class Item {
		public enum Types {
			Stone,
			Consumable,
			Equipment
		}
		
		public Types Type;
		public Definitions.Mana_Colors Color;
		public GameObject Object;
		
		public int Column;
		public int Row;
	}


	void OnMouseDown() {
		Board.Destroy_Item (This);
	}
}
