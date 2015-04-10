using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class SpriteAnimator : MonoBehaviour
{
	protected MeshRenderer meshren;


	#region "fade animation"

	protected bool fading = false;
	protected float fadetime;
	protected float alphat;
	protected float alpha;
	protected float valpha;


	public void SetAlpha(float a)
	{
		if (meshren == null) return;

		Color c = meshren.material.GetColor("_TintColor");
		c.a = a;
		meshren.material.SetColor("_TintColor", c);

		c = meshren.material.GetColor("_BackTintColor");
		c.a = a;
		meshren.material.SetColor("_BackTintColor", c);

	}
	public void FadeIn(float maxalpha, float time)
	{
		//SetAlpha(0);
		alphat = Mathf.Clamp01(maxalpha);
		fadetime = time;

		fading = true;
	}
	public void FadeIn(float startalpha, float maxalpha, float time)
	{
		SetAlpha(startalpha);
		alphat = Mathf.Clamp01(maxalpha);
		fadetime = time;

		fading = true;
	}
	public void FadeOut(float minalpha, float time)
	{
		//SetAlpha(0);
		alphat = Mathf.Clamp01(minalpha);
		fadetime = time;

		fading = true;
	}
	protected void DoFade()
	{
		if (fading)
		{
			//Debug.Log("fading " + alpha.ToString());
			alpha = Mathf.SmoothDamp(alpha, alphat, ref valpha, fadetime);

			if (Mathf.Abs(alpha - alphat) < 0.001f)
			{
				alpha = alphat;
				fading = false;
			}
			SetAlpha(alpha);
		}

	}

	#endregion

	#region "size"
	protected Vector3 originalscale;

	protected bool sizing = false, sizeoscill = false;
	public float sizetime, size,sizet,vsize, sizefreq, sizemin,sizemax, sizeoscillphi = 0;

	public void SizePulse(float startsize, float maxsize, float time)
	{
		transform.localScale = originalscale * startsize;
		StartCoroutine(SizePulser(startsize, maxsize, time));

		
	}
	protected IEnumerator SizePulser(float startsize, float maxsize, float time)
	{
		sizing = true;
		sizet = maxsize;
		sizetime = time * 0.3f;
		
		yield return new WaitForSeconds(time * 0.5f);

		sizing = true;
		sizet = startsize;
		  
	}

	public void SizeOscillate(float min, float max, float period)
	{
		sizefreq = 1.0f/period;
		sizemin = min; sizemax = max;
		sizing = false; sizeoscill = true;
		size = 1;
		StartCoroutine(SizeOsciller(min, max, period));

	}
	public void SizeOscillateStop(float time)
	{
		if (!sizeoscill) return;

		sizeoscillphi = 0;
		sizeoscill = false;
		sizing = true;
		sizet = 1;
		sizetime = time;
	}
	protected IEnumerator SizeOsciller(float min, float max, float time)
	{
		yield return new WaitForSeconds(time * 0.1f);
		/*
		while (sizeoscill)
		{
			//Debug.Log("starting oscillation");
			sizing = true;
			sizet = sizemax;
			sizetime = time * 0.3f;

			yield return new WaitForSeconds(time * 0.5f);

			sizing = true;
			sizet = sizemin;
			sizetime = time * 0.3f;
			//Debug.Log("deflating oscillation");
		}
		*/
	}


	protected void DoSize() {

		if (sizing && !sizeoscill)
		{
			//Debug.Log("fading " + alpha.ToString());
			size = Mathf.SmoothDamp(size, sizet, ref vsize, sizetime);

			if (Mathf.Abs(size - sizet) < 0.001f)
			{
				size = sizet;
				sizing = false;
			}
			transform.localScale = originalscale * size;
		}


		if (sizeoscill)
		{
			sizeoscillphi += 2 * Mathf.PI * sizefreq*Time.deltaTime;
			sizeoscillphi = sizeoscillphi % (2 * Mathf.PI);
			float tmp = (0.5f * (sizemax - sizemin)) * (0.5f * Mathf.Sin(sizeoscillphi)) + (sizemax + sizemin) * 0.5f;
			size = Mathf.SmoothDamp(size, tmp, ref vsize, 0.5f / sizefreq);
			//Debug.Log(sizeoscillphi);

			transform.localScale = originalscale * size;
		}


	}

	#endregion

	#region "flip"
	
	protected bool flipping = false;
	protected float flipangle, flipanglet, vflipangle, fliptime;
	public void Flip(float angle, float time)
	{
		flipanglet = angle;
		fliptime = time;

		flipping = true;
	}

	protected void DoFlip()
	{
		if (flipping)
		{

			float myflip = Vector3.Angle(transform.forward, Vector3.forward);
			//if (transform.forward == -Vector3.forward)
				//myflip = 180;
			//else
				myflip *= Mathf.Sign(transform.forward.x);

			//myflip *= Mathf.Rad2Deg;

			flipangle = Mathf.SmoothDampAngle(myflip, flipanglet, ref vflipangle, fliptime);
			
			//Debug.Log(flipangle.ToString()+" "+myflip.ToString()+" "+flipanglet.ToString());

			if (Mathf.Abs(flipanglet - flipangle) < 0.1f)
			{
				flipangle = flipanglet;
				flipping = false;
			}
			
			Vector3 newz = Vector3.RotateTowards(transform.forward, transform.right, 
				-Mathf.Deg2Rad*(myflip-flipangle), 0);

			transform.RotateAround(transform.up, -(myflip - flipangle)*Mathf.Deg2Rad);
			//transform.LookAt(transform.position + newz, transform.position + transform.up);
		}

	}

	#endregion

	#region "rotate around global z"

	protected bool rolling = false;
	protected float rollangle, rollanglet, vrollangle, rolltime;
	public void Roll(float angle, float time)
	{
		rollanglet = angle;
		rolltime = time;

		rolling = true;
	}

	protected void DoRoll()
	{
		if (rolling)
		{
			float myroll = Vector3.Angle(transform.up, Vector3.up);
			myroll *= Mathf.Sign(transform.up.x);
			rollangle = Mathf.SmoothDamp(myroll, rollanglet, ref vrollangle, rolltime);
			//Debug.Log((myroll - rollangle).ToString() + " " + myroll.ToString() + " " + rollanglet.ToString());

			if (Mathf.Abs(rollanglet - rollangle) < 0.01f)
			{
				rollangle = rollanglet;
				rolling = false;
			}
			Vector3 newy = Vector3.RotateTowards(transform.up, Vector3.Cross(transform.up, Vector3.forward),
				-Mathf.Deg2Rad * (myroll - rollangle), 0);
			transform.RotateAround(Vector3.forward, (myroll - rollangle)*Mathf.Deg2Rad);

			//transform.localRotation = Quaternion.Euler(eulers);
		}

	}

	#endregion



	public void SetTexture(Texture2D tex)
	{
		GetComponent<MeshRenderer>().material.SetTexture("_MainTex", tex);
	}

	void Start()
	{
		originalscale = transform.localScale;
		meshren = GetComponent<MeshRenderer>();
	}


	void Update()
	{
		DoFade();
		DoSize();
		DoFlip();
		DoRoll();
	}

	/*
	#region "mesh properties"

	/// <summary>
	/// Ratio between width/height of the billboard.
	/// </summary>
	public float aspectratio = 1;
	/// <summary>
	/// Factor giving the final size on the screen.
	/// </summary>
	public float scalefactor = 1;
	/// <summary>
	/// Set to true to make the billboard y axis point in the forward direction
	/// </summary>
	public bool yToForward = false;
	/// <summary>
	/// If yToForward is true, set a transform to give the forward direction. Set in inspector.
	/// </summary>
	public Transform yDirector;

	protected Vector3 yp, zparent;
	protected float diagonal;

	public bool scalewithcamera = true;
	public bool scalemesh = true;
	public bool scalecollider = true;
	/// <summary>
	/// Sphere collider to scale with the mesh. Set in inspector.
	/// </summary>
	public SphereCollider spherecollider;
	protected float initialradius;


	#endregion

	#region "animation"
	public bool animate = true;
	public int rows;
	public int columns;
	public float fps;

	protected Vector2 texsize, texoffset;
	public int frameindex;

	#endregion


	protected virtual void Start()
	{

		//make the quad
		MakeQuad();

		if (scalecollider)
		{
			//spherecollider = unit.GetComponent<SphereCollider>();
			if (spherecollider == null)
				scalecollider = false;
			else
				initialradius = spherecollider.radius;
		}

		texsize = new Vector2(1.0f / columns, 1.0f / rows);
		texoffset = Vector2.zero;
		renderer.material.SetTextureScale("_MainTex", texsize);

		SetFrame();

		if (animate)
			StartCoroutine("Animate");


	}

	protected virtual void MakeQuad()
	{

		Mesh m = new Mesh(); //GetComponent<MeshFilter>().mesh;
		m.Clear();
		m.vertices = new Vector3[] {
			-Vector3.right*aspectratio+Vector3.up, Vector3.right*aspectratio+Vector3.up,
			 Vector3.right*aspectratio-Vector3.up, -Vector3.right*aspectratio-Vector3.up};
		m.triangles = new int[] { 0, 1, 2, 2, 3, 0 };
		m.uv = new Vector2[] {
		Vector2.up,Vector2.up+Vector2.right, Vector2.right, Vector2.zero};

		m.RecalculateBounds();
		m.RecalculateNormals();

		GetComponent<MeshFilter>().mesh = m;

		//Material mat = GetComponent<MeshRenderer>().material;
		//mat.mainTexture = icon;
		//Color c = new Color(0.5f, 0.5f, 0.5f, 1);
		//mat.SetColor("_TintColor", c);


	}


	// Update is called once per frame
	void Update ()
	{

		#region "billboard rotation"
		transform.rotation = Camera.main.transform.rotation; //use the same as camera rotation
		if (yToForward)
		{
			//rotate around z until the projection of y on the camera xy plane
			//is pointing at a forward (parents forward for now)
			yp = transform.up - Vector3.Dot(transform.up, Camera.main.transform.forward) *
				Camera.main.transform.forward;

			zparent = yDirector.forward - Vector3.Dot(yDirector.forward, Camera.main.transform.forward) *
				Camera.main.transform.forward;

			transform.Rotate(Quaternion.FromToRotation(yp, zparent).eulerAngles);

		}
		#endregion

		#region "scaling"
		// far camera->large diagonal->bigger scaling factor


		if (scalewithcamera)
		{

			diagonal = Camera.main.transform.position.magnitude;
			diagonal += 0.03f * (Camera.main.transform.position - transform.position).magnitude;

			if (scalemesh)
				transform.localScale = Vector3.one * scalefactor * diagonal;

			if (scalecollider)
			{
				diagonal = Mathf.Max(initialradius, initialradius * scalefactor * diagonal);
				spherecollider.radius = diagonal;

			}

		}


		#endregion

		//index = Mathf.FloorToInt(Time.time * fps);
		//if (index != cindex)
		//{
		//    cindex = index;
		//    index = index % (columns * rows);

		//    offset.x = (index % columns) * size.x;
		//    offset.y = 1.0f - size.y - ((float)index / rows) * size.y;
		//    renderer.material.SetTextureOffset("_MainTex", offset);


		//}
	}

	protected virtual IEnumerator Animate()
	{
		while (true)
		{
			//move to the next index
			frameindex++;
			if (frameindex >= rows * columns)
				frameindex = 0;

			SetFrame();


			yield return new WaitForSeconds(1f / fps);
		}

	}

	protected virtual void SetFrame()
	{
		//split into x and y indexes
		texoffset.x = (float)frameindex / columns - (frameindex / columns);
		texoffset.y = (frameindex / columns) / (float)rows;

		renderer.material.SetTextureOffset("_MainTex", texoffset);

	}


	*/
}
