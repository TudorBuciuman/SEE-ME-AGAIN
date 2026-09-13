using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class IntroSequenceController : MonoBehaviour
{
    [SerializeField]
    public GameObject icon;
    public GameObject Canvas1;
    public GameObject Canvas2;

    public GameObject camera1;
    public GameObject camera2;

    //Thank you 
    public GameObject obj0;
    public GameObject obj1;
    public GameObject obj2;
    public GameObject obj3;
    public GameObject obj4;
    public GameObject objjj;

    public AudioSource audioSource;
    public AudioClip audioClip;
    public Material crtMaterial;

    //Warnings
    public GameObject warning1;
    public GameObject warning2;

    //Title Screen  
    public GameObject titleScreen;
    public GameObject author;

    void Start()
    {
        crtMaterial.SetFloat("_ShakeTrigger", 0.01f);
        StartCoroutine(RunThisTown());
    }
    public IEnumerator RunThisTown()
    {
        yield return new WaitForSeconds(2f);
        icon.SetActive(true);
        yield return new WaitForSeconds(5f);
        audioSource.clip = audioClip;
        audioSource.Play();
        yield return new WaitForSeconds(0.2f);
        icon.SetActive(false);
        yield return new WaitForSeconds(3.5f);
        yield return StartCoroutine(ThankYou());
        icon.SetActive(true);
        yield return new WaitForSeconds(2.2f);
        StartCoroutine(GlitchRoutine());
        yield return new WaitForSeconds(0.2f);
        icon.SetActive(false);
        Canvas1.SetActive(false);
        camera1.SetActive(false);
        camera2.SetActive(true);
        yield return StartCoroutine(WarningSequence());
        yield return StartCoroutine(TitleScreenSequence());
    }
    public IEnumerator ThankYou()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("Thank you for playing!");
        obj0.SetActive(true);
        yield return new WaitForSeconds(2f);
        objjj.SetActive(true);
        yield return new WaitForSeconds(1f);
        obj0.SetActive(false);
        objjj.SetActive(false);
        yield return new WaitForSeconds(0.05f);
        obj1.SetActive(true);
        yield return new WaitForSeconds(4f);
        obj1.SetActive(false);
        yield return new WaitForSeconds(0.05f);
        obj2.SetActive(true);
        yield return new WaitForSeconds(4f);
        obj2.SetActive(false);
        yield return new WaitForSeconds(0.05f);
        obj3.SetActive(true);
        yield return new WaitForSeconds(4f);
        obj3.SetActive(false);
        yield return new WaitForSeconds(0.05f);
        obj4.SetActive(true);
        yield return new WaitForSeconds(4f);
        obj4.SetActive(false);
        yield return new WaitForSeconds(2f);
        yield return null;
    }

    public IEnumerator WarningSequence()
    {
        yield return new WaitForSeconds(1f);
        warning1.SetActive(true);
        yield return new WaitForSeconds(3f);
        warning1.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        warning2.SetActive(true);
        yield return new WaitForSeconds(3f);
        warning2.SetActive(false);
        yield return new WaitForSeconds(0.5f);
    }
    public IEnumerator TitleScreenSequence()
    {
        yield return new WaitForSeconds(1f);
        titleScreen.SetActive(true);
        yield return new WaitForSeconds(3f);
        author.SetActive(true);
        yield return new WaitForSeconds(3f);
        titleScreen.SetActive(false);
        author.SetActive(false);
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator GlitchRoutine()
    {
        //crtMaterial.SetFloat("_ShakeTrigger", 1f);
        crtMaterial.SetFloat("_ShakeTrigger", 1.7f);

        yield return new WaitForSeconds(0.2f);

        crtMaterial.SetFloat("_ShakeTrigger", 0.01f);
    }
}
