using UnityEngine;
using System.Collections;


public class GameSetup : MonoBehaviour {

	public GameObject seatprefab;

	public int nseats = 4;
	public float aspect = 1;
	public float seatmaxdistance = 10;
	public float seatdist = 0;
	protected float seatv = 0;

	public GameObject[] slots;
	protected Vector3[] slotpos;
	public SeatType[] slottypes;
	public Texture2D[] slotbuttons;
	public int[] slotsID;

	public Texture2D startbuttontex;
	protected GameObject startbutton;

	protected bool buttonsON = true;

	// Use this for initialization
	void Start () {
	
		//create seats
		slots = new GameObject[nseats];
		slotpos = new Vector3[nseats];
		slotsID = new int[nseats];

		for (int i = 0; i < nseats; i++)
		{

			slots[i] = (GameObject)Instantiate(seatprefab);
			Quaternion q = Quaternion.AngleAxis(i * 360.0f / nseats, Vector3.forward);
			slotpos[i] = q * Vector3.up;
			slotpos[i].y /= aspect;

			slots[i].transform.position = slotpos[i];
			slots[i].GetComponent<Button2D>().icon = (i == 0) ? slotbuttons[0] : slotbuttons[1];
			slotsID[i] = (i == 0) ? 0 : 1;

			slots[i].transform.parent = transform;

		}


		startbutton = (GameObject)Instantiate(seatprefab);
		startbutton.GetComponent<Button2D>().SetImage(startbuttontex);
		startbutton.transform.parent = transform;

	}
	
	// Update is called once per frame
	void Update () {

		if (slots == null || !buttonsON)
			return;


		if (Mathf.Abs(seatdist - seatmaxdistance) > 0.1f)
		{

			seatdist = Mathf.SmoothDamp(seatdist, seatmaxdistance, ref seatv, 0.5f);
			for (int i = 0; i < nseats; i++)
				slots[i].transform.position = slotpos[i]* seatdist;

		}
		


	}

	void StartGame()
	{
		Debug.Log("calling game start...");

		GetComponent<GameLogic>().SetupTable(slotsID);

		//destroy the buttons somehow
		for (int i = 0; i < nseats; i++)
		{
			Destroy(slots[i]);
		}

		Destroy(startbutton);
		buttonsON = false;

	}


	void ButtonClick(Button2D button)
	{
		if (button.gameObject == startbutton)
		{
			StartGame();
			return;
		}
		

		int index = System.Array.IndexOf<GameObject>(slots, button.gameObject);

		//switch button
		slotsID[index]++;
		if (slotsID[index] == slotbuttons.Length)
			slotsID[index] = 0;

		button.SetImage(slotbuttons[slotsID[index]]);
		
		Debug.Log("received! " + slotsID[index].ToString());


	}



}
