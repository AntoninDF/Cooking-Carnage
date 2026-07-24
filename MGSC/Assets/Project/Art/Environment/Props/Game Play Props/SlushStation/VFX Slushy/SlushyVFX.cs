using System.Collections;
using UnityEngine;

public class SlushyVFX : MonoBehaviour

{
    public GameObject particleLoop;
    public GameObject particleEnd;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       particleLoop.SetActive(false);
       particleEnd.SetActive(false);
       StartCoroutine (AnimationPlay());

    }

    private IEnumerator AnimationPlay()
    {
        particleLoop.SetActive(true);
        yield return new WaitForSeconds(3);
        particleLoop.SetActive(false);
        particleEnd.SetActive(true);       
       
    } 
}
