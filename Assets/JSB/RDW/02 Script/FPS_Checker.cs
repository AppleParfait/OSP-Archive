using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _OSP
{
    public class FPS_Checker : MonoBehaviour
    {
        float deltaTime = 0.0f;

        GUIStyle style;
        Rect rect;
        float msec;
        float fps;
        float worstFps = 999f;
        string text;

        private bool tempb = true;

        void Awake()
        {
            //Application.targetFrameRate = -1;
            //Application.targetFrameRate = 90;

            int w = Screen.width, h = Screen.height;

            rect = new Rect(0, 0, w, h * 4 / 100);

            style = new GUIStyle();
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 4 / 100;
            style.normal.textColor = Color.red;
        }

        private void OnDisable()
        {
            tempb = false;
            StopAllCoroutines();
        }

        private void Start()
        {
            StartCoroutine("worstReset");
        }

        //void Update()
        //{
        //    deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        //}

        void FixedUpdate()
        {
            deltaTime += (Time.fixedDeltaTime - deltaTime) * 0.1f;
        }

        void OnGUI()//소스로 GUI 표시.
        {
            msec = deltaTime * 1000.0f;
            fps = 1.0f / deltaTime;  //초당 프레임 - 1초에

            if (fps < worstFps)  //새로운 최저 fps가 나왔다면 worstFps 바꿔줌.
                worstFps = fps;
            text = msec.ToString("F2") + "ms (" + fps.ToString("F0") + ") //worst : " + worstFps.ToString("F1");
            GUI.Label(rect, text, style);
        }

        IEnumerator worstReset() //코루틴으로 15초 간격으로 최저 프레임 리셋해줌.
        {
            while (tempb)
            {
                yield return new WaitForSeconds(5.0f);
                worstFps = 999f;
            }
        }

    }
}