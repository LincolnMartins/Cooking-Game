using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
	public static bool  soundEnabled;
	public static bool  isPaused;
	private float savedTimeScale;
	public GameObject pausePlane;

	enum Page
	{
		PLAY,
		PAUSE
	}
	private Page currentPage = Page.PLAY;

	void Awake ()
	{		
		soundEnabled = true;
		isPaused = false;
		
		Time.timeScale = 1.0f;
		
		if(pausePlane)
	    	pausePlane.SetActive(false); 
	}

	// Update is called once per frame
	void Update ()
	{
		buttonManager();
		
		//pause
		if(Input.GetKeyDown(KeyCode.P) || Input.GetKeyUp(KeyCode.Escape))
		{
			switch (currentPage)
			{
	            case Page.PLAY: 
					PauseGame(); 
					break;
	            case Page.PAUSE: 
					UnPauseGame(); 
					break;
	            default: 
					currentPage = Page.PLAY;
					break;
	        }
		}

		//restart
		if(Input.GetKeyDown(KeyCode.R))
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
	}

	void buttonManager ()
	{
		if(Input.GetMouseButtonUp(0))
		{
			RaycastHit hitInfo;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hitInfo))
			{
				string objectHitName = hitInfo.transform.gameObject.name;
				switch (objectHitName)
				{
					case "PauseBtn":
						{
							//pause is not allowed when game is finished
							if (GameController.gameIsFinished)
								return;

							switch (currentPage)
							{
								case Page.PLAY:
									PauseGame();
									break;
								case Page.PAUSE:
									UnPauseGame();
									break;
								default:
									currentPage = Page.PLAY;
									break;
							}
							break;
						}
					case "Btn-Resume":
					{
						switch (currentPage)
						{
							case Page.PLAY:
								PauseGame();
								break;
							case Page.PAUSE:
								UnPauseGame();
								break;
							default:
								currentPage = Page.PLAY;
								break;
						}
						break;
					}
					case "Btn-Restart":
					{
						UnPauseGame();
						SceneManager.LoadScene(SceneManager.GetActiveScene().name);
						break;
					}
					case "End-Restart":
					{
						SceneManager.LoadScene(SceneManager.GetActiveScene().name);
						break;
					}
				}
			}
		}
	}


	void PauseGame ()
	{
		//print("Game is Paused.");
		isPaused = true;
		savedTimeScale = Time.timeScale;
	    Time.timeScale = 0;
	    AudioListener.volume = 0;
	    if(pausePlane)
	    	pausePlane.SetActive(true);
	    currentPage = Page.PAUSE;
	}


	void UnPauseGame ()
	{
		//print("Unpause");
	    isPaused = false;
	    Time.timeScale = savedTimeScale;
	    AudioListener.volume = 1.0f;
		if(pausePlane)
	    	pausePlane.SetActive(false);   
	    currentPage = Page.PLAY;
	}

}