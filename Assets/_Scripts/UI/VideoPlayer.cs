using UnityEngine;
using UnityEngine.Video;

namespace _Scripts.Visual_Elements
{
    public class VideoPlayer : MonoBehaviour
    {
        public UnityEngine.Video.VideoPlayer videoPlayer;

    
        public float PlayVideo(string videoFileName)
        {
            if (!videoPlayer) videoPlayer = GetComponent<UnityEngine.Video.VideoPlayer>();
            if (videoPlayer)
            {
                string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);
                videoPlayer.url = videoPath;
                videoPlayer.Play();
                //return the length of the clip
                return (float)videoPlayer.length;
            }

            //Did not find video player, return 0 (No time)
            return 0;
        }

        public void StopVideo()
        {
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }
        }

    
    }
}
