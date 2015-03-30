using UnityEngine;
using System.Collections;



public class Sprite : MonoBehaviour {


	public Texture2D texture;
	public void SetTexture(Texture2D tex)
	{
		texture = tex;
		if (meshobject != null)
			meshobject.GetComponent<MeshRenderer>().material.mainTexture = tex;
	}

	public Color basecolor;
	public void SetColor(Color color)
	{
		basecolor = color;
		if (meshobject != null)
			meshobject.GetComponent<MeshRenderer>().material.color = color;
	}


	protected GameObject meshobject;

	public float aspectratio;
	public float scalefactor;

	protected Vector3 targetpos, targetv;
	protected float tangle, tanglev;

	protected bool ismoving = false;

	public SpriteAnimator animator;

	public static Sprite Fabricate()
	{
		GameObject g = (GameObject)Instantiate(Resources.Load("spritegeneric", typeof(GameObject)));
		

		Sprite s = g.GetComponent<Sprite>();
		//s.CreateSpriteMesh();

		s.transform.LookAt(s.transform.position - Camera.mainCamera.transform.forward);



		return s;
	}




	protected virtual void Start()
	{

		CreateSpriteMesh();

	}

	protected virtual void CreateSpriteMesh()
	{
		meshobject = transform.FindChild("mesh").gameObject;

		Mesh m = new Mesh(); //GetComponent<MeshFilter>().mesh;
		m.Clear();
		m.vertices = new Vector3[] {
			-Vector3.right+Vector3.up, Vector3.right+Vector3.up,
			 Vector3.right-Vector3.up, -Vector3.right-Vector3.up};
		m.triangles = new int[] { 0, 1, 2, 2, 3, 0 };
		m.uv = new Vector2[] {
		Vector2.up,Vector2.up+Vector2.right, Vector2.right, Vector2.zero};

		m.RecalculateBounds();
		m.RecalculateNormals();

		meshobject.GetComponent<MeshFilter>().mesh = m;
		Material mat = meshobject.GetComponent<MeshRenderer>().material;
		mat.mainTexture = texture;
		mat.SetColor("_TintColor", basecolor);

		Vector3 aspect = Vector3.one; aspect.y /= aspectratio;
		meshobject.transform.localScale = aspect * scalefactor;

	}


	protected virtual void Update()
	{
		if (ismoving)
		{
			Move();
		}

	}

	protected virtual void Move()
	{
		transform.position = Vector3.SmoothDamp(transform.position, targetpos, ref targetv, 0.5f);

		/*
		float ang = Mathf.Atan2(meshobject.transform.up.y, meshobject.transform.up.x) * Mathf.Rad2Deg;
		//ang = Vector3.Angle(transform.up, transform.parent.forward) - tangle;
		float ang2 = Mathf.SmoothDampAngle(ang, tangle, ref tanglev, 0.5f);

		//Debug.Log(ang.ToString() + " " + tangle.ToString());

		meshobject.transform.Rotate(Vector3.forward, ang2 - ang);

		//transform.Rotate( Quaternion.RotateTowards(transform.rotation, Quaternion.AngleAxis(tangle, Vector3.forward), 0.001f).eulerAngles);
		*/
		if ((targetpos - transform.position).sqrMagnitude < 0.001f)
		{
			ismoving = false;
		}
	}

	public void SendTo(Vector3 pos, float angle)
	{
		targetpos = pos;
		tangle = angle;
		ismoving = true;

	}



}
