using UnityEngine;

public class PivotAdjustmentForObject : MonoBehaviour
{
    [SerializeField] private Vector3 _pivotOffset;
    public void SetPosition(Vector3 newPosition)
    {
        transform.position = newPosition + _pivotOffset;
    }
}
