using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class IntroSequenceController : MonoBehaviour
{
    [SerializeField]
    public GameObject icon;
    public GameObject icon2;
    public GameObject Canvas1;
    public GameObject Canvas2;
    public GameObject Canvas3;

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

    public Image redSky;
    public GameObject SkyObj;
    void Start()
    {
        crtMaterial.SetFloat("_ShakeTrigger", 0.01f);
        StartCoroutine(RunThisTown());
    }
    public IEnumerator RunThisTown()
    {
        //yield return new WaitForSeconds(2f);
        //icon.SetActive(true);
        //yield return new WaitForSeconds(5f);
        //audioSource.clip = audioClip;
        //audioSource.Play();
        yield return StartCoroutine(ChangeColor());
        yield return StartCoroutine(SlideSky());
        yield return new WaitForSeconds(3.2f);
        icon.SetActive(false);
        yield return new WaitForSeconds(3.5f);
        yield return StartCoroutine(ThankYou());
        icon2.SetActive(true);
        yield return new WaitForSeconds(2.2f);
        StartCoroutine(GlitchRoutine());
        yield return new WaitForSeconds(0.2f);
        icon2.SetActive(false);
        Canvas1.SetActive(false);
        camera1.SetActive(false);
        camera2.SetActive(true);
        //yield return StartCoroutine(WarningSequence());
        yield return StartCoroutine(TitleScreenSequence());
    }
    public IEnumerator ChangeColor()
    {
        float elapsedTime = 0f;
        float duration = 4f;
        Color c = redSky.color;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            redSky.color = new Color(c.r,c.g,c.b,Mathf.Lerp(0,179/256f,t));
            yield return null;
        }

        redSky.color = new Color(c.r,c.g,c.b,179/256f);
        yield return new WaitForSeconds(0.5f);
    }
    public IEnumerator SlideSky()
    {
        float elapsedTime = 0f;
        float duration = 3f; 
        Vector2 startPosition = SkyObj.transform.position;
        Vector2 endPosition = new Vector2(startPosition.x, startPosition.y + 1000f); 
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            SkyObj.transform.position = Vector2.Lerp(startPosition, endPosition, t);
            yield return null;
        }
        SkyObj.transform.position = endPosition;
        yield return null;
    }
    public IEnumerator ThankYou()
    {
        Canvas2.SetActive(true);
        Canvas1.SetActive(true);
        camera1.SetActive(true);
        camera2.SetActive(false);
        yield return new WaitForSeconds(1f);
        Debug.Log("To my friends..");
        obj0.SetActive(true);
        yield return new WaitForSeconds(2f);
        objjj.SetActive(true);
        yield return new WaitForSeconds(1f);
        obj0.SetActive(false);
        objjj.SetActive(false);
        yield return new WaitForSeconds(1f);
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
        Canvas2.SetActive(false);
        Canvas1.SetActive(false);
        camera1.SetActive(false);
        camera2.SetActive(true);
        yield return new WaitForSeconds(3f);
        Debug.Log("To the haters.. Congratulation!");
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
        Canvas3.SetActive(true);
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
