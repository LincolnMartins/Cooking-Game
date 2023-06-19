using UnityEngine;
using System.Collections;

public class ProductMover : MonoBehaviour
{
	/* public variable.
	 * do not edit these vars.
	 * we get their values from other classes. */
	public int factoryID;				//actual ingredient ID to be served to customer
	public bool needsProcess;			//does this ingredient should be processed before delivering to customer?
	
	//only required for ingredients that needs process!
	private bool processFlag;			//will be set to true the first time this ingredient enters a processor
	private bool isProcessed;			//process has been finished
	private bool isOverburned;			//burger stayed for too long on the grill
	public Material[] beforeAfterMat;	//index[0] = raw    
										//index[1] = processed
										//index[2] = overburned

	//Private flags.
	private GameObject target;			//target object for this ingredient 
										//(deliveryPlate, a processor machine, etc...)
	private bool canGetDragged;			//are we allowed to drag this plate to customer?
	private GameObject serverPlate;		//server plate game object
	private GameObject grill;			//grill machine to process burgers
	private GameObject trashBin;		//reference to trashBin object
	private bool isFinished;			//are we done with positioning and processing the ingredient on the plate?
	private float minDeliveryDistance;	//Minimum distance to deliveryPlate required to land this ingredint on plate.
	private Vector3 tmpPos;				//temp variable for storing player input position on screen

	//player input variables
	private RaycastHit hitInfo;
	private Ray ray;

	//point fx
	public GameObject point3dText;  //3d text mesh

	// Start is called before the first frame update
	void Start ()
	{
		canGetDragged = true;
		minDeliveryDistance = 1.0f;
		isFinished = false;			//!Important : we use this flag to prevent ingredients to be draggable after placed on the plate.
		isProcessed = false;
		isOverburned = false;
		processFlag = false;
		
		//find possible targets
		serverPlate = GameObject.FindGameObjectWithTag("serverPlate");
		grill = GameObject.FindGameObjectWithTag("grill");
		trashBin = GameObject.FindGameObjectWithTag("trashbin");

		if(needsProcess) target = grill;
		else target = serverPlate;

		//print (gameObject.name + " - " + target.name);
	}

	// Update is called once per frame
	void Update ()
	{
		//if dragged
		if(Input.GetMouseButton(0) && canGetDragged)
			followInputPosition();
		
		//if released and doesn't need process
		if( (!Input.GetMouseButton(0) && Input.touches.Length < 1) && !isFinished && !needsProcess)
		{
			canGetDragged = false;
			checkCorrectPlacement();
		}

		//if released and needs process
		if( (!Input.GetMouseButton(0) && Input.touches.Length < 1) && !isFinished && needsProcess)
		{
			canGetDragged = false;
			checkCorrectPlacementOnProcessor();
		}

		//if needs process and process is finished successfully
		if(needsProcess && isProcessed && !isOverburned && !GameController.deliveryQueueIsFull && !IngredientsController.itemIsInHand)
			manageSecondDrag();

		//if needs process and process took too long and burger is overburned and must be discarded
		if(needsProcess && isProcessed && isOverburned && !GameController.deliveryQueueIsFull && !IngredientsController.itemIsInHand)
			manageDiscard();

		//Optional - change target's color when this ingredient is near enough
		if(!processFlag || (isProcessed && target == serverPlate) )
			changeTargetsColor(target);

	}

	/// <summary>
	/// Let the player move the processed ingredient
	/// </summary>
	void manageDiscard()
	{
		if(Input.GetMouseButtonDown(0))
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		else
			return;
		
		if(Physics.Raycast(ray, out hitInfo))
		{
			GameObject objectHit = hitInfo.transform.gameObject;
			if(objectHit.tag == "overburnedIngredient" && objectHit.name == gameObject.name) {
				
				IngredientsController.itemIsInHand = true;	//we have an ingredient in hand. no room for other ingredients!
				target = trashBin;							//we can just deliver this ingredient to trashbin.
				
				StartCoroutine(discardIngredient());
			}
		}
	}

	/// <summary>
	/// Let the player move the processed ingredient to the delivery plate
	/// </summary>
	void manageSecondDrag()
	{
		if(Input.GetMouseButtonDown(0))
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		else
			return;
		
		if(Physics.Raycast(ray, out hitInfo)) {
			GameObject objectHit = hitInfo.transform.gameObject;
			if(objectHit.tag == "ingredient" && objectHit.name == gameObject.name) {

				IngredientsController.itemIsInHand = true;	//we have an ingredient in hand. no room for other ingredients!
				target = serverPlate;						//we can just deliver this ingredient to main plate. there is no other
															//destination for this processed ingredient

				StartCoroutine(followInputTimeIndependent());
			}
		}
	}

