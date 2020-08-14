﻿using OrchestratorWrapping;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Workers
{
    public class SocketIOWriter : BaseWriter {
        Workers.B2DWriter.DashStreamDescription[] streams;

        public SocketIOWriter(User user, string remoteURL, string remoteStream, Workers.B2DWriter.DashStreamDescription[] streams) : base(WorkerType.End) {
            if (streams == null) {
                throw new System.Exception($"[FPA] {Name()}: outQueue is null");
            }
            this.streams = streams;
            for (int i = 0; i < streams.Length; ++i) {
                streams[i].name = $"{remoteURL}{remoteStream}#{i}";
                Debug.Log($"[FPA] DeclareDataStream userId {user.userId} StreamType {streams[i].name}");
                OrchestratorWrapper.instance.DeclareDataStream(streams[i].name);
            }
            try {
                Start();
                Debug.Log($"[FPA] {Name()}: Started.");
            } catch (System.Exception e) {
                Debug.LogError(e.Message);
                throw e;
            }
        }

        public override string Name() {
            return $"{this.GetType().Name}";
        }


        public override void Stop() {
            base.Stop();
            for (int i = 0; i < streams.Length; ++i) {
                if (!streams[i].inQueue.IsClosed()) {
                    Debug.LogWarning($"[FPA] {Name()}: inQueue not closed, closing");
                    streams[i].inQueue.Close();
                }
            }
            Debug.Log($"[FPA] {Name()}: Stopped.");
            OrchestratorWrapper.instance.RemoveDataStream("AUDIO");
        }

        protected override void Update() {
            base.Update();
            if (OrchestratorWrapper.instance!=null && OrchestratorWrapper.instance.isConnected()) {
                for (int i = 0; i < streams.Length; ++i) {
                    BaseMemoryChunk chk = streams[i].inQueue.Dequeue();
                    if (chk == null) return;

                    var buf = new byte[chk.length];
                    System.Runtime.InteropServices.Marshal.Copy(chk.pointer, buf, 0, chk.length);
                    OrchestratorWrapper.instance.SendData(streams[i].name, buf);
                    chk.free();

                }
            }
        }
        // FPA: Ask Jack about GetSyncInfo().

        public override SyncConfig.ClockCorrespondence GetSyncInfo() {
            System.TimeSpan sinceEpoch = System.DateTime.UtcNow - new System.DateTime(1970, 1, 1);
            return new SyncConfig.ClockCorrespondence();
        }
    }
}
