/* Menu_Handler.cs
 * by Ibi Keller
 * 08 Apr 2013
 * 
 * Attach to any GameObject with menu items (GUIElements) in its heirarchy,
 * the script gives increased functionality for the menu as a whole.
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class __Menu : MonoBehaviour {
	
	public enum Anchors {
		NONE,
		TOP_LEFT,
		TOP_CENTER,
		TOP_RIGHT,
		MIDDLE_LEFT,
		MIDDLE_CENTER,
		MIDDLE_RIGHT,
		BOTTOM_LEFT,
		BOTTOM_CENTER,
		BOTTOM_RIGHT
	}
	
	__Actions _Action;
	
	public bool _Enabled = false;
	public Anchors _Anchor = Anchors.NONE;
	public Vector2 _Offset = new Vector2(0, 0);
	Vector2 _Offset__Last = new Vector2(0, 0);
	
	public Texture _Box__Checked,
					_Box__Unchecked;
	
	bool _Reach__Enabled = false;
	Transform _Reach__Position;
	
	GameObject _Parent;
	List<GUIElement> _Items = new List<GUIElement>();
	
	
	public __Menu Instantiate(GameObject Parent) {
		_Parent = Parent;
		Shift(-Get_Anchor());
		
		return this;
	}
	void Awake() {
        _Action = GameObject.Find("__Game Controller").GetComponent<__Actions>();
		_Action._GUI__Rescan();
		
		_Items.AddRange(gameObject.GetComponentsInChildren<GUIElement>() as GUIElement[]);
		_Items.ForEach((obj) => obj.enabled = _Enabled);
		
		// Uncomment to use _Offset live while creating/editing menus
		// Instantiate(new GameObject(""));	
	}
	void Update() {
		if (_Offset != _Offset__Last) {
			Shift(_Offset__Last - _Offset);
			_Offset__Last = _Offset;
		}
	}
	
	void __Input (__Actions.Input_Collision _Input) {
		/* Pass all input to the script/gameObject that owns this menu, even though this menu is not
		 * parented to it in the project menu (to prevent transform distortions)
		 */
		
		_Parent.SendMessageUpwards("__Input", _Input, SendMessageOptions.DontRequireReceiver);

		if (_Input.Name == "_Window__Close" && _Input.Key__Pressed)
			Hide();
	}
	
	public void Show() {
		_Enabled = true;
		_Items.ForEach((obj) => obj.enabled = _Enabled);	
	}
	public void Show(bool Reach, Transform Position) {
		_Reach__Enabled = Reach;
		_Reach__Position = Position;
		Show();
	}
	public void Hide() {
		_Enabled = false;
		_Items.ForEach((obj) => obj.enabled = _Enabled);
	}
	public void Toggle() {
		if (_Enabled)
			Hide();
		else if (!_Enabled)
			Show();
	}
	public void Visibility(bool State) {
		if (State)
			Show();
		else if (!State)
			Hide();
	}
	public bool Visible 
		{ get { return _Enabled; } }
	
	Vector2 Get_Anchor() {
		switch (_Anchor) {
		default:
		case Anchors.NONE:
			return new Vector2(0, 0);
			break;
		case Anchors.TOP_LEFT:
			return new Vector2(0, Screen.height);
			break;
		case Anchors.TOP_CENTER:
			return new Vector2(Screen.width / 2, Screen.height);
			break;
		case Anchors.TOP_RIGHT:
			return new Vector2(Screen.width, Screen.height);
			break;
		case Anchors.MIDDLE_LEFT:
			return new Vector2(0, Screen.height / 2);
			break;
		case Anchors.MIDDLE_CENTER:
			return new Vector2(Screen.width / 2, Screen.height / 2);
			break;
		case Anchors.MIDDLE_RIGHT:
			return new Vector2(Screen.width, Screen.height / 2);
			break;
		case Anchors.BOTTOM_LEFT:
			return new Vector2(0, 0);
			break;
		case Anchors.BOTTOM_CENTER:
			return new Vector2(Screen.width / 2, 0);
			break;
		case Anchors.BOTTOM_RIGHT:
			return new Vector2(Screen.width, 0);
			break;
			
		}
	}
	void Shift(Vector2 _Shift) {
		foreach (GUIElement eachElement in _Items) {
			if (eachElement.guiText != null)
				eachElement.guiText.pixelOffset = new Vector2(
					eachElement.guiText.pixelOffset.x - _Shift.x,
					eachElement.guiText.pixelOffset.y - _Shift.y);
			if (eachElement.guiTexture != null)
				eachElement.guiTexture.pixelInset = new Rect(
					eachElement.guiTexture.pixelInset.x - _Shift.x,
					eachElement.guiTexture.pixelInset.y - _Shift.y,
					eachElement.guiTexture.pixelInset.width,
					eachElement.guiTexture.pixelInset.height);
		}
	}
	
	public string Text__Get(string Name) { 
		return _Items.Find((obj) => obj.guiText != null && obj.name == Name).guiText.text;
	}
	public void Text__Append(string Name, string Append, string _Color, bool NewLine = true) {
		Text__Set(Name, String.Format("{0}{1}{2}", Text__Get(Name), NewLine ? Environment.NewLine : "", Append), _Color);
	}
	public void Text__Remove(string Name, string Remove, string _Color, bool Trim = true) {
		string newText = Text__Get(Name);
		if (!newText.Contains(Remove))
			return;
		
		newText = newText.Remove(newText.IndexOf(Remove), Remove.Length);
		if (Trim)
			newText = newText.Trim(Environment.NewLine.ToCharArray());
		
		Text__Set(Name, newText, _Color);
	}
	public void Text__Clear(string Name) {
		_Items.FindAll((obj) => obj.guiText != null && obj.name == Name)
			.ForEach((obj) => obj.guiText.text = String.Empty);
	}
	public void Text__Set(string Name, string Text, string _Color) {
		_Items.FindAll((obj) => obj.guiText != null && obj.name == Name)
			.ForEach((obj) => {
				obj.guiText.text = String.Format("<color={0}>{1}</color>", _Color, Text);
				obj.guiText.richText = true;
		});
	}
	
	public void Texture_Set(string Name, Texture _Texture) {
		_Items.FindAll((obj) => obj.guiTexture != null && obj.name == Name)
				.ForEach((obj) => obj.guiTexture.texture = _Texture);
	}
}
