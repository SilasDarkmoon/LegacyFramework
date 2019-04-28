using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public string[] PlayList = null;
	// Use this for initialization
	void Start () {
	    if (PlayList != null)
	    {
	        MusicManager.Play(PlayList);
	    }
	}

}
