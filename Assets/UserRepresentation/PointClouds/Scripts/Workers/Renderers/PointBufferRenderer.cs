﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Workers
{
    public class PointBufferRenderer : MonoBehaviour
    {
        ComputeBuffer           pointBuffer;
        int                     pointCount = 0;
        Material                material;
        MaterialPropertyBlock   block;
        Workers.BufferPreparer preparer;

        public string Name()
        {
            return $"{this.GetType().Name}#{instanceNumber}";
        }


        // Start is called before the first frame update
        void Start() {
            if (material == null)  material = Resources.Load<Material>("PointCloudsBuffer");
            block = new MaterialPropertyBlock();
        }

        public void SetPreparer(Workers.BufferPreparer _preparer)
        {
            if (preparer != null)
            {
                Debug.LogError($"Programmer error: {Name()}: attempt to set second preparer");
            }
            preparer = _preparer;
        }

        private void Update() {
            pointCount = preparer.GetComputeBuffer(ref pointBuffer);
            if (pointCount == 0 || pointBuffer == null || !pointBuffer.IsValid()) return;
            block.SetBuffer("_PointBuffer", pointBuffer);
            block.SetFloat("_PointSize", preparer.GetPointSize());
            block.SetMatrix("_Transform", transform.localToWorldMatrix);

            Graphics.DrawProcedural(material, new Bounds(transform.position, Vector3.one*2), MeshTopology.Points, pointCount, 1, null, block);
            statsUpdate(pointCount, preparer.currentTimestamp);
        }
     
        public void OnDestroy() {
            if (pointBuffer != null) { pointBuffer.Release(); pointBuffer = null; }
            if (material != null) { material = null; }
        }

        static int instanceCounter = 0;
        int instanceNumber = instanceCounter++;
        System.DateTime statsLastTime;
        double statsTotalPointcloudCount = 0;
        double statsTotalPointCount = 0;
        const int statsInterval = 1;

        public void statsUpdate(int pointCount, ulong timestamp)
        {
            System.TimeSpan sinceEpoch = System.DateTime.UtcNow - new System.DateTime(1970, 1, 1);
            if (statsLastTime == null)
            {
                statsLastTime = System.DateTime.Now;
                statsTotalPointcloudCount = 0;
                statsTotalPointCount = 0;
            }
            if (System.DateTime.Now > statsLastTime + System.TimeSpan.FromSeconds(statsInterval))
            {
                Debug.Log($"stats: ts={System.DateTime.Now.TimeOfDay.TotalSeconds:F3}, component={Name()}, fps={statsTotalPointcloudCount / statsInterval}, points_per_cloud={(int)(statsTotalPointCount / (statsTotalPointcloudCount==0?1: statsTotalPointcloudCount))}, pc_timestamp={timestamp}, pc_latency_ms={(ulong)sinceEpoch.TotalMilliseconds-timestamp}");
                statsTotalPointcloudCount = 0;
                statsTotalPointCount = 0;
                statsLastTime = System.DateTime.Now;
            }
            statsTotalPointCount += pointCount;
            statsTotalPointcloudCount += 1;
        }
    }
}
