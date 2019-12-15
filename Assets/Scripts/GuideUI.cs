using UnityEngine;
using UnityEngine.UI;

public class GuideUI : MonoBehaviour
{
    public Text step1;
    public Text step2;
    public Text step3;
    public Button rightButton;
    public Button leftButton;
    public Button skipButton;
    public Button takePicButton;
    public Button helpButton;
    public Canvas guideMask;
    public Canvas guide;
    private int count;
    // Start is called before the first frame update

    public void RightClick()
    {
        if (count == 1)
        {
            step1.GetComponent<Text>().enabled = false;
            step2.GetComponent<Text>().enabled = true;
            count++;
            leftButton.enabled = true;
            leftButton.gameObject.SetActive(true);
        }
        else if (count == 2)
        {
            step2.GetComponent<Text>().enabled = false;
            step3.GetComponent<Text>().enabled = true;
            count++;
            rightButton.enabled = false;
            rightButton.gameObject.SetActive(false);
            skipButton.GetComponentInChildren<Text>().text = "Done";
        }
    }

    public void LeftClick()
    {
        if (count == 2)
        {
            step1.GetComponent<Text>().enabled = true;
            step2.GetComponent<Text>().enabled = false;
            count--;
            leftButton.enabled = false;
            leftButton.gameObject.SetActive(false);
        }
        else if (count == 3)
        {
            step2.GetComponent<Text>().enabled = true;
            step3.GetComponent<Text>().enabled = false;
            count--;
            rightButton.enabled = true;
            rightButton.gameObject.SetActive(true);
        }
    }

    public void SkipClick()
    {
        guideMask.enabled = false;
        guide.enabled = false;
    }

    public void HelpClick()
    {
        count = 1;
        leftButton.enabled = false;
        leftButton.gameObject.SetActive(false);
        rightButton.enabled = true;
        rightButton.gameObject.SetActive(true);
        step1.GetComponent<Text>().enabled = true;
        step2.GetComponent<Text>().enabled = false;
        step3.GetComponent<Text>().enabled = false;
        guideMask.enabled = true;
        guide.enabled = true;
    }

    public void TakePhotoClick()
    {

    }

    void Start()
    {
        count = 1;
        leftButton.enabled = false;
        leftButton.gameObject.SetActive(false);
        rightButton.enabled = true;
        rightButton.gameObject.SetActive(true);
        step1.GetComponent<Text>().enabled = true;
        step2.GetComponent<Text>().enabled = false;
        step3.GetComponent<Text>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
