using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateController : MonoBehaviour
{
	//Private flags
	private Vector3 initialPosition;
	private GameObject trashbin;

	public MissionController missionController;

	//AudioClip
	public AudioClip trashSfx;
	public AudioClip pointSfx;

	//points fx
	public GameObject points3dText;  //3d text mesh

	void Awake()
	{
		initialPosition = transform.position;
		trashbin = GameObject.FindGameObjectWithTag("trashbin");
	}

	// Update is called once per frame
	void Update()
	{
		if (GameController.deliveryQueueIsFull)
		{
			//get materials from all ingredients on plate
			var deliveryid = new List<int>();
			for (int i = 0; i < transform.childCount; i++)
				if (transform.GetChild(i).tag == "deliveryQueueItem")
					deliveryid.Add(transform.GetChild(i).gameObject.GetComponent<ProductMover>().factoryID);

			//check if ingredients on plate are same of mission
			for (int i = 0; i < missionController.productIngredients.Length; i++)
			{
				//not same
				if (missionController.productIngredients[i] != deliveryid[i])
				{
					GameController.gamePoints -= 3;
					GameObject points = Instantiate(points3dText,
														gameObject.transform.position + new Vector3(0, 0, -0.8f),
														Quaternion.Euler(0, 0, 0));
					points.GetComponent<TextMeshController>().myText = "-3";
					playSfx(trashSfx);
					RemoveIngredients();
					missionController.RandomizeMission();
					return;
				}
			}

			//ingredients are same
			GameController.gamePoints += 5;
			GameObject points3d = Instantiate(points3dText,
												gameObject.transform.position + new Vector3(0, 0, -0.8f),
												Quaternion.Euler(0, 0, 0));
			points3d.GetComponent<TextMeshController>().myText = "+5";
			playSfx(pointSfx);
			RemoveIngredients();
			missionController.RandomizeMission();
		}

		manageDeliveryDrag();
	}

	/// <summary>
	/// If we are starting our drag on deliveryPlate, move the plate with our mouse.
	/// </summary>
	void manageDeliveryDrag()
	{
		Ray ray;
		if (Input.GetMouseButtonDown(0))
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		else
			return;

		RaycastHit hitInfo;
		if (Physics.Raycast(ray, out hitInfo))
		{
			GameObject objectHit = hitInfo.transform.gameObject;
			if (objectHit.tag == "serverPlate" && objectHit.name == gameObject.name && !IngredientsController.itemIsInHand)
			{
				StartCoroutine(createDeliveryPackage());
			}
		}
	}

	/// <summary>
	/// Move the plate
	/// </summary>
	IEnumerator createDeliveryPackage()
	{
		while (GameController.deliveryQueueItems > 0)
		{
			Vector3 _Pos;
			//follow mouse
			_Pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			_Pos = new Vector3(_Pos.x, _Pos.y, -0.5f);
			//follow player's pointer
			transform.position = _Pos + new Vector3(0, 0, 0);
			//better to be transparent, when dragged
			GetComponent<Renderer>().material.color = new Color(GetComponent<Renderer>().material.color.r,
												GetComponent<Renderer>().material.color.g,
												GetComponent<Renderer>().material.color.b,
												0.5f);

			if (!Input.GetMouseButton(0))
			{
				resetPosition();
				break;
			}

			yield return 0;
		}
	}

	/// <summary>
	/// Move the plate to it's initial position.
	/// we also check if user wants to trash his delivery, before any other process.
	/// </summary>
	void resetPosition()
	{
		//just incase user wants to move this to trashbin, check it here first
		if (trashbin.GetComponent<TrashBinController>().isCloseEnoughToTrashbin)
		{
			//empty plate contents
			playSfx(trashSfx);

			//trash loss
			GameController.gamePoints -= GameController.globalTrashLoss;
			GameObject points3d = Instantiate(points3dText,
												trashbin.transform.position + new Vector3(0, 0, -0.8f),
												Quaternion.Euler(0, 0, 0));
			points3d.GetComponent<TextMeshController>().myText = "-" + (GameController.globalTrashLoss * GameController.deliveryQueueItems).ToString();

			RemoveIngredients();
		}

		//take the plate back to it's initial position
		//print("Back to where we belong");
		transform.position = initialPosition;
		GetComponent<Renderer>().material.color = new Color(GetComponent<Renderer>().material.color.r,
											GetComponent<Renderer>().material.color.g,
											GetComponent<Renderer>().material.color.b,
											1);
	}

	void RemoveIngredients()
	{
		//reset main queue
		GameController.deliveryQueueItems = 0;
		GameController.deliveryQueueIsFull = false;

		//destroy the contents of the serving plate.
		GameObject[] DeliveryQueueItems = GameObject.FindGameObjectsWithTag("deliveryQueueItem");
		foreach (GameObject item in DeliveryQueueItems)
			Destroy(item);
	}

	/// <summary>
	/// Play AudioClips
	/// </summary>
	void playSfx(AudioClip _sfx)
	{
		GetComponent<AudioSource>().clip = _sfx;
		if (!GetComponent<AudioSource>().isPlaying)
			GetComponent<AudioSource>().Play();
	}

}