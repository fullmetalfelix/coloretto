using UnityEngine;
using System.Collections;


[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class Mesh2D : MonoBehaviour {

	public Texture2D icon;
	//public Unit unit;
	public float scalefactor = 10;

	/// <summary>
	/// If true the scale will be adjusted with respect to the camera distance. 
	/// Otherwise, the scale will be the one set in inspector.
	/// </summary>
	public bool scalewithcamera = true;
	protected Vector3 screenpos;
	protected Vector3 screenupright;
	protected float diagonal;

	public bool scalecollider = true;
	public SphereCollider spherecollider;
	protected float initialradius;

	/// <summary>
	/// Set to true to make the billboard y axis point in the forward direction?
	/// </summary>
	public bool yToForward = false;
	/// <summary>
	/// If yToForward is true, set a transform to give the forward direction. Set in inspector.
	/// </summary>
	public Transform yDirector;

	protected Vector3 yp, zparent;
	// Use this for initialization
	void Start ()
	{


		#region "create mesh"

		Mesh m = new Mesh(); //GetComponent<MeshFilter>().mesh;
		m.Clear();
		m.vertices = new Vector3[] {
			-Vector3.right+Vector3.up, Vector3.right+Vector3.up,
			 Vector3.right-Vector3.up, -Vector3.right-Vector3.up};
		m.triangles = new int[] { 0, 1,2, 2,3,0 };
		m.uv = new Vector2[] {
		Vector2.up,Vector2.up+Vector2.right, Vector2.right, Vector2.zero};

		m.RecalculateBounds();
		m.RecalculateNormals();

		GetComponent<MeshFilter>().mesh = m;
		Material mat = GetComponent<MeshRenderer>().material;
		mat.mainTexture = icon;
		Color c = new Color(0.5f,0.5f,0.5f,1);
		mat.SetColor("_TintColor", c);

		#endregion

		//transform.localScale = new Vector3(1, 1, 1);


		if (scalecollider)
		{
			//spherecollider = unit.GetComponent<SphereCollider>();
			if (spherecollider == null)
				scalecollider = false;
			else
				initialradius = spherecollider.radius;
		}

	}
	
	// Update is called once per frame
	void Update () {

		transform.rotation = Camera.main.transform.rotation;
		if (yToForward)
		{
			//rotate around z until the projection of y on the camera xy plane
			//is pointing at a forward (parents forward for now)
			yp = transform.up - Vector3.Dot(transform.up, Camera.main.transform.forward) * 
				Camera.main.transform.forward;

			zparent = yDirector.forward - Vector3.Dot(yDirector.forward, Camera.main.transform.forward) *
				Camera.main.transform.forward;

			transform.Rotate(Quaternion.FromToRotation(yp, zparent).eulerAngles);
			//transform.RotateAroundLocal(Vector3.forward, AngleHelper.Angle(transform.up,
			//    transform.parent.forward, Vector3.up));

		}

		//transform.LookAt(Camera.main.transform);

		//rescale the transform according to the distance camera-center
		//the idea is to have a constant size in screen space

		//compute the diagonal in screen space
		//screenpos = Camera.main.WorldToScreenPoint(transform.position);
		//screenupright = Camera.main.WorldToScreenPoint(transform.position + Vector3.up + Vector3.right);
		//diagonal = (screenupright - screenpos).magnitude;
		//the idea is to scale so that the diagonal is constant

		//transform.localScale = new Vector3(5, 5, 5)*scalefactor/diagonal;

		//----------------------------------------------
		// far camera->large diagonal->bigger scaling factor
		diagonal = Camera.main.transform.position.magnitude;
		diagonal += 0.03f * (Camera.main.transform.position - transform.position).magnitude;

		if (scalewithcamera)
		{
			transform.localScale = Vector3.one * scalefactor * diagonal;
		}

		if (scalecollider)
		{
			diagonal = Mathf.Max(initialradius, initialradius * scalefactor * diagonal);
			spherecollider.radius = diagonal;

		}

	}


}
