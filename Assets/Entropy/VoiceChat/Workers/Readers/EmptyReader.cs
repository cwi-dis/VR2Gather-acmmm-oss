﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP.SocketIO;

namespace Workers
{
    public class EmptyReader : BaseWorker {
        Coroutine coroutine;

        public EmptyReader() : base(WorkerType.Init) {            
            Start();
        }

        protected override void Update() {
            base.Update();
            if (token != null) {
                Next();
            }
        }

        public override void OnStop() {
            base.OnStop();
            Debug.Log("EmptyReader Sopped");
        }

    }
}