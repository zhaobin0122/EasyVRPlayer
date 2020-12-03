// Copyright 2016 Nibiru. All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using UnityEngine;
namespace Nvr.Internal
{
    // 帧率打印，挂载在任意物体即可
    public class NvrFPS : MonoBehaviour
    {

        private string fpsFormat;
        private float updateInterval = 0.2f;//设定更新帧率的时间间隔为0.2秒  
        private float accum = .0f;
        private int frames = 0;
        private float timeLeft;
        public static float fpsDeltaTime;

        TextMesh textMesh;
        // Use this for initialization
        void Start()
        {
            textMesh = GetComponent<TextMesh>();
        }

        // Update is called once per frame
        void Update()
        {
            calculate_fps();
            fpsDeltaTime += Time.deltaTime;
            if (fpsDeltaTime > 1)
            {
                //Debug.Log(fpsFormat);
                fpsDeltaTime = 0;
                if (textMesh != null)
                {
                    textMesh.text = fpsFormat;
                }
            }
        }

        private void calculate_fps()
        {
            timeLeft -= Time.deltaTime;
            accum += Time.timeScale / Time.deltaTime;
            ++frames;

            if (timeLeft <= 0)
            {
                float fps = accum / frames;
                fpsFormat = System.String.Format("{0:F3}fps", fps);
                // Debug.Log("FPS:" + fpsFormat);
                timeLeft = updateInterval;
                accum = .0f;
                frames = 0;
            }
        }
    }
}