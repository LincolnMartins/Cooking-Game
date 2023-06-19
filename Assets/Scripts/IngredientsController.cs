using UnityEngine;
using System.Collections;

public class IngredientsController : MonoBehaviour
{
	public GameObject[] ingredientsArray;   //public list of all available ingredients.
	public int factoryID;                   //Public ID of this ingredient.
	public bool needsProcess = false;		//items that needs process, should first be moved to a machine to become ready to serve. normal items can be served directly and do not need to be processed.

	//Private flags
	private float delayTime;				//after this delay, we let player to be able to choose another ingredient again
	private bool  canCreate = true;			//cutome flag to prevent double picking

	//Static flag
	public static bool  itemIsInHand;		//can only pick and drag one ingredient eachtime.

	public AudioClip itemPick;

	void Awake ()
	{
		delayTime = 1.0f;
		itemIsInHand = false;
	}

	// Update is called once per frame
	void Update ()
	{
		managePlayerDrag();
		
		if(!Input.GetMouseButton(0))
			itemIsInHand = false;

		//debug
		//print ("itemIsInHand: " + itemIsInHand);
	}

	/// <summary>
	/// If player has dragged on of the ingredients, make an instance of it,
	/// then follow players mouse position.
	/// </summary>
	void managePlayerDrag ()
	{
		Ray ray;
		if(Input.GetMouseButtonDown(0))
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		else
			return;

		RaycastHit hitInfo;
		if (Physics.Raycast(ray, out hitInfo))
		{
			GameObject objectHit = hitInfo.transform.gameObject;
			if(objectHit.tag == "ingredient" && objectHit.name == gameObject.name && !itemIsInHand)
			{
				if(!needsProcess)
					createIngredient();
				else
					createRawIngredient();	//raw ingredient needs to be processed (in a machine) before use
			}
		}
	}

	/// <summary>
	/// Create an instance of this ingredient and make it a child of deliveryPlate.
	/// </summary>
	void createIngredient ()
	{
		if(canCreate && !GameController.gameIsFinished && !GameController.deliveryQueueIsFull)
		{
			canCreate = false;
			itemIsInHand = true;
			GameObject prod = Instantiate(ingredientsArray[factoryID], transform.position + new Vector3(0,0, -1), Quaternion.Euler(90, 180, 0));
			prod.name = ingredientsArray[factoryID].name;
			prod.tag = "deliveryQueueItem";
			prod.GetComponent<MeshCollider>().enabled = false;
			prod.GetComponent<ProductMover>().factoryID = factoryID;
			prod.GetComponent<ProductMover>().needsProcess = false;
			prod.transform.localScale = new Vector3(0.17f, 0.01f, 0.135f);
			playSfx(itemPick);
			StartCoroutine(reactivate());
		}
	}

	/// <summary>
	/// Create an instance of this ingredient and make it a child of deliveryPlate.
	/// </summary>
	void createRawIngredient ()
	{
		if(canCreate && !GameController.gameIsFinished)
		{
			canCreate = false;
			itemIsInHand = true;
			GameObject prod = Instantiate(ingredientsArray[factoryID], transform.position + new Vector3(0,0, -1), Quaternion.Euler(90, 180, 0));
			prod.name = ingredientsArray[factoryID].name + "-RAW";
			prod.tag = "rawIngredient";
			//prod.GetComponent<MeshCollider>().enabled = false;
			prod.GetComponent<ProductMover>().factoryID = factoryID;
			prod.GetComponent<ProductMover>().needsProcess = true;
			prod.transform.localScale = new Vector3(0.17f, 0.01f, 0.135f);
			playSfx(itemPick);
			StartCoroutine(reactivate());
		}
	}

	/// <summary>
	/// make this ingredient draggable again
	/// </summary>
	IEnumerator reactivate ()
	{
		yield return new WaitForSeconds(delayTime);
		canCreate = true;
	}

	/// <summary>
	/// Play AudioClips
	/// </summary>
	void playSfx (AudioClip _sfx)
	{
		GetComponent<AudioSource>().clip = _sfx;
		if(!GetComponent<AudioSource>().isPlaying)
			GetComponent<AudioSource>().Play();
	}

}