// @ 2022 Lukas Chodosevicius

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectDawn.LocalAvoidance
{
    /// <summary>
    /// This structure can be used for constructing local objet avoidance structure and finding the closest direction for it.
    /// It is composed of three main methods: Constructor, InsertObstacle and FindClosestDirection.
    /// </summary>
    [DebuggerDisplay("IsCreated = {IsCreated}")]
    [BurstCompile]
    public struct SonarAvoidance : IDisposable
    {
        const float s_Angle = math.PI;

        float3 m_Position;
        quaternion m_Rotation;
        float m_InnerRadius;
        float m_OuterRadius;
        float m_Speed;
        NativeList<SonarNode> m_Nodes;

        /// <summary>
        /// Root node. Always starts at zero handle.
        /// </summary>
        SonarNodeHandle Root => new SonarNodeHandle();

        /// <summary>
        /// Position of sonar.
        /// </summary>
        public float3 Position => m_Position;

        /// <summary>
        /// Rotation of sonar.
        /// </summary>
        public quaternion Rotation => m_Rotation;

        /// <summary>
        /// Inner radius of sonar.
        /// </summary>
        public float InnerRadius => m_InnerRadius;

        /// <summary>
        /// Outer radius of sonar.
        /// </summary>
        public float OuterRadius => m_OuterRadius;

        /// <summary>
        /// Speed of sonar.
        /// </summary>
        public float Speed => m_Speed;

        /// <summary>
        /// True if structure is created.
        /// </summary>
        public bool IsCreated => m_Nodes.IsCreated;

        /// <summary>
        /// Constructs copy of sonar avoidance. No memory is shared between copy and original.
        /// </summary>
        /// <param name="other">Copies from</param>
        /// <param name="allocator">Allocator type</param>
        public SonarAvoidance(in SonarAvoidance other, Allocator allocator = Allocator.Temp)
        {
            other.CheckIsCreated();

            m_Position = other.m_Position;
            m_Rotation = other.m_Rotation;
            m_InnerRadius = other.m_InnerRadius;
            m_OuterRadius = other.m_OuterRadius;
            m_Speed = other.m_Speed;

            // Make a copy of nodes
            // Uses unsafe context for faster copy API
            unsafe
            {
                var length = other.m_Nodes.Length;
                m_Nodes = new NativeList<SonarNode>(length, allocator);
                m_Nodes.ResizeUninitialized(length);
                UnsafeUtility.MemCpy(m_Nodes.GetUnsafePtr(), other.m_Nodes.GetUnsafePtr(), sizeof(SonarNode) * length);
            }
        }

        /// <summary>
        /// Constructs sonar avoidance using position, direction and radius.
        /// </summary>
        /// <param name="position">Position of sonar</param>
        /// <param name="direction">Direction of sonar. Note this is forward direction on x axis, not z like LookRotation uses</param>
        /// <param name="up">Up direction</param>
        /// <param name="innerRadius">Minimum radius from which sonar will tracks obstacles and also used for path size</param>
        /// <param name="outerRadius">Maximum radius from which sonar will tracks obstacles</param>
        /// <param name="speed">Speed of sonar</param>
        /// <param name="allocator">Allocator type</param>
        public SonarAvoidance(float3 position, float3 direction, float3 up, float innerRadius, float outerRadius, float speed, Allocator allocator = Allocator.Temp)
            : this(position, SonarAvoidance.DirectionToRotation(direction, up), innerRadius, outerRadius, speed, allocator) {}

        /// <summary>
        /// Constructs sonar avoidance using position, direction and radius.
        /// </summary>
        /// <param name="position">Position of sonar</param>
        /// <param name="rotation">Rotation of sonar</param>
        /// <param name="innerRadius">Minimum radius from which sonar will tracks obstacles and also used for path size</param>
        /// <param name="outerRadius">Maximum radius from which sonar will tracks obstacles</param>
        /// <param name="speed">Speed of sonar</param>
        /// <param name="allocator">Allocator type</param>
        public SonarAvoidance(float3 position, quaternion rotation, float innerRadius, float outerRadius, float speed, Allocator allocator = Allocator.Temp)
        {
            CheckArguments(rotation, innerRadius, outerRadius);

            m_Position = position;
            m_Rotation = rotation;
            m_InnerRadius = innerRadius;
            m_OuterRadius = outerRadius;
            m_Speed = speed;
            m_Nodes = new NativeList<SonarNode>(16, allocator);

            Clear();
        }

        /// <summary>
        /// Constructs sonar avoidance without properties. After this constructor you must call <see cref="SonarAvoidance.Set"/> at least once to setup correct properties.
        /// </summary>
        /// <param name="allocator">Allocator type</param>
        public SonarAvoidance(Allocator allocator)
        {
            m_Position = 0;
            m_Rotation = quaternion.identity;
            m_InnerRadius = 0;
            m_OuterRadius = 0;
            m_Speed = 0;
            m_Nodes = new NativeList<SonarNode>(16, allocator);
        }

        /// <summary>
        /// Reconstructs sonar avoidance using position, direction and radius.
        /// </summary>
        /// <param name="position">Position of sonar</param>
        /// <param name="direction">Direction of sonar. Note this is forward direction on x axis, not z like LookRotation uses</param>
        /// <param name="up">Up direction</param>
        /// <param name="innerRadius">Minimum radius from which sonar will tracks obstacles and also used for path size</param>
        /// <param name="outerRadius">Maximum radius from which sonar will tracks obstacles</param>
        /// <param name="speed">Speed of sonar</param>
        /// <param name="allocator">Allocator type</param>
        public void Set(float3 position, float3 direction, float3 up, float innerRadius, float outerRadius, float speed)
        {
            Set(position, SonarAvoidance.DirectionToRotation(direction, up), innerRadius, outerRadius, speed);
        }

        /// <summary>
        /// Reconstructs sonar avoidance using position, direction and radius.
        /// </summary>
        /// <param name="position">Position of sonar</param>
        /// <param name="rotation">Rotation of sonar</param>
        /// <param name="innerRadius">Minimum radius from which sonar will tracks obstacles and also used for path size</param>
        /// <param name="outerRadius">Maximum radius from which sonar will tracks obstacles</param>
        /// <param name="speed">Speed of sonar</param>
        public void Set(float3 position, quaternion rotation, float innerRadius, float outerRadius, float speed)
        {
            CheckIsCreated();
            CheckArguments(rotation, innerRadius, outerRadius);

            m_Position = position;
            m_Rotation = rotation;
            m_InnerRadius = innerRadius;
            m_OuterRadius = outerRadius;
            m_Speed = speed;

            Clear();
        }

        /// <summary>
        /// Remove all inserted obstacles.
        /// </summary>
        public void Clear()
        {
            CheckIsCreated();

            m_Nodes.Clear();

            // Create Root node with childs left and right
            CreateNode(new Line(-s_Angle, s_Angle));
            var left = CreateNode(new Line(-s_Angle, 0));
            var right = CreateNode(new Line(0, s_Angle));
            m_Nodes[Root] = new SonarNode
            {
                Line = new Line(-s_Angle, s_Angle),
                Left = left,
                Right = right,
            };
        }

        /// <summary>
        /// Inserts radius obstacle into sonar.
        /// </summary>
        /// <param name="direction">Direction of obstacle from sonar</param>
        /// <param name="radius">Radius of obstacle</param>
        /// <returns> True if obstacle was added successfully</returns>
        public bool InsertObstacle(float3 direction, float radius)
        {
            CheckIsCreated();

            var directionLS = ToLocalSpace(direction);
            var angle = DirectionLSToAngle(directionLS);

            var radiusHalf = radius * 0.5f;
            var angleRight = angle - radiusHalf;
            var angleLeft = angle + radiusHalf;

            InsertObstacle(new Line(angleRight, angleLeft));
            return true;
        }

        /// <summary>
        /// Inserts sphere obstacle into sonar.
        /// </summary>
        /// <param name="obstaclePosition">Position of obstacle</param>
        /// <param name="obstacleVelocity">Velocity of obstacle (Zero can be used for non moving obstacle)</param>
        /// <param name="obstacleRadius">Radius of obstacle</param>
        /// <returns> True if obstacle was added successfully</returns>
        public bool InsertObstacle(float3 obstaclePosition, float3 obstacleVelocity, float obstacleRadius)
        {
            CheckIsCreated();

            var obstaclePositionLS = ToLocalSpace(obstaclePosition - m_Position);
            var obstacleVelocityLS = ToLocalSpace(obstacleVelocity);

            Circle current = new Circle(0, m_InnerRadius);
            Circle obstacle = new Circle(obstaclePositionLS, obstacleRadius);
            if (Intersection.IntersectionOfTwoMovingCircles(obstacle, obstacleVelocityLS, current, m_Speed, m_OuterRadius + m_InnerRadius, out Line angles, out Line times))
            {
                InsertObstacle(angles);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Inserts line obstacle into sonar.
        /// </summary>
        /// <param name="obstacleStartPosition">Start position of obstacle</param>
        /// <param name="obstacleEndPosition">End position of obstacle</param>
        /// <returns> True if obstacle was added successfully</returns>
        public bool InsertObstacle(float3 obstacleStartPosition, float3 obstacleEndPosition)
        {
            CheckIsCreated();

            var startLS = ToLocalSpace(obstacleStartPosition - m_Position);
            var endLS = ToLocalSpace(obstacleEndPosition - m_Position);

            // Find segment that intersects with the sonar
            if (!Intersection.IntersectionOfCircleAndSegment(new Circle(0, m_OuterRadius), startLS, endLS, out startLS, out endLS))
                return false;

            float startAngle = DirectionLSToAngle(startLS);
            float diffAngle = SignedAngle(startLS, endLS);
            float endAngle = startAngle + diffAngle;

            float fromReal = Circle.TangentLineToCircleAngle(InnerRadius, math.length(startLS));
            float toReal = Circle.TangentLineToCircleAngle(InnerRadius, math.length(endLS));

            float min = math.min(endAngle - toReal, startAngle - fromReal);
            float max = math.max(startAngle + fromReal, endAngle + toReal);

            InsertObstacle(new Line(min, max));

            return true;
        }

        /// <summary>
        /// Finds closest desired direction that is not obstructed by obstacle.
        /// </summary>
        /// <param name="direction">Closest direction found</param>
        /// <returns> True if direction was found</returns>
        public bool FindClosestDirection(out float3 direction)
        {
            CheckIsCreated();

            // Find closest angle in left side nodes
            var successLeft = FindClosestAngleLeft(out float angleLeft);

            // Find closest angle in right side nodes
            var successRight = FindClosestAngleRight(out float angleRight);

            if (successLeft && successRight)
            {
                if (math.abs(angleLeft) < math.abs(angleRight))
                {
                    direction = ToWorldSpace(AngleToDirectionLS(angleLeft));
                    return true;
                }
                else
                {
                    direction = ToWorldSpace(AngleToDirectionLS(angleRight));
                    return true;
                }
            }
            else if (successLeft)
            {
                direction = ToWorldSpace(AngleToDirectionLS(angleLeft));
                return true;
            }
            else if (successRight)
            {
                direction = ToWorldSpace(AngleToDirectionLS(angleRight));
                return true;
            }

            direction = float3.zero;
            return false;
        }

        /// <summary>
        /// Dispose implementation.
        /// </summary>
        public void Dispose()
        {
            m_Nodes.Dispose();
        }

        /// <summary>
        /// This is almost same as Quaternion.LookRotation just uses forward as x axis instead of z.
        /// </summary>
        /// <param name="forward">Forward direction on x axis</param>
        /// <param name="up">Up direction</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion DirectionToRotation(float3 forward, float3 up)
        {
            float3 t = math.normalize(math.cross(up, forward));
            return new quaternion(new float3x3(forward, math.cross(t, forward), t));
        }

        void InsertObstacle(Line rangeLS)
        {
            var angleRight = rangeLS.From;
            var angleLeft = rangeLS.To;

            // Early out if obstacle is very small
            if (rangeLS.Length < math.EPSILON)
                return;

            if (angleRight < -math.PI)
            {
                InsertObstacle(m_Nodes[Root].Right, new Line(math.PI + (angleRight + math.PI), math.PI));
                InsertObstacle(m_Nodes[Root].Left, new Line(-math.PI, angleLeft));
            }
            else if (angleLeft > math.PI)
            {
                InsertObstacle(m_Nodes[Root].Right, new Line(angleRight, math.PI));
                InsertObstacle(m_Nodes[Root].Left, new Line(-math.PI, angleLeft - math.PI * 2));
            }
            else
            {
                InsertObstacle(Root, new Line(angleRight, angleLeft));
            }
        }

        void InsertObstacle(SonarNodeHandle handle, Line line)
        {
            CheckHandle(handle);

            SonarNode node = m_Nodes[handle];

            var nodeLine = node.Line;
            if (nodeLine.Length == 0)
                return;

            if (Line.CutLine(nodeLine, line, out var result))
            {
                if (node.IsLeaf)
                {
                    if (result.SegmentCount == 2)
                    {
                        node.Left = CreateNode(result.Segment0);
                        node.Right = CreateNode(result.Segment1);
                        m_Nodes[handle] = node;
                    }
                    else
                    {
                        node.Line = result.Segment0;
                        m_Nodes[handle] = node;
                    }
                }
                else
                {
                    InsertObstacle(node.Right, line);
                    InsertObstacle(node.Left, line);
                }
            }
        }

        SonarNodeHandle CreateNode(Line line)
        {
            m_Nodes.Add(new SonarNode(line));
            return new SonarNodeHandle { Index = m_Nodes.Length - 1 };
        }

        bool FindClosestAngleLeft(out float angle)
        {
            var node = m_Nodes[m_Nodes[Root].Left];

            while (true)
            {
                if (node.IsLeaf)
                {
                    angle = node.Line.To;
                    return true;
                }
                else
                {
                    var left = m_Nodes[node.Left];
                    if (left.Line.Length != 0)
                    {
                        node = left;
                        continue;
                    }

                    var right = m_Nodes[node.Right];
                    if (right.Line.Length != 0)
                    {
                        node = right;
                        continue;
                    }

                    angle = 0;
                    return false;
                }
            }
        }

        bool FindClosestAngleRight(out float angle)
        {
            var node = m_Nodes[m_Nodes[Root].Right];

            while (true)
            {
                if (node.IsLeaf)
                {
                    angle = node.Line.From;
                    return true;
                }
                else
                {
                    var left = m_Nodes[node.Left];
                    if (left.Line.Length != 0)
                    {
                        node = left;
                        continue;
                    }

                    var right = m_Nodes[node.Right];
                    if (right.Line.Length != 0)
                    {
                        node = right;
                        continue;
                    }

                    angle = 0;
                    return false;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        float2 ToLocalSpace(float3 value) => math.mul(math.conjugate(m_Rotation), value).xz;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        float3 ToWorldSpace(float2 value) => math.mul(m_Rotation, new float3(value.x, 0, value.y));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float2 AngleToDirectionLS(float angle) => new float2(math.cos(angle), math.sin(angle));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float DirectionLSToAngle(float2 direction) => math.atan2(direction.y, direction.x);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float SignedAngle(float2 a, float2 b) => math.atan2(b.y * a.x - b.x * a.y, math.dot(a, b));

        /// <summary>
        /// Interface used to implement draw arc method.
        /// </summary>
        public interface IDrawArc
        {
            void DrawArc(float3 center, float3 normal, float3 from, float3 to, float angle, UnityEngine.Color color);
        }

        /// <summary>
        /// Interface used to implement draw circle method.
        /// </summary>
        public interface IDrawCircle
        {
            void DrawCircle(float3 center, float radius, UnityEngine.Color color);
        }

        /// <summary>
        /// Interface used to implement draw circle method.
        /// </summary>
        public interface IDrawLine
        {
            void DrawLine(float3 from, float3 to, UnityEngine.Color color);
        }

        /// <summary>
        /// Draws sonar that is not obstructed by obstacle. Use provided action to draw it.
        /// </summary>
        /// <param name="action">Structure used for drawing</param>
        /// <typeparam name="T">The type of draw structure</typeparam>
        public void DrawSonar<T>(T action) where T : unmanaged, IDrawArc
        {
            CheckIsCreated();
            if (m_Nodes.Length == 0)
                return;
            DrawSonarNode(action, Root, m_Position, m_Rotation);
        }

        /// <summary>
        /// Draw sonar obstacle collision points. Use provided action to draw it.
        /// </summary>
        /// <param name="action">Structure used for drawing</param>
        /// <param name="position">Position of obstacle</param>
        /// <param name="velocity">Velocity of obstacle</param>
        /// <param name="radius">Radius of obstacle</param>
        /// <param name="numIterations">Number of iterations of obstacle collisions</param>
        /// <typeparam name="T">The type of draw structure</typeparam>
        public void DrawObstacle<T>(T action, float3 position, float3 velocity, float radius, int numIterations = 5) where T : unmanaged, IDrawCircle
        {
            var c0 = new Circle(position.xz, radius);
            var c1 = new Circle(m_Position.xz, m_InnerRadius);

            if (Intersection.IntersectionOfTwoMovingCircles(c0, velocity.xz, c1, m_Speed, m_InnerRadius + m_OuterRadius, out Line angles, out Line times))
            {
                for (int i = 0; i < numIterations; ++i)
                {
                    float progress = ((float)i / (numIterations-1));
                    float angle = progress * angles.Length + angles.From;
                    float2 direction = new float2(math.cos(angle), math.sin(angle));
                    bool succes = Intersection.IntersectionOfTwoMovingCircles(c0, velocity.xz, c1, direction * m_Speed, out float t0, out float t1);
                    if (t0 >= 0 && math.isfinite(t0))
                    {
                        float3 p0 = new float3(c0.Point + velocity.xz * t0, position.y);
                        float3 p1 = new float3(c1.Point + direction * m_Speed * t0, m_Position.y);

                        p0 = math.mul(m_Rotation, p0);
                        p1 = math.mul(m_Rotation, p1);

                        action.DrawCircle(p0, radius, new Color(1, 0, 0, 0.2f));
                        action.DrawCircle(p1, m_InnerRadius, new Color(0, 0, 1, 0.2f));
                    }
                }
            }
        }

        /// <summary>
        /// Draw sonar obstacle collision points. Use provided action to draw it.
        /// </summary>
        /// <param name="action">Structure used for drawing</param>
        /// <param name="position">Position of obstacle</param>
        /// <param name="velocity">Velocity of obstacle</param>
        /// <param name="radius">Radius of obstacle</param>
        /// <param name="numIterations">Number of iterations of obstacle collisions</param>
        /// <typeparam name="T">The type of draw structure</typeparam>
        public void DrawObstacle<T>(T action, float3 from, float3 to) where T : unmanaged, IDrawLine
        {
            action.DrawLine(from, to, Color.red);
        }

        void DrawSonarNode<T>(T action, SonarNodeHandle handle, float3 position, quaternion rotation) where T : unmanaged, IDrawArc
        {
            var node = m_Nodes[handle];

            if (node.IsLeaf)
            {
                var line = node.Line;

                if (line.Length == 0)
                    return;

                float3 directionFromWS = ToWorldSpace(AngleToDirectionLS(line.From));
                float3 directionToWS = ToWorldSpace(AngleToDirectionLS(line.To));

                float3 up = Vector3.up;
                up = math.mul(rotation, up);

                action.DrawArc(position, up, directionFromWS, directionToWS, line.Length, new UnityEngine.Color(0, 1, 0, 0.2f));
            }
            else
            {
                DrawSonarNode(action, node.Left, position, rotation);
                DrawSonarNode(action, node.Right, position, rotation);
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        internal void CheckIsCreated()
        {
            if (!IsCreated)
                throw new Exception("SonarAvoidance is not initialized. It can happen if was created with argumentless contructor or disposed.");
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        void CheckHandle(SonarNodeHandle handle)
        {
            if (handle == SonarNodeHandle.Null)
                throw new Exception("SonarAvoidance handle is not valid.");
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        internal static void CheckArguments(quaternion rotation, float innerRadius, float outerRadius)
        {
            if (innerRadius < 0)
                throw new ArgumentException("Radius must be non negative", "innerRadius");
            if (outerRadius <= 0)
                throw new ArgumentException("Radius must be greater than zero", "outerRadius");
            if (math.any(!math.isfinite(rotation.value)))
                throw new ArgumentException($"Rotation cannot be zero or Infinite/NaN. ({rotation.value})", "rotation");
        }
    }

    /// <summary>
    /// Sonar avoidance utility class with some helpful functions.
    /// </summary>
    public static class SonarAvoidanceUtility
    {
        /// <summary>
        /// Draws sonar that is not obstructed by obstacle. Must be called inside <see cref="MonoBehaviour.OnGizmos"/> and only works in Editor.
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void DrawSonar(this SonarAvoidance sonar)
        {
            sonar.DrawSonar(new DrawArcGizmos 
            {
                InnerRadius = sonar.InnerRadius,
                OuterRadius = sonar.OuterRadius,
            });
        }

        /// <summary>
        /// Draw sonar obstacle collision points. Use provided action to draw it. Must be called inside <see cref="MonoBehaviour.OnGizmos"/> and only works in Editor.
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void DrawObstacle(this SonarAvoidance sonar, float3 position, float3 velocity, float radius, int numIterations = 5)
        {
            sonar.DrawObstacle(new DrawCircleGizmos(), position, velocity, radius, numIterations);
        }

        /// <summary>
        /// Draw sonar obstacle collision points. Use provided action to draw it. Must be called inside <see cref="MonoBehaviour.OnGizmos"/> and only works in Editor.
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void DrawObstacle(this SonarAvoidance sonar, float3 from, float3 to)
        {
            sonar.DrawObstacle(new DrawLineGizmos(), from, to);
        }

        /// <summary>
        /// Draws closest desired direction that is not obstructed by obstacle. Must be called inside <see cref="MonoBehaviour.OnGizmos"/> and only works in Editor.
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void DrawClosestDirection(this SonarAvoidance sonar)
        {
#if UNITY_EDITOR
            sonar.CheckIsCreated();

            if (!sonar.FindClosestDirection(out var direction))
                return;

            float3 position = sonar.Position;

            float3 up = new float3(0, 1, 0);
            up = math.mul(sonar.Rotation, up);

            float3 r = math.cross(up, direction);
            float3 l = -math.cross(up, direction);

            float3 rr = r * sonar.InnerRadius;
            float3 ll = l * sonar.InnerRadius;

            Vector3[] vertices = new Vector3[4];
            vertices[0] = position + rr;
            vertices[1] = position + direction * sonar.OuterRadius + rr;
            vertices[2] = position + direction * sonar.OuterRadius + ll;
            vertices[3] = position + ll;

            UnityEditor.Handles.color = new Color(0, 0, 1, 0.3f);
            UnityEditor.Handles.DrawAAConvexPolygon(vertices);
#endif
        }

        struct DrawArcGizmos : SonarAvoidance.IDrawArc
        {
            public float InnerRadius;
            public float OuterRadius;

            void SonarAvoidance.IDrawArc.DrawArc(float3 position, float3 up, float3 from, float3 to, float angle, UnityEngine.Color color)
            {
#if UNITY_EDITOR
                // Draw outer solid arc
                UnityEditor.Handles.color = color;
                UnityEditor.Handles.DrawSolidArc(position, up, to, math.degrees(angle), OuterRadius);

                // Draw wire for arc
                UnityEditor.Handles.color =  UnityEngine.Color.white;
                UnityEditor.Handles.DrawWireArc(position, up, to, math.degrees(angle), OuterRadius);
                UnityEditor.Handles.DrawLine(position, position + from * OuterRadius);
                UnityEditor.Handles.DrawLine(position, position + to * OuterRadius);

                // Draw inner solid arc
                UnityEditor.Handles.color = new UnityEngine.Color(1, 1, 1, 0.4f);
                UnityEditor.Handles.DrawSolidArc(position, up, to, math.degrees(angle), InnerRadius);
#endif
            }
        }

        struct DrawCircleGizmos : SonarAvoidance.IDrawCircle
        {
            void SonarAvoidance.IDrawCircle.DrawCircle(float3 position, float radius, UnityEngine.Color color)
            {
#if UNITY_EDITOR
                UnityEditor.Handles.color = color;
                UnityEditor.Handles.DrawSolidArc(new Vector3(position.x, 0, position.y), Vector3.up, Vector3.right, 360, radius);
#endif
            }
        }

        struct DrawLineGizmos : SonarAvoidance.IDrawLine
        {
            void SonarAvoidance.IDrawLine.DrawLine(float3 from, float3 to, UnityEngine.Color color)
            {
#if UNITY_EDITOR
                UnityEditor.Handles.color = color;
                UnityEditor.Handles.DrawLine(from, to);
#endif
            }
        }
    }
}
