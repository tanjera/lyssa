using UnityEngine;
using System;
using System.Collections;

public class __Board_Item : MonoBehaviour {

	[HideInInspector]
	public __Board _Board;
	public __Item _This;

	public class __Item {
		public enum __Type {
			Stone,
			Consumable,
			Equipment
		}
		
		public enum __Color {
			Black,
			Blue,
			Green,
			Purple,
			Red,
			White,
			Yellow
		}
		
		public __Type _Type;
		public __Color _Color;
		public GameObject _Object;
		
		public int _Column;
		public int _Row;
	}


	void OnMouseDown() {
		_Board.Destroy_Item (_This);
	}
}
