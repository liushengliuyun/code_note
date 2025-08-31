using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

namespace ProjectDawn.LocalAvoidance
{
    public class AgentLineDraw : MonoBehaviour
    {
        public float InnerRadius = 1;
        public float OuterRadius = 5;
        public Transform ObstacleStart;
        public Transform ObstacleEnd;

        void OnDrawGizmos()
        {
            using (var sonar = new SonarAvoidance(transform.position, quaternion.identity, InnerRadius, OuterRadius, 0, Allocator.Temp))
            {
                //sonar.InsertObstacle(new float3(-1, 0, 0), math.radians(180));
                if (ObstacleStart && ObstacleEnd)
                {
                    sonar.InsertObstacle(ObstacleStart.transform.position, ObstacleEnd.transform.position);
                    sonar.DrawObstacle(ObstacleStart.transform.position, ObstacleEnd.transform.position);
                }
                sonar.DrawSonar();
                sonar.DrawClosestDirection();
            }
        }
    }
}
