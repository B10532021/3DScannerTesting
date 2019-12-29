using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

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

    /// <summary>
    /// The raw image where the video will be played.
    /// </summary>
    public RawImage RawImage;
    public RawImage RawImage3;

    /// <summary>
    /// The video player component to be played.
    /// </summary>
    public VideoPlayer VideoPlayer;
    public VideoPlayer VideoPlayer3;

    private Texture m_RawImageTexture;
    private Texture m_RawImageTexture3;

    public void RightClick()
    {
        if (count == 1)
        {
            step1.GetComponent<Text>().enabled = false;
            step2.GetComponent<Text>().enabled = true;
            count++;
            leftButton.enabled = true;
            leftButton.gameObject.SetActive(true);
            
            VideoPlayer.Stop();
            RawImage.texture = m_RawImageTexture;
            VideoPlayer.enabled = false;
        }
        else if (count == 2)
        {
            step2.GetComponent<Text>().enabled = false;
            step3.GetComponent<Text>().enabled = true;
            count++;
            rightButton.enabled = false;
            rightButton.gameObject.SetActive(false);
            skipButton.GetComponentInChildren<Text>().text = "Done";

            VideoPlayer3.enabled = true;
            VideoPlayer3.Play();
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

            VideoPlayer.enabled = true;
            VideoPlayer.Play();
        }
        else if (count == 3)
        {
            step2.GetComponent<Text>().enabled = true;
            step3.GetComponent<Text>().enabled = false;
            count--;
            rightButton.enabled = true;
            rightButton.gameObject.SetActive(true);

            VideoPlayer3.Stop();
            RawImage3.texture = m_RawImageTexture3;
            VideoPlayer3.enabled = false;
        }
    }

    public void SkipClick()
    {
        VideoPlayer.Stop();
        VideoPlayer3.Stop();
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

        VideoPlayer.enabled = false;
        VideoPlayer3.enabled = false;
        m_RawImageTexture = RawImage.texture;
        m_RawImageTexture3 = RawImage3.texture;
        VideoPlayer.prepareCompleted += _PrepareCompleted;
        VideoPlayer3.prepareCompleted += _PrepareCompleted3;

        VideoPlayer.enabled = true;
        VideoPlayer.Play();
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

        VideoPlayer.enabled = false;
        VideoPlayer3.enabled = false;
        m_RawImageTexture = RawImage.texture;
        m_RawImageTexture3 = RawImage3.texture;
        VideoPlayer.prepareCompleted += _PrepareCompleted;
        VideoPlayer3.prepareCompleted += _PrepareCompleted3;

        VideoPlayer.enabled = true;
        VideoPlayer.Play();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void _PrepareCompleted(VideoPlayer player)
    {
        RawImage.texture = player.texture;
    }

    private void _PrepareCompleted3(VideoPlayer player)
    {
        RawImage3.texture = player.texture;
    }
}
