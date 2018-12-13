using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class GunController : MonoBehaviour {
	enum State
	{
		detached,
		grappling,
		grabing
	};
	State state = State.detached;

	public float maxRange;
	public float swingForce;
	public float grabThrowForceMul;
	public float maxThrowForce;
	public float grappleTolerence;
	public float grabTolerence;
	float lastDist;
	Vector3 lastPos;
	Vector3 grabOffset;
	public float swingMultiplier;
	public LayerMask mask;
	public LayerMask collidableLayers;
	public Grabable grabed = null;

	// visuals


	// input
	SteamVR_Input_Sources inputSources;
	public SteamVR_Action_Single triggerAction;

	// references
	public GunController otherGun;
	Transform barrelEnd;
	LineRenderer lr;
	PlayerController pc;
	Rigidbody rb;
	MeshRenderer barrelRenderer;
	CapsuleCollider capsule;
	
	void Start() {
		barrelEnd = transform.Find("Spawn");
		lr = GetComponent<LineRenderer>();
		lr.enabled = false;
		pc = GetComponentInParent<PlayerController>();
		rb = GetComponentInParent<Rigidbody>();
		capsule = GetComponentInParent<CapsuleCollider>();
		barrelRenderer = transform.Find("Barrel").GetComponent<MeshRenderer>();
		
		// initialize input
		if (transform.parent.name.Contains ("Left")) {
			inputSources = SteamVR_Input_Sources.LeftHand;
		} else {
			inputSources = SteamVR_Input_Sources.RightHand;
		}
	}

	void GrappleAttach (AttachData data) {
		if (otherGun.grabed != null) {
			if (data.obj == otherGun.grabed.transform) {
				return;
			}
		}
		
		state = State.grappling;
		GetNewAnchor(data.pos, data.obj);

		lr.enabled = true;
		UpdateLine();

		Vector3 diff = grabed.GetAnchorPos() - barrelEnd.position;
		lastDist = diff.magnitude;
	}

	public void OtherGunGrabbed (Transform obj)
	{
		if (state == State.grabing) {
			Detach();
		} else if (state == State.grappling) {
			if (obj == grabed.transform) {
				Detach();
			}
		}
	}

	void GrabAttach (AttachData data) {
		otherGun.OtherGunGrabbed(data.obj);
		GetNewAnchor(data.pos, data.obj);

		state = State.grabing;
		rb.isKinematic = true;
		barrelRenderer.material.color = Color.green;

		grabOffset = rb.transform.position - data.pos;
		lastPos = barrelEnd.position;
	}

	void EndGrab (bool attached)
	{
		barrelRenderer.material.color = Color.red;
		rb.isKinematic = false;

		if (!attached) {
			// calculate departing velocity
			Vector3 diff = lastPos - barrelEnd.position;
			Vector3 force = Vector3.ClampMagnitude(diff * grabThrowForceMul, maxThrowForce);
			rb.AddForce(force);
		}
	}

	public void Detach (bool attached = false)
	{
		if (state == State.grappling) {
			lr.enabled = false;
		} else if (state == State.grabing) {
			EndGrab(attached);
		}

		state = State.detached;
		if (grabed != null) {
			grabed.DestoryAnchor();
			grabed = null;
		}
	}

	void GetNewAnchor (Vector3 position, Transform obj)
	{
		grabed = obj.GetComponent<Grabable>();
		grabed.CreateAnchor(position);
	}

	void UpdateLine ()
	{
		lr.SetPosition(0, barrelEnd.position);
		lr.SetPosition(1, grabed.GetAnchorPos());
	}

	void Update() {
		if (SteamVR_Input._default.inActions.GrabPinch.GetStateDown(inputSources)) {
			if (state == State.detached) {
				// grab takes priority
				AttachData? target = GetGrabTarget();
				if (target.HasValue) {
					GrabAttach(target.Value);
				} else {
					AttachData data = GetGrappleTarget();
					if (data.obj != null) {
						GrappleAttach(data);
					}
				}
			}
		}

		if (SteamVR_Input._default.inActions.GrabPinch.GetStateUp(inputSources)) {
			Detach();
		}

		if (state == State.grappling) {
			UpdateLine();

			Vector3 diff = grabed.GetAnchorPos() - barrelEnd.position;
			float dist = diff.magnitude;
			float distDiff = dist - lastDist;
			float forceMul = 1f;
			if (distDiff > 0) {
				forceMul = 1f + (distDiff * swingMultiplier); // to make it so that the force is greater when you are on the upside of a swing
			}

			if (dist > grabTolerence * 2f) {
				Vector3 force = diff.normalized * swingForce * forceMul * Time.deltaTime;
				rb.AddForce(force);
				grabed.AddForce(-force);
			}
			lastDist = dist;

			AttachData? possibleGrab = GetGrabTarget();
			if (possibleGrab.HasValue) {
				Detach();
				GrabAttach(possibleGrab.Value);
			}
		} else if (state == State.grabing) {
			Vector3 handDiff = lastPos - barrelEnd.position;
			Vector3 targetPos = grabed.GetAnchorPos() + grabOffset + handDiff;

			// test to see if the move would put the player into an object
			Vector3 capsuleOffset = new Vector3(capsule.center.x, 0f, capsule.center.z);
			Collider[] colls = Physics.OverlapCapsule(targetPos + capsuleOffset + Vector3.up * (capsule.radius), targetPos + capsuleOffset + Vector3.up * (capsule.height - capsule.radius), capsule.radius, collidableLayers);

			if (colls.Length > 0) {
				RecenterPlayer(true);
			} else { // position valid
				grabOffset += handDiff;
				rb.transform.position = grabOffset + grabed.GetAnchorPos();
				RecenterPlayer();
			}

			lastPos = barrelEnd.position;

			// check to make sure still on wall
			AttachData? possibleGrab = GetGrabTarget();
			if (!possibleGrab.HasValue) {
				Detach();
				return;
			}

			// apply downward force to object we are grabbing
			grabed.AddForce(rb.mass * Physics.gravity);
		}

		if (otherGun.state != State.grabing && state != State.grabing) {
			RecenterPlayer();
		}
	}

	void RecenterPlayer (bool compensate = false) {
		Vector3 curCenter = capsule.center;
		capsule.center = new Vector3(Camera.main.transform.localPosition.x, capsule.center.y, Camera.main.transform.localPosition.z);
		if (compensate) {
			rb.transform.Translate(-capsule.center + curCenter);
		}
	}

	AttachData GetGrappleTarget ()
	{
		RaycastHit hit;
		Physics.SphereCast(barrelEnd.position, grappleTolerence, barrelEnd.transform.forward, out hit, maxRange, mask);
		return new AttachData (hit.point, hit.transform);
	}

	AttachData? GetGrabTarget ()
	{
		Collider[] colls = Physics.OverlapSphere(barrelEnd.position, grabTolerence, mask);
		if (colls.Length > 0) {
			return new AttachData (transform.position, colls[0].transform);
		} else {
			return null;
		}
	}

	private struct AttachData
	{
		public Vector3 pos;
		public Transform obj;

		public AttachData (Vector3 pos, Transform obj)
		{
			this.pos = pos;
			this.obj = obj;
		}
	}
}
