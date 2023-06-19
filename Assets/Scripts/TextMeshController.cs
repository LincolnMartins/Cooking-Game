using UnityEngine;
using System.Collections;

public class TextMeshController : MonoBehaviour
{
	void Start ()
	{
		//animate the 3d text.
		StartCoroutine(scaleUp());
	}

	// Simple animation routine based on scale.
	IEnumerator scaleUp ()
	{
		while(transform.localScale.x < 2)
		{
			transform.localScale = new Vector3(transform.localScale.x + 0.045f,
			                                   transform.localScale.y + 0.045f,
			                                   transform.localScale.z);
			transform.position = new Vector3(transform.position.x,
			                                 transform.position.y + 0.025f,
			                                 transform.position.z);
			yield return 0;
		}

		float t = 2;
		while(t > 0)
 		{
			t -= Time.deltaTime;	
			transform.position = new Vector3(transform.position.x,
			                                 transform.position.y + 0.01f,
			                                 transform.position.z);

			if(t <= 0)
				Destroy(gameObject);
			
			yield return 0;
		}
	}

}