	/// <summary>
	/// change plate's color when dragged ingredients are near enough
	/// </summary>
	void changeTargetsColor(GameObject _target)
	{
		if(IngredientsController.itemIsInHand)
		{
			float myDistance;
			myDistance = Vector3.Distance(_target.transform.position, gameObject.transform.position);
			//print("myDistance: " + myDistance);

			if(myDistance < minDeliveryDistance)
				_target.GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.5f); //change target's color to let the player know this is the correct place to release the items.
			else
				_target.GetComponent<Renderer>().material.color = new Color(1, 1, 1); //change target's color back to normal
		}
	}

	/// <summary>
	/// Check if the ingredients are dragged into the deliveryPlate. Otherwise delete them.
	/// </summary>
	void checkCorrectPlacementOnProcessor()
	{
		//if there is already an item on the processor, destroy this new ingredient
		if(!target.GetComponent<GrillController>().isEmpty)
		{
			Destroy(gameObject);
			target.GetComponent<Renderer>().material.color = new Color(1, 1, 1);
			return;
		}
		
		//if this ingredient is close enough to it's processor machine, leave it there. otherwise drop and delete it.
		float distanceToProcessor = Vector3.Distance(target.transform.position, gameObject.transform.position);
		//print("distanceToProcessor: " + distanceToProcessor);
		
		if(distanceToProcessor < minDeliveryDistance)
		{
			//close enough to land on processor
			transform.parent = target.transform;
			transform.position = new Vector3(target.transform.position.x,
			                                 target.transform.position.y + 0.75f,
			                                 target.transform.position.z - 0.1f);

			//change deliveryPlate's color back to normal
			target.GetComponent<Renderer>().material.color = new Color(1, 1, 1);

			//start processing the raw ingredient
			StartCoroutine(processRawIngredient());

			//we no longer need this ingredient's script (ProductMover class)
			//GetComponent<ProductMover>().enabled = false;

		}
		else Destroy(gameObject);
		
		//Not draggable anymore.
		isFinished = true;
	}

	/// <summary>
	/// Process raw ingredinet and transform it to a usable ingredient
	/// </summary>
	IEnumerator processRawIngredient()
	{
		processFlag = true; //should always remain true!
		isProcessed = false;
		isOverburned = false;

		float processTime = GrillController.grillTimer;
		float keepWarmTime = GrillController.grillKeepWarmTimer;

		target.GetComponent<GrillController>().isOn = true;
		target.GetComponent<GrillController>().isEmpty = false;
		target.GetComponent<GrillController>().playSfx(target.GetComponent<GrillController>().frySfx);

		float t = 0.0f;
		while(t < 1)
		{
			t += Time.deltaTime * (1/processTime);
			//print (gameObject.name + " is getting processed! - Time to wait: " + t);
			yield return 0;
		}

		if(t >= 1)
		{
			isProcessed = true;
			isOverburned = false;

			target.GetComponent<GrillController>().isWarm = true;	//grill has entered the state to keep the burger warm.
			target.GetComponent<GrillController>().playSfx(target.GetComponent<GrillController>().readySfx);

			gameObject.tag = "ingredient";
			gameObject.name = gameObject.name.Substring(0, gameObject.name.Length - 4);
			//target.GetComponent<GrillController>().isOn = false;
			GetComponent<Renderer>().material = beforeAfterMat[1];

			//change the target 
			target = serverPlate;

			//Now check if we pick the fried burger on time, otherwise this burger will get overburned and should be discarded.
			float v = 0.0f;
			while(v < 1)
			{
				if(!IngredientsController.itemIsInHand)
					v += Time.deltaTime * (1/keepWarmTime);
				//print ("Time to OverBurn: " + (1-v) );
				yield return 0;
			}

			//This burger is overburned! so it should be discarded in trashbin.
			if(v >= 1 && gameObject.tag != "deliveryQueueItem" && !IngredientsController.itemIsInHand)
			{
				isProcessed = true;
				isOverburned = true;

				GetComponent<Renderer>().material = beforeAfterMat[2];	//change the material to overburned burger

				grill.GetComponent<GrillController>().playSfx(grill.GetComponent<GrillController>().overburnSfx);	//play fail sfx
				grill.GetComponent<GrillController>().isEmpty = false;
				grill.GetComponent<GrillController>().isWarm = false;
				grill.GetComponent<GrillController>().isOn = true;
				grill.GetComponent<GrillController>().isOverburned = true;

				gameObject.tag = "overburnedIngredient";
				target = trashBin;
			}

		}

	}

	/// <summary>
	/// Check if the ingredients are dragged into the deliveryPlate. Otherwise delete them.
	/// </summary>
	void checkCorrectPlacement()
	{
		//if this ingredient is close enough to serving plate, we can add it to main queue. otherwise drop and delete it.
		float distanceToPlate = Vector3.Distance(serverPlate.transform.position, gameObject.transform.position);
		//print("distanceToPlate: " + distanceToPlate);
		
		if(distanceToPlate < minDeliveryDistance)
		{
			//close enough to land on plate
			transform.parent = serverPlate.transform;
			transform.position = new Vector3(serverPlate.transform.position.x,
			                                 serverPlate.transform.position.y + (0.35f * GameController.deliveryQueueItems + 0.1f),
				                             serverPlate.transform.position.z - (0.2f * GameController.deliveryQueueItems + 0.1f));
			
			GameController.deliveryQueueItems++;
			
			//change deliveryPlate's color back to normal
			serverPlate.GetComponent<Renderer>().material.color = new Color(1, 1, 1);
			
			//we no longer need this ingredient's script (ProductMover class)
			GetComponent<ProductMover>().enabled = false;
		}
		else Destroy(gameObject);
		
		//Not draggable anymore.
		isFinished = true;
	}

	/// <summary>
	/// Follow players mouse position on screen.
	/// </summary>
	void followInputPosition()
	{
		Vector3 _Pos;
		_Pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		//Custom offset. these objects should be in front of every other GUI instances.
		_Pos = new Vector3(_Pos.x, _Pos.y, -0.5f);
		//follow player's pointer
		transform.position = _Pos + new Vector3(0, 0, 0);
	}

	/// <summary>
	/// Follow players mouse position on screen.
	/// This is an IEnumerator and run independent of game main cycle
	/// </summary>
	IEnumerator followInputTimeIndependent()
	{
		while(IngredientsController.itemIsInHand || target == serverPlate)
		{
			tmpPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			tmpPos = new Vector3(tmpPos.x, tmpPos.y, -0.5f);
			transform.position = tmpPos + new Vector3(0, 0, 0);

			//if user release the input, check if we delivered the processed ingredient to the plate or we just release it nowhere!
			if(Input.touches.Length < 1 && !Input.GetMouseButton(0))
			{
				//if we delivered it to the plate
				if(Vector3.Distance(target.transform.position, gameObject.transform.position) <= minDeliveryDistance)
				{
					//print ("Landed on Plate in: " + Time.time);

					grill.GetComponent<GrillController>().isEmpty = true;
					grill.GetComponent<GrillController>().isWarm = false;
					grill.GetComponent<GrillController>().isOn = false;

					gameObject.tag = "deliveryQueueItem";
					gameObject.GetComponent<MeshCollider>().enabled = false;
					transform.position = new Vector3(serverPlate.transform.position.x,
					                                 serverPlate.transform.position.y + (0.35f * GameController.deliveryQueueItems + 0.1f),
					                                 serverPlate.transform.position.z - (0.2f * GameController.deliveryQueueItems + 0.1f));
					
					transform.parent = serverPlate.transform;
					GameController.deliveryQueueItems++;
					//change deliveryPlate's color back to normal
					serverPlate.GetComponent<Renderer>().material.color = new Color(1, 1, 1);
					//we no longer need this ingredient's script (ProductMover class)
					GetComponent<ProductMover>().enabled = false;
					yield break;

				}
				else //if we released it nowhere
				{					
					//print ("Reset Position");
					target = grill;	
					transform.parent = target.transform;
					transform.position = new Vector3(target.transform.position.x,
					                                 target.transform.position.y + 0.75f,
					                                 target.transform.position.z - 0.1f);
				}
			}

			yield return 0;
		}
	}

	/// <summary>
	/// Follow players mouse position on screen.
	/// This is an IEnumerator and run independent of game main cycle
	/// </summary>
	IEnumerator discardIngredient()
	{
		while(IngredientsController.itemIsInHand || target == trashBin)
		{			
			tmpPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			tmpPos = new Vector3(tmpPos.x, tmpPos.y, -0.5f);
			transform.position = tmpPos + new Vector3(0, 0, 0);

			//update trashbin door state
			float tmpDistanceToTrashbin = Vector3.Distance(target.transform.position, gameObject.transform.position);
			if(tmpDistanceToTrashbin <= minDeliveryDistance) 
				trashBin.GetComponent<TrashBinController>().updateDoorState(1);
			else
				trashBin.GetComponent<TrashBinController>().updateDoorState(0);

			//if user release the input, check if we delivered the processed ingredient to the trashbin or we just release it nowhere!
			if(Input.touches.Length < 1 && !Input.GetMouseButton(0))
			{
				//if we delivered it to the trashbin
				if(tmpDistanceToTrashbin <= minDeliveryDistance)
				{
					trashBin.GetComponent<TrashBinController>().playSfx(trashBin.GetComponent<TrashBinController>().deleteSfx);	//play trash sfx

					//trash loss
					GameController.gamePoints -= GameController.globalTrashLoss;
					GameObject money3d = Instantiate(point3dText, trashBin.transform.position + new Vector3(0, 0, -0.8f), Quaternion.Euler(0, 0, 0));
					money3d.GetComponent<TextMeshController>().myText = "-" + GameController.globalTrashLoss.ToString();
					
					grill.GetComponent<GrillController>().isEmpty = true;
					grill.GetComponent<GrillController>().isWarm = false;
					grill.GetComponent<GrillController>().isOn = false;
					grill.GetComponent<GrillController>().isOverburned = false;
	
					Destroy(gameObject);
					trashBin.GetComponent<TrashBinController>().updateDoorState(0);
					yield break;
					
				}
				else //if we released it nowhere
				{
					//print ("Reset Position");
					target = grill;	
					transform.parent = target.transform;
					transform.position = new Vector3(target.transform.position.x,
					                                 target.transform.position.y + 0.75f,
					                                 target.transform.position.z - 0.1f);
				}
			}
			
			yield return 0;
		}
	}

}