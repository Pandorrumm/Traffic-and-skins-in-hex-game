using UnityEngine;

public class Border : MonoBehaviour
{
    public Transform topLeftPos;
    public Transform bottomRightPos;
    public BoxCollider boxCollider;

    public bool isPositionInsideBorder(Vector3 position) {
        boxCollider.center = Vector3.Lerp(topLeftPos.position, bottomRightPos.position, 0.5f);
        boxCollider.size = new Vector3(Mathf.Abs(topLeftPos.position.x - bottomRightPos.position.x),
                                       Mathf.Abs(topLeftPos.position.y - bottomRightPos.position.y),
                                       Mathf.Abs(topLeftPos.position.z - bottomRightPos.position.z));

        return boxCollider.bounds.Contains(position);
    }

    public void OnDrawGizmos()
    {
        if (topLeftPos != null && bottomRightPos != null) {
            Gizmos.color = Color.red;

            Vector3 center = Vector3.Lerp(topLeftPos.position, bottomRightPos.position, 0.5f);
            Vector3 size = new Vector3(Mathf.Abs(topLeftPos.position.x - bottomRightPos.position.x),
                                       Mathf.Abs(topLeftPos.position.y - bottomRightPos.position.y),
                                       Mathf.Abs(topLeftPos.position.z - bottomRightPos.position.z));

            Gizmos.DrawWireCube(center, size);
        }
    }
}