using KnightRun.Player;
using UnityEngine;

namespace KnightRun.CameraSystem
{
    public class CameraFollow : MonoBehaviour
    {
        Transform target;
        Vector3 offset = new Vector3(0f, 4.5f, -8f);
        float followSmooth = 6f;

        public void SetTarget(Transform followTarget)
        {
            target = followTarget;
            if (target != null)
                transform.position = target.position + offset;
        }

        void LateUpdate()
        {
            if (target == null)
                return;

            Vector3 desired = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desired, followSmooth * Time.deltaTime);
            transform.LookAt(target.position + Vector3.up * 1.2f);
        }
    }
}
