using UnityEngine;

namespace SlotX.CodeAnimation
{
    public class RotateLoop : MonoBehaviour
    {
        public float rotationSpeed = -1f;

        private void Update()
        {
            // 获取当前的欧拉角
            Vector3 currentEulerAngles = transform.localEulerAngles;

            // 增加旋转角度
            currentEulerAngles.z += rotationSpeed * Time.deltaTime;

            // 限制角度在0到360之间
            currentEulerAngles.z = Mathf.Repeat(currentEulerAngles.z, 360f);

            // 更新欧拉角
            transform.localEulerAngles = currentEulerAngles;
        }
    }
}
