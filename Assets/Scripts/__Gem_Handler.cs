using UnityEngine;
using System;
using System.Collections;

public class __Gem_Handler : MonoBehaviour {

	public GameObject __Gem_Prototype__Black, __Gem_Prototype__Blue, __Gem_Prototype__Green,
	__Gem_Prototype__Red, __Gem_Prototype__White;

	public class __Gem {
		public enum __Color {
			Black,
			Blue,
			Green,
			Red,
			White
		}
		
		public __Color _Color;
		public GameObject _Object;
	}


	public __Gem Create_Gem__Random() {
		__Gem thisGem = new __Gem ();

		thisGem._Color = (__Gem.__Color)UnityEngine.Random.Range (0, Enum.GetNames (typeof(__Gem.__Color)).Length);

		switch (thisGem._Color) {
			default:
			case __Gem.__Color.Black: thisGem._Object = (GameObject)GameObject.Instantiate(__Gem_Prototype__Black); break;
			case __Gem.__Color.Blue: thisGem._Object = (GameObject)GameObject.Instantiate(__Gem_Prototype__Blue); break;
			case __Gem.__Color.Green: thisGem._Object = (GameObject)GameObject.Instantiate(__Gem_Prototype__Green); break;
			case __Gem.__Color.Red: thisGem._Object = (GameObject)GameObject.Instantiate(__Gem_Prototype__Red); break;
			case __Gem.__Color.White: thisGem._Object = (GameObject)GameObject.Instantiate(__Gem_Prototype__White); break;
		}

		thisGem._Object.transform.parent = this.transform;
		return thisGem;
	}
}
