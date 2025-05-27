using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;

public class MoveObjectController : MonoBehaviour
{
	public float reachRange = 1.8f;
    public int objectNumber = 0;

    private Animator anim;
	private Camera fpsCam;
	private GameObject player;

	private bool playerEntered;

	private int rayLayerMask;

	[SerializeField] GameObject ShowObject;

    void Start()
	{
		//Initialize moveDrawController if script is enabled.
		player = GameObject.FindGameObjectWithTag("Player");

		fpsCam = Camera.main;

		//create AnimatorOverrideController to re-use animationController for sliding draws.
		anim = GetComponent<Animator>(); 

	}
		
	void OnTriggerEnter(Collider other)
	{		
		if (other.gameObject == player)		//player has collided with trigger
		{
			ShowObject.SetActive(true);
        }
	}

	void OnTriggerExit(Collider other)
	{		
		if (other.gameObject == player)		//player has exited trigger
		{
			ShowObject.SetActive(false);
		}
	}

	public void UnLock()
	{
		anim.SetTrigger("Open");
		GetComponent<Collider>().enabled = false;
		ShowObject.SetActive(false);
	}

    /*protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        string paramName = "isOpen_Obj_" + objectNumber;
        bool isOpen = anim.GetBool(paramName);
        anim.SetBool(paramName, !isOpen);
    }*/
}
