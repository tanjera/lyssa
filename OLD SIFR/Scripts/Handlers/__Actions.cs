/* Action_Handler.cs
 * 01 Mar 2013
 * 
 * To process input and pass it to the target collider,
 * and to handle basic player/camera actions
 *
 */

using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class __Actions : MonoBehaviour {
	
	/* Game variables */
	__Game _Game;
	__Audio _Audio_Handler;

    __HUD _HUD;
    __Ship _Flight;
    GameObject _Player;

	/* Arrays for GUI processing */
	List<GUIElement> _GUI__Items = new List<GUIElement>();
	
	/* Actions that can be taken (inputted) by player */
	public enum Actions {
		NONE,
		
        THRUST_DRAG__ALL,
        THRUST_DRAG__TOGGLE,
		
        FIRE_MAIN,
        FIRE_SECONDARY,

        TARGET_ENEMY,
        TARGET_FRIENDLY,
        TARGET_NEAREST,
		
        CLICK,

		PAUSE,
		PRINTSCREEN,
		ESCAPE
	}
	
	/* Class for storing key assignments for game functions */
	public class Key_Setting {
		public KeyCode _Key;
		public Actions _Function;
		
		public Key_Setting(Actions Function, KeyCode Key) {
			_Key = Key;
			_Function = Function;
		}
	}
	
	/* List of key assignments for game functions */
	List<Key_Setting> _Keys = new List<Key_Setting>( new Key_Setting[] {
        new Key_Setting(Actions.THRUST_DRAG__ALL, KeyCode.JoystickButton5),
        new Key_Setting(Actions.THRUST_DRAG__TOGGLE, KeyCode.JoystickButton3),
        
        new Key_Setting(Actions.FIRE_MAIN, KeyCode.JoystickButton0),
        new Key_Setting(Actions.FIRE_SECONDARY, KeyCode.JoystickButton1),

        new Key_Setting(Actions.TARGET_ENEMY, KeyCode.JoystickButton2),
        new Key_Setting(Actions.TARGET_FRIENDLY, KeyCode.JoystickButton4),
        
        
        new Key_Setting(Actions.CLICK, KeyCode.Mouse0),
        
        new Key_Setting(Actions.PRINTSCREEN, KeyCode.F5),
		new Key_Setting(Actions.ESCAPE, KeyCode.X)
	});

	
	/* Mouse selection variables */
	public class Input_Collision {
		public enum Key_States {
			NONE,
			
			PRESSED,
			RELEASED,
			HELD,
			
			MOUSE_ON,
			MOUSE_HELD,
			MOUSE_OFF
		}
		public enum Item_Type {
			NONE,
			OBJ_RAYCAST,
			GUI_HITTEST
		}
		__Game _Game;
		public bool _Is_Hit = false;
		public Item_Type _Type;
		public RaycastHit _Hit;
		public GUIElement _GUI;
		public float _Time;
		public Actions _Action = __Actions.Actions.NONE;
		public Key_States _State__Key = Key_States.NONE;
		public Key_States _State__Mouse = Key_States.NONE;
		
		public Input_Collision() { }
		public Input_Collision(List<GUIElement> _Elements, Vector3 _Mouse, float __Time) {
			// Overload to detect GUI collisions
			foreach (GUIElement eachElement in _Elements) {
				if (eachElement.enabled 
						&& eachElement.gameObject.layer == LayerMask.NameToLayer("__Mouse Collision")
						&& eachElement.HitTest(_Mouse)) {
					_Is_Hit = true;
					_Time = __Time;
					_Type = Item_Type.GUI_HITTEST;
					_GUI = eachElement;
					return;
				}
			}
		}
		public Input_Collision(Camera _Active_Camera, float __Time) {
			// Overload to detect collisions with objects in space
			_Time = __Time;
			_Type = Item_Type.OBJ_RAYCAST;
			_Is_Hit = Physics.Raycast(_Active_Camera.ScreenPointToRay(Input.mousePosition), out _Hit, 100, 
						1 << LayerMask.NameToLayer("__Mouse Collision"));
		}
		
		public string Name {
			get { return _Hit.collider != null ? _Hit.collider.name
						: (_GUI != null ? _GUI.name : ""); }	
		}
		
		public bool Type__Object
			{ get { return _Type == Item_Type.OBJ_RAYCAST; } }
		public bool Type__GUI
			{ get { return _Type == Item_Type.GUI_HITTEST; } }
		
		public bool Key__Pressed
			{ get { return _State__Key == Key_States.PRESSED; } }
		public bool Key__Held
			{ get { return _State__Key == Key_States.HELD; } }
		public bool Key__Released
			{ get { return _State__Key == Key_States.RELEASED; } }
		public bool Mouse__On
			{ get { return _State__Mouse == Key_States.MOUSE_ON; } }
		public bool Mouse__Held
			{ get { return _State__Mouse == Key_States.MOUSE_HELD; } }
		public bool Mouse__Off
			{ get { return _State__Mouse == Key_States.MOUSE_OFF; } }
		
		public bool Action__None
			{ get { return _Action == __Actions.Actions.NONE; } }
		public bool Action__Click
			{ get { return _Action == __Actions.Actions.CLICK; } }
		public bool Action__Pause 
            { get { return _Action == __Actions.Actions.PAUSE; } }
		public bool Action__Printscreen 
            { get { return _Action == __Actions.Actions.PRINTSCREEN; } }
		public bool Action__Escape 
            { get { return _Action == __Actions.Actions.ESCAPE; } }
	}
	
	Input_Collision _Collision__GUI,
					_Collision__OBJ;
	List<Input_Collision> _List_Collisions = new List<Input_Collision>();
	bool _Capture_Enabled = false,
			_Capture_Multiple = false;
	Actions _Capture_Trigger = Actions.CLICK,
			_Capture_Cancel = Actions.ESCAPE;
	_Capture_Delegate _Capture_Function;
	public delegate bool _Capture_Delegate(Input_Collision _Collision);
		
	/* Unity standard functions */
	void Awake () {
		_Game = GameObject.Find("__Game Controller").GetComponent<__Game>();
		_Audio_Handler = GameObject.Find("__Game Controller").GetComponent<__Audio>();

        _Player = GameObject.Find("__Player");
        _HUD = GameObject.Find("__HUD").GetComponent<__HUD>();
        _Flight = GameObject.Find("__Player").GetComponent<__Ship>();

		_GUI__Rescan();
	}
	void Update () {
		if (_GetKeyDown(Actions.PAUSE))
			_Game.Pause__Toggle();
		if (_GetKeyDown(Actions.PRINTSCREEN))
			StartCoroutine(Screenshot());
		
		Collisions();
		Interface();
		Move();
        Interact();
	}
	
	public void _GUI__Rescan() {
		_GUI__Items.Clear();
		_GUI__Items.AddRange(GameObject.FindObjectsOfType(typeof(GUIElement)) as GUIElement[]);
	}
	
	void Collisions() {
		// Process all existing mouse clicks held down
		// Note: this only begins running the frame *after* any collisions begun
		for (int i = _List_Collisions.Count - 1; i >= 0; i--) {
			if (i > _List_Collisions.Count - 1)
				continue;
			
			// Set the mouseover state
			if (_List_Collisions[i]._Type == Input_Collision.Item_Type.GUI_HITTEST)
				_List_Collisions[i]._State__Mouse = _List_Collisions[i]._GUI == _Collision__GUI._GUI
					? Input_Collision.Key_States.MOUSE_HELD
					: Input_Collision.Key_States.MOUSE_OFF;
			else if (_List_Collisions[i]._Type == Input_Collision.Item_Type.OBJ_RAYCAST)
				_List_Collisions[i]._State__Mouse = _List_Collisions[i]._Hit.collider == _Collision__OBJ._Hit.collider
					? Input_Collision.Key_States.MOUSE_HELD
					: Input_Collision.Key_States.MOUSE_OFF;
			
			// Set the keypress state
			if (_GetKeyUp(_List_Collisions[i]._Action))
				_List_Collisions[i]._State__Key = Input_Collision.Key_States.RELEASED;
			else if (_GetKey(_List_Collisions[i]._Action))
				_List_Collisions[i]._State__Key = Input_Collision.Key_States.HELD;
			else {
				_List_Collisions[i]._Action = Actions.NONE;
				_List_Collisions[i]._State__Key = Input_Collision.Key_States.NONE;
			}
			
			// Keep it or kill it. 
			// Note: if the key is released, a new collision will be propogated next frame anyway... so kill it.
			if (_List_Collisions[i].Key__Held || _List_Collisions[i].Mouse__Held)
				Collision_Hold(_List_Collisions[i]);
			else  
				Collision_Remove(_List_Collisions[i]);
		}	
	}
	public void Collision_Add (Input_Collision _Collision) {
		// Make sure we're not adding the same keypress/mouseover from earlier...
		for (int i = _List_Collisions.Count - 1; i >= 0; i--) {
			if (i > _List_Collisions.Count - 1)
				continue;
			
			if (_List_Collisions[i]._Type != _Collision._Type	)
				continue;
			else if ((_List_Collisions[i].Type__GUI 
						&& _List_Collisions[i]._GUI == _Collision._GUI)
					|| (_List_Collisions[i].Type__Object 
						&& _List_Collisions[i]._Hit.collider == _Collision._Hit.collider)) {
				if (_List_Collisions[i].Action__None && !_Collision.Action__None) {
					_List_Collisions.RemoveAt(i);
					break;
				}
				else if (_Collision.Action__None && _List_Collisions[i].Action__None)
					return;
			}
		}
		
		// Collision actions are only assigned for keypresses, so this is inferred
		if (_Collision._Action != Actions.NONE)
			_Collision._State__Key = Input_Collision.Key_States.PRESSED;
		
		// Blast off a message to the object!
		if (_Collision._Type == Input_Collision.Item_Type.OBJ_RAYCAST)
			_Collision._Hit.collider.SendMessageUpwards("__Input", _Collision, SendMessageOptions.DontRequireReceiver);
		else if (_Collision._Type == Input_Collision.Item_Type.GUI_HITTEST)
			_Collision._GUI.SendMessageUpwards("__Input", _Collision, SendMessageOptions.DontRequireReceiver);
		_List_Collisions.Add(_Collision);
	}
	public void Collision_Hold (Input_Collision _Collision) {
		if (_Collision._Type == Input_Collision.Item_Type.OBJ_RAYCAST)
			_Collision._Hit.collider.SendMessageUpwards("__Input", _Collision, SendMessageOptions.DontRequireReceiver);
		else if (_Collision._Type == Input_Collision.Item_Type.GUI_HITTEST)
			_Collision._GUI.SendMessageUpwards("__Input", _Collision, SendMessageOptions.DontRequireReceiver);
	}
	public void Collision_Remove (Input_Collision _Collision) {
		_List_Collisions.FindAll((obj) => obj._Action == _Collision._Action)
			.ForEach((obj) => {
				if (obj._Type == Input_Collision.Item_Type.OBJ_RAYCAST)
					obj._Hit.collider.SendMessageUpwards("__Input", obj, SendMessageOptions.DontRequireReceiver);
				else if (obj._Type == Input_Collision.Item_Type.GUI_HITTEST)
					obj._GUI.SendMessageUpwards("__Input", obj, SendMessageOptions.DontRequireReceiver);
				_List_Collisions.Remove(obj);
			});
	}
		
	public bool _GetKey (Actions Function) {
		Key_Setting[] _Iter = _Keys.FindAll((obj) => obj._Function == Function).ToArray();
		foreach (Key_Setting eachIter in _Iter)
			if (Input.GetKey(eachIter._Key))
				return true;
		return false;
	}
	public bool _GetKeyDown (Actions Function) {
		Key_Setting[] _Iter = _Keys.FindAll((obj) => obj._Function == Function).ToArray();
		foreach (Key_Setting eachIter in _Iter)
			if (Input.GetKeyDown(eachIter._Key))
				return true;
		return false;
	}
	public bool _GetKeyUp (Actions Function) {
		Key_Setting[] _Iter = _Keys.FindAll((obj) => obj._Function == Function).ToArray();
		foreach (Key_Setting eachIter in _Iter)
			if (Input.GetKeyUp(eachIter._Key))
				return true;
		return false;
	}
	
	public void Capture (Actions _Trigger, Actions _Cancel, _Capture_Delegate _Trigger_Function, bool _Select_Multiple) {
		_Capture_Enabled = true;
		_Capture_Trigger = _Trigger;
		_Capture_Cancel = _Cancel;
		_Capture_Function = _Trigger_Function;
		_Capture_Multiple = _Select_Multiple;
	}
	
	void Interface() {
		if (!_Game._Can_Interface)
			return;
		
		/* Collisions with all GUI Elements */
		// Process new collisions with GameObject colliders
		_Collision__GUI = new Input_Collision(_GUI__Items, Input.mousePosition, _Game._Time);
		
		if (_Collision__GUI._Is_Hit) {
			_Collision__GUI._State__Mouse = Input_Collision.Key_States.MOUSE_ON;
			
			if (_GetKeyDown(Actions.CLICK))
				_Collision__GUI._Action = Actions.CLICK;
			
			Collision_Add(_Collision__GUI);
		}
	}
	void Move() {
		if (!_Game._Can_Move)
			return;

        _Flight.Move(-Input.GetAxis("Axis_4"), 
            -Input.GetAxis("Axis_2"), Input.GetAxis("Axis_1"), -Input.GetAxis("Axis_3"),
            Input.GetAxis("Axis_5"), Input.GetAxis("Axis_6"));
        
        if (_GetKey(Actions.THRUST_DRAG__ALL))
            _Flight.Drag_All();
        if (_GetKeyDown(Actions.THRUST_DRAG__TOGGLE))
            _Flight.Drag_Toggle();
	}	
    void Interact() {

        // Weapons are constant-fire, have their own timers
        if (_GetKey(Actions.FIRE_MAIN))
            _Flight.Fire_Main();
        if (_GetKey(Actions.FIRE_SECONDARY))
            _Flight.Fire_Secondary();

        if (_GetKeyDown(Actions.TARGET_ENEMY))
            _HUD.Target__Enemy();
        if (_GetKeyDown(Actions.TARGET_FRIENDLY))
            _HUD.Target__Friendly();
        if (_GetKeyDown(Actions.TARGET_NEAREST))
            _HUD.Target__Nearest();
    }

	
	IEnumerator Screenshot () {
		if (!Application.isWebPlayer) {
	        yield return new WaitForEndOfFrame();
			
			Texture2D _Screen = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
			_Screen.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
			_Screen.Apply();
			byte[] _Capture = _Screen.EncodeToPNG();
			Directory.CreateDirectory(String.Format("{0}/Screencaps", Application.dataPath));
			
			FileStream _Output = new FileStream(
				String.Format("{0}/Screencaps/scrcap.sfr.{1}.png", Application.dataPath, DateTime.Now.ToString("MMddyyHHmmff")),
				FileMode.CreateNew);
			
			_Output.Write(_Capture, 0, _Capture.Length);
			Debug.Log(String.Format("Screenshot written to {0}/../Screencap{1}.png", Application.dataPath, DateTime.Now.ToString("MMddyyHHmmff")));
		}
	}
	
	Camera Active_Camera () {
		return camera ? camera : Camera.main; }
}