﻿using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Features
{

    public abstract class SignalRImageStreamerBase<TEntityController, TArgs> : SignalRUnityFeatureBase<TEntityController, TArgs>
            where TEntityController : SignalREntityController<TArgs>
    {
        public Camera camera;
        private RenderTexture cameraTexture;

        public int resolutionX;
        public int resolutionY;

        public override void AwakeVirtual()
        {
            base.AwakeVirtual();
            cameraTexture = new(resolutionX, resolutionY, 32);
        }

        private bool first = true;

        private HubConnection HubConnection;

        public override void Connected(HubConnection hubConnection)
        {
            base.Connected(hubConnection);
            HubConnection = hubConnection;
        }

        public void Update()
        {
            if ((first || Time.renderedFrameCount % 30 == 0) && HubConnection is not null)
            {
                if (camera is { })
                {
                    camera.targetTexture = cameraTexture;
                    camera.Render();
                    camera.targetTexture = null;
                    Texture2D texture2D = new(resolutionX, resolutionX, TextureFormat.RGBA32, false);
                    var oldActive = RenderTexture.active;
                    RenderTexture.active = cameraTexture;
                    
                    texture2D.ReadPixels(new Rect(0, 0, resolutionX, resolutionY), 0, 0);
                    RenderTexture.active = oldActive;

                    var pngBytes = texture2D.EncodeToPNG();
                    var base64 = Convert.ToBase64String(pngBytes);
                    //HubConnection.StreamAsync("GameplayImage", base64);
                    HubConnection.SendAsync("GameplayImage", base64);
                }
                first = false;
            }
        }
    }
}
