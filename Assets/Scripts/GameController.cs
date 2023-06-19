using UnityEngine;

public class GameController : MonoBehaviour
{
	private int availableTime; //Seconds

	//static variables.
	static public int gameTime;
	static public int gamePoints;
	static public bool gameIsFinished;										//Flag
	static public int globalTrashLoss = 1;									//when player trashes a product or ingredient, we deduct a fixed loss from player balance.
	static public bool deliveryQueueIsFull;									//delivery queue can accept 3 ingredients. more is not acceptable.
	static public int deliveryQueueItems;									//number of items in delivery queue

	//game timer vars
	private int seconds;
	private int minutes;
	
	// Text Objects
	public GameObject missionText;
	public GameObject timeText;

	//AudioClips
	public AudioClip timeEndSfx;

	public GameObject endGamePlane;
	public GameObject mainCamera;


	public void Awake()
	{
		deliveryQueueIsFull = false;
		deliveryQueueItems = 0;

		gameIsFinished = false;

		seconds = 0;
		minutes = 2;
		availableTime = 120;

	}

	// Update is called once per frame
	void Update()
	{
		//no more ingredient can be picked
		if (deliveryQueueItems >= 3)
			deliveryQueueIsFull = true;
		else
			deliveryQueueIsFull = false;

		if (!gameIsFinished)
		{
			manageClock();
			manageGuiTexts();
		}
	}

	void manageGuiTexts()
	{
		missionText.GetComponent<TextMesh>().text = gamePoints.ToString();
	}

	void manageClock()
	{
		if (gameIsFinished)
			return;

		gameTime = (int)(availableTime - Time.timeSinceLevelLoad);
		seconds = Mathf.CeilToInt(availableTime - Time.timeSinceLevelLoad) % 60;
		minutes = Mathf.CeilToInt(availableTime - Time.timeSinceLevelLoad) / 60;
		var remainingTime = string.Format("{0:00} : {1:00}", minutes, seconds);
		timeText.GetComponent<TextMesh>().text = remainingTime.ToString();

		if(seconds < 1 && minutes < 1)
		{
			mainCamera.GetComponent<AudioSource>().Stop();
			playSfx(timeEndSfx);
			endGamePlane.SetActive(true);
			endGamePlane.transform.Find("ScoreText").GetComponent<TextMesh>().text = "Score: " + gamePoints.ToString();
			gameIsFinished = true;
		}
	}

	void playSfx(AudioClip _sfx)
	{
		GetComponent<AudioSource>().PlayOneShot(_sfx);
	}
}