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
	public float grappleTolerence;
	public float grabTolerence;
	float lastDist;
	Vector3 lastPos;
	public float swingMultiplier;
	public LayerMask mask;
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
	
	void Start() {
		barrelEnd = transform.Find("Spawn");
		lr = GetComponent<LineRenderer>();
		lr.enabled = false;
		pc = GetComponentInParent<PlayerController>();
		rb = GetComponentInParent<Rigidbody>();
		barrelRenderer = transform.Find("Barrel").GetComponent<MeshRenderer>();
		
		// initialize input
		if (transform.parent.name.Contains ("Left")) {
			inputSources = SteamVR_Input_Sources.LeftHand;
		} else {
			inputSources = SteamVR_Input_Sources.RightHand;
		}
	}

	void GrappleAttach (GrappleData data) {
		if (otherGun.grabed != null) {
			if (data.obj == otherGun.grabed.transform) {
				return;
			}
		}
		

		state = State.grappling;
		GetNewAnchor(data.pos, data.obj);

		lr.enabled = true;
		UpdateLine();

		Vector3 diff = grabed.anchorPos() - barrelEnd.position;
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

	void Grab (Transform obj) {
		state = State.grabing;
		GetNewAnchor(obj.position, obj);
		rb.isKinematic = true;

		barrelRenderer.material.color = Color.green;

		otherGun.OtherGunGrabbed(obj);

		lastPos = barrelEnd.position;
	}

	void EndGrab ()
	{
		barrelRenderer.material.color = Color.red;
		rb.isKinematic = false;

		// calculate departing velocity
		Vector3 diff = lastPos - barrelEnd.position;
		rb.AddForce(diff * grabThrowForceMul);
	}

	void Detach ()
	{
		if (state == State.grappling) {
			lr.enabled = false;
		} else if (state == State.grabing) {
			EndGrab();
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
		lr.SetPosition(1, grabed.anchorPos());
	}

	void Update() {
		if (SteamVR_Input._default.inActions.GrabPinch.GetStateDown(inputSources)) {
			if (state == State.detached) {
				// grab takes priority
				Transform target = GetGrabTarget();
				if (target != null) {
					Grab(target);
				} else {
					GrappleData data = GetGrappleTarget();
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

			Vector3 diff = grabed.anchorPos() - barrelEnd.position;
			float dist = diff.magnitude;
			float distDiff = dist - lastDist;
			float forceMul = 1f;
			if (distDiff > 0) {
				forceMul = 1f + (distDiff * swingMultiplier); // to make it so that the force is greater when you are on the upside of a swing
			}

			if (dist > grabTolerence * 2f) {
				rb.AddForce(diff.normalized * swingForce * forceMul * Time.deltaTime);
			} else {
				Transform possibleGrab = GetGrabTarget();
				if (possibleGrab != null) {
					Detach();
					Grab(possibleGrab);
				}
			}
			lastDist = dist;
		} else if (state == State.grabing) {
			Vector3 handDiff = lastPos - barrelEnd.position;
			rb.transform.Translate(handDiff + grabed.anchorDiff(), Space.World);

			lastPos = barrelEnd.position;

		}
	}

	GrappleData GetGrappleTarget ()
	{
		RaycastHit hit;
		Physics.SphereCast(barrelEnd.position, grappleTolerence, barrelEnd.transform.forward, out hit, maxRange, mask);
		return new GrappleData (hit.point, hit.transform);
	}

	Transform GetGrabTarget ()
	{
		Collider[] colls = Physics.OverlapSphere(barrelEnd.position, grabTolerence, mask);
		if (colls.Length > 0) {
			return colls[0].transform;
		} else {
			return null;
		}
	}

	private struct GrappleData
	{
		public Vector3 pos;
		public Transform obj;

		public GrappleData (Vector3 pos, Transform obj)
		{
			this.pos = pos;
			this.obj = obj;
		}
	}
}
