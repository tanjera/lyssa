using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
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
                if (infoCollision.gameObject.GetComponent<Board_Item>().Is_Dragging == false)
                    Board.Destroy_Item(infoCollision.gameObject.GetComponent<Board_Item>().Item);
    }


    /* Board_Item rigidbody dragging */

    public bool Is_Dragging = false;
    public float force = 600;
	public float damping = 6;
	
	Transform jointTrans;
	float dragDepth;

	void OnMouseDown ()
	{
		HandleInputBegin (Input.mousePosition);
        Is_Dragging = true;
	}
	
	void OnMouseUp ()
	{
		HandleInputEnd (Input.mousePosition);
        if (gameObject.GetComponent<Board_Item>() != null)
            gameObject.GetComponent<Board_Item>().Board.Destroy_Item(gameObject.GetComponent<Board_Item>().Item);
        Board.Iterate_Matrix();
        Is_Dragging = false;
	}
	
	void OnMouseDrag ()
	{
		HandleInput (Input.mousePosition);
	}
	
	public void HandleInputBegin (Vector3 screenPosition)
	{
		var ray = Camera.main.ScreenPointToRay (screenPosition);
		RaycastHit hit;
		if (Physics.Raycast (ray, out hit)) {
			if (hit.transform.gameObject.layer == LayerMask.NameToLayer ("Interactive")) {
				dragDepth = CameraPlane.CameraToPointDepth (Camera.main, hit.point);
				jointTrans = AttachJoint (hit.rigidbody, hit.point);
			}
		}
	}
	
	public void HandleInput (Vector3 screenPosition)
	{
		if (jointTrans == null)
			return;
		var worldPos = Camera.main.ScreenToWorldPoint (screenPosition);
		jointTrans.position = CameraPlane.ScreenToWorldPlanePoint (Camera.main, dragDepth, screenPosition);
	}
	
	public void HandleInputEnd (Vector3 screenPosition)
	{
		Destroy (jointTrans.gameObject);
	}
	
	Transform AttachJoint (Rigidbody rb, Vector3 attachmentPosition)
	{
		GameObject go = new GameObject ("Attachment Point");
		go.hideFlags = HideFlags.HideInHierarchy; 
		go.transform.position = attachmentPosition;
		
		var newRb = go.AddComponent<Rigidbody> ();
		newRb.isKinematic = true;
		
		var joint = go.AddComponent<ConfigurableJoint> ();
		joint.connectedBody = rb;
		joint.configuredInWorldSpace = true;
		joint.xDrive = NewJointDrive (force, damping);
		joint.yDrive = NewJointDrive (force, damping);
		joint.zDrive = NewJointDrive (force, damping);
		joint.slerpDrive = NewJointDrive (force, damping);
		joint.rotationDriveMode = RotationDriveMode.Slerp;
		
		return go.transform;
	}
	
	private JointDrive NewJointDrive (float force, float damping)
	{
		JointDrive drive = new JointDrive ();
		drive.mode = JointDriveMode.Position;
		drive.positionSpring = force;
		drive.positionDamper = damping;
		drive.maximumForce = Mathf.Infinity;
		return drive;
	}
}
