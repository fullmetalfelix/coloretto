using UnityEngine;
using System.Collections;

public class LazyMouseHook : MonoBehaviour {


	void OnMouseUpAsButton()
	{

		transform.parent.SendMessage("OnMouseUpAsButton");

	}


}
