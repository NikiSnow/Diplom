using UnityEngine;

public class Camerafollower : MonoBehaviour
{
    [SerializeField] GameObject Target;
    [SerializeField] Vector3 Offset;
    [SerializeField] bool IsUnder;
    [SerializeField] Vector3 Locker;
    Vector3 RightVector;
    void Update()
    {
        if (IsUnder && Target.transform.position.y + Offset.y < Locker.y)
        {
            RightVector = new Vector3(Target.transform.position.x + Offset.x, Locker.y, Target.transform.position.z + Offset.z);
        }
        else
        {
            RightVector = new Vector3(Target.transform.position.x + Offset.x, Target.transform.position.y + Offset.y, Target.transform.position.z + Offset.z);
        }
        this.gameObject.transform.position = RightVector;
    }
}
