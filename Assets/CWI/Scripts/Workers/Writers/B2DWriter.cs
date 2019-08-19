﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Workers {
    public class B2DWriter : BaseWorker {
        bin2dash.connection uploader;
        BinaryWriter bw;

        public B2DWriter(Config._User._PCSelfConfig._Bin2Dash cfg) : base(WorkerType.End) {
            try {
<<<<<<< HEAD
                signals_unity_bridge_pinvoke.SetPaths("bin2dash");
                uploader = bin2dash_pinvoke.vrt_create(cfg.streamName, bin2dash_pinvoke.VRT_4CC('c', 'w', 'i', '1'), cfg.url, cfg.segmentSize, cfg.segmentLife);
                if (uploader != System.IntPtr.Zero) {
                    Debug.Log($"Bin2Dash {cfg.url}");
=======
                if ( cfg.fileMirroring ) bw = new BinaryWriter(new FileStream( $"{Application.dataPath}/../{cfg.streamName}.dashdump", FileMode.Create));
                uploader = bin2dash.create(cfg.streamName, bin2dash.VRT_4CC('c', 'w', 'i', '1'), cfg.url, cfg.segmentSize, cfg.segmentLife);
                if (uploader != null)
                {
                    Debug.Log($"Bin2Dash vrt_create(url={cfg.url})");
>>>>>>> master
                    Start();
                }
                else
                    throw new System.Exception($"PCRealSense2Reader: vrt_create: failed to create uploader url={cfg.url}/{cfg.streamName}.mpd");
            }
            catch (System.Exception e) {
                Debug.LogError($"Exception during B2DWriter constructor: {e.Message}");
                throw e;
            }
        }

        public B2DWriter(Config._User._PCSelfConfig._Bin2Dash cfg, string id) : base(WorkerType.End) {
            try {
                signals_unity_bridge_pinvoke.SetPaths("bin2dash");
                uploader = bin2dash_pinvoke.vrt_create(cfg.streamName, bin2dash_pinvoke.VRT_4CC('c', 'w', 'i', '1'), cfg.url + id + "/", cfg.segmentSize, cfg.segmentLife);
                if (uploader != System.IntPtr.Zero) {
                    Debug.Log($"Bin2Dash {cfg.url + id + "/" + cfg.streamName}");
                    Start();
                }
                else
                    throw new System.Exception($"PCRealSense2Reader: vrt_create: failed to create uploader {cfg.url + id + "/"}/{cfg.streamName}.mpd");
            }
            catch (System.Exception e) {
                Debug.LogError(e.Message);
                throw e;
            }
        }

        public override void OnStop() {
            uploader = null;
            bw?.Close();
            base.OnStop();
            Debug.Log("B2DWriter Sopped");
        }

        protected override void Update() {
            base.Update();
            if (token != null) {
                bw?.Write(token.currentByteArray, 0, token.currentSize);
                if (!uploader.push_buffer(token.currentBuffer, (uint)token.currentSize))
                    Debug.Log("ERROR sending data");
                Next();
            }
        }
    }
}
