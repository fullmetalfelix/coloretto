using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ScoreBoard : MonoBehaviour {



	public int[] currentpoints;
	public List<int[]> scores;


	public Rect area;


	public static ScoreBoard Fabricate(int[] players)
	{

		GameObject g = new GameObject("scores");
		ScoreBoard s = g.AddComponent<ScoreBoard>();


		return s;
	}


	void OnGUI()
	{

		GUI.BeginGroup(area);



		GUI.EndGroup();

	}



}
