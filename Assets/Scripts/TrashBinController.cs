using UnityEngine;

public class TrashBinController : MonoBehaviour
{
	//AudioClip
	public AudioClip deleteSfx;

	//Textures for open/closed states
	public Texture2D[] state;

	//Flags
	private GameObject deliveryPlate;	
	public bool isCloseEnoughToTrashbin; //Flag used to let managers know that player is intended to send the order to trashbin.

	void Awake ()
	{
		GetComponent<Renderer>().material.mainTexture = state[0];
		deliveryPlate = GameObject.FindGameObjectWithTag("serverPlate");
		isCloseEnoughToTrashbin = false;
	}

	// Update is called once per frame
	void Update()
	{
		//check if player wants to move the order to trash bin
		checkDistanceToDelivery();
	}

	/// <summary>
	/// If player is dragging the deliveryPlate, check if maybe he wants to trash it.
	/// we do this by calculation the distance of deliveryPlate and trashBin.
	/// </summary>
	void checkDistanceToDelivery()
	{
		float myDistance;
		myDistance = Vector3.Distance(transform.position, deliveryPlate.transform.position);
		//print("distance to trashBin is: " + myDistance + ".");

		//2.0f is a hardcoded value. specify yours with caution.
		if (myDistance < 2.0f)
		{
			isCloseEnoughToTrashbin = true;
			//change texture
			GetComponent<Renderer>().material.mainTexture = state[1];
		}
		else
		{
			isCloseEnoughToTrashbin = false;
			//change texture
			GetComponent<Renderer>().material.mainTexture = state[0];
		}
	}

	/// <summary>
	/// Allow other controllers to update the animation state of this trashbin object
	/// by controlling it's door state.
	/// </summary>
	public void updateDoorState(int _state) 
	{
		if(_state == 1)
			GetComponent<Renderer>().material.mainTexture = state[1];
		else
			GetComponent<Renderer>().material.mainTexture = state[0];
	}

	/// <summary>
	/// Play AudioClips
	/// </summary>
	public void playSfx(AudioClip _sfx)
	{
		GetComponent<AudioSource>().clip = _sfx;
		if(!GetComponent<AudioSource>().isPlaying)
			GetComponent<AudioSource>().Play();
	}
}