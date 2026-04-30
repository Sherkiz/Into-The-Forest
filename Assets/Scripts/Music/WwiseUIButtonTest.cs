using UnityEngine;

public class WwiseUIButtonTest : MonoBehaviour
{
    public AK.Wwise.Event eventToPost;

    public void PlayWwiseEvent()
    {
        Debug.Log("Generate button clicked: WwiseUIButtonTest.PlayWwiseEvent() was called.");

        if (eventToPost == null)
        {
            Debug.LogError("No Wwise event assigned to eventToPost.");
            return;
        }

        uint playingId = eventToPost.Post(gameObject);
        Debug.Log("Posted Wwise event. Playing ID: " + playingId);
    }
}