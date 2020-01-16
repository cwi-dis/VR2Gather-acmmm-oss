﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceDashReceiver : MonoBehaviour {

    Workers.BaseWorker reader;
    Workers.BaseWorker codec;
    Workers.AudioPreparer preparer;
    Workers.Token token;
    AudioSource audioSource;

    // Start is called before the first frame update
    public void Init(Config._User._SUBConfig cfg, string url = "") {
        const int frequency = 16000;
        const double optimalAudioBufferDuration = 1.2;   // How long we want to buffer audio (in seconds)
        const int optimalAudioBufferSize = (int)(frequency * optimalAudioBufferDuration);
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;
        //audioSource.clip = AudioClip.Create("clip0", 320, 1, 16000, false);
        audioSource.loop = true;
        audioSource.Play();
        try {
            reader = new Workers.SUBReader(cfg, url);
            codec = new Workers.VoiceDecoder();
            preparer = new Workers.AudioPreparer(optimalAudioBufferSize);
            reader.AddNext(codec).AddNext(preparer).AddNext(reader);
            reader.token = token = new Workers.Token();
        } catch (System.Exception e) {
            Debug.Log(">>ERROR " + e);

        }
    }

    void OnDestroy() {
        reader?.Stop();
        codec?.Stop();
        preparer?.Stop();
    }

    void OnAudioRead(float[] data) {
        if (preparer == null || !preparer.GetAudioBuffer(data, data.Length))
            System.Array.Clear(data, 0, data.Length);
    }

    float[] tmpBuffer;
    void OnAudioFilterRead(float[] data, int channels) {
        if (tmpBuffer == null) tmpBuffer = new float[data.Length];
        if (preparer != null && preparer.GetAudioBuffer(tmpBuffer, tmpBuffer.Length)) {
            int cnt = 0;
            do {
                data[cnt] += tmpBuffer[cnt];
            } while (++cnt < data.Length);
        }
    }


}