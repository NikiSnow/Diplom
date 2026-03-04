using UnityEngine;

public class TileColliderFix : MonoBehaviour
{
    [SerializeField] Vector3 FixedUpPosition;
    [SerializeField] Vector3 FixedDownPosition;

    private bool Active = false; 

    public void PlayerGoesUp()
    {
        Active = true;
    }

    public void PlayerGoesDown()
    {
        Active = true;
    }

    public void TransitionEnded()
    {
        Active = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }
}
