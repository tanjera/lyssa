using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Board_Item : MonoBehaviour {

	[HideInInspector]
    Game_Handler Game_Handler;

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

    public void Start() {
        Game_Handler = (Game_Handler)GameObject.FindObjectOfType(typeof(Game_Handler));
    }
    public void OnCollisionEnter(Collision infoCollision) {
        if (infoCollision.gameObject.GetComponent<Board_Item>() != null
                && infoCollision.gameObject.GetComponent<Board_Item>().isDragging == false)
            Board.Destroy_Item(infoCollision.gameObject.GetComponent<Board_Item>().Item);
    }


    /* Board_Item rigidbody dragging */

    public bool isDragging = false;
    public float Force = 1000;
	public float Damping = 0;
	
	Transform jointTrans;
	float dragDepth;

	void OnMouseDown ()
	{
		HandleInputBegin (Input.mousePosition);
        Board.Release_Item(this);
        isDragging = true;
	}
	void OnMouseUp () {
		HandleInputEnd (Input.mousePosition);
        if (gameObject.GetComponent<Board_Item>() != null)
            gameObject.GetComponent<Board_Item>().Board.Destroy_Item(gameObject.GetComponent<Board_Item>().Item);
        isDragging = false;
	}
	void OnMouseDrag () {
		HandleInput (Input.mousePosition);
	}
	
	public void HandleInputBegin (Vector3 screenPosition) {
		Ray ray = Camera.main.ScreenPointToRay (screenPosition);
		RaycastHit hit;
		if (Physics.Raycast (ray, out hit)) {
			if (hit.transform.gameObject.layer == LayerMask.NameToLayer ("Interactive")) {
				dragDepth = CameraPlane.CameraToPointDepth (Camera.main, hit.point);
				jointTrans = AttachJoint (hit.rigidbody, hit.point);
                Game_Handler.Scale(hit.transform, hit.transform.localScale * 0.8f, 0.1f);
			}
		}
	}
	public void HandleInput (Vector3 screenPosition) {
		if (jointTrans == null)
			return;
		jointTrans.position = Camera.main.ScreenToWorldPoint (screenPosition);
	}
	public void HandleInputEnd (Vector3 screenPosition) {
		Destroy (jointTrans.gameObject);
	}
	
	Transform AttachJoint (Rigidbody body, Vector3 attachPoint) {
		GameObject obj = new GameObject ("Attachment Point");
		obj.hideFlags = HideFlags.HideInHierarchy; 
		obj.transform.position = attachPoint;
		
		Rigidbody rigid = obj.AddComponent<Rigidbody> ();
		rigid.isKinematic = true;
		
		ConfigurableJoint joint = obj.AddComponent<ConfigurableJoint> ();
		joint.connectedBody = body;
		joint.configuredInWorldSpace = true;
		joint.xDrive = NewJointDrive (Force, Damping);
		joint.yDrive = NewJointDrive (Force, Damping);
		joint.zDrive = NewJointDrive (Force, Damping);
		joint.slerpDrive = NewJointDrive (Force, Damping);
		joint.rotationDriveMode = RotationDriveMode.Slerp;
		
		return obj.transform;
	}
	private JointDrive NewJointDrive (float force, float damping) {
		JointDrive drive = new JointDrive ();
		drive.mode = JointDriveMode.Position;
		drive.positionSpring = force;
		drive.positionDamper = damping;
		drive.maximumForce = Mathf.Infinity;
		return drive;
	}
}
