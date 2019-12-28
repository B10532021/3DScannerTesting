using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Helper class that plays a video on a RawImage texture.
/// </summary>
[RequireComponent(typeof(RawImage))]
[RequireComponent(typeof(VideoPlayer))]

public class MyVideoPlayer : MonoBehaviour
{
    /// <summary>
    /// The raw image where the video will be played.
    /// </summary>
    public RawImage RawImage;

    /// <summary>
    /// The video player component to be played.
    /// </summary>
    public VideoPlayer VideoPlayer;

    private Texture m_RawImageTexture;

    /// <summary>
    /// The Unity Start() method.
    /// </summary>
    public void Start()
    {
        VideoPlayer.enabled = false;
        m_RawImageTexture = RawImage.texture;
        VideoPlayer.prepareCompleted += _PrepareCompleted;
    }

    /// <summary>
    /// The Unity Update() method.
    /// </summary>
    public void Update()
    {
        if (RawImage.enabled && !VideoPlayer.enabled)
        {
            VideoPlayer.enabled = true;
            VideoPlayer.Play();
        }
        else if (!RawImage.enabled && VideoPlayer.enabled)
        {
            // Stop video playback to save power usage.
            VideoPlayer.Stop();
            RawImage.texture = m_RawImageTexture;
            VideoPlayer.enabled = false;
        }
    }

    private void _PrepareCompleted(VideoPlayer player)
    {
        RawImage.texture = player.texture;
    }
}
