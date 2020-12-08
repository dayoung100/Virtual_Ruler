//-----------------------------------------------------------------------
// <copyright file="HelloARController.cs" company="Google LLC">
//
// Copyright 2020 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleARCore.Examples.HelloAR
{
    using System.Collections.Generic;
    using GoogleARCore;
    using GoogleARCore.Examples.Common;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI; //button 사용을 위해 추가

#if UNITY_EDITOR
    // Set up touch input propagation while using Instant Preview in the editor.
    using Input = InstantPreviewInput;
#endif

    /// <summary>
    /// Controls the HelloAR example.
    /// </summary>
    public class HelloARController : MonoBehaviour
    {
        //-----추가-----
        //instantiate gameobjects for the Points
        GameObject[] Points;
        public int p_index = 0;

        //instances of the Lines
        GameObject[] Lines;
        public int l_index = 0;

        //instances of the Lines
        GameObject[] Texts;
        public int t_index = 0;

        //prevent doubles
        bool exist = false;

        /// A model to place when a raycast from a user touch hits a plane.
        public GameObject prefabPoint, prefabLine, prefabText;

        /// The rotation in degrees need to apply to model when the Andy model is placed.
        private const float k_ModelRotation = 180.0f;

        //-----추가-----

        /// <summary>
        /// The Depth Setting Menu.
        /// </summary>
        public DepthMenu DepthMenu;

        /// <summary>
        /// The Instant Placement Setting Menu.
        /// </summary>
        public InstantPlacementMenu InstantPlacementMenu;

        /// <summary>
        /// A prefab to place when an instant placement raycast from a user touch hits an instant
        /// placement point.
        /// </summary>
        public GameObject InstantPlacementPrefab;

        /// <summary>
        /// The first-person camera being used to render the passthrough camera image (i.e. AR
        /// background).
        /// </summary>
        public Camera FirstPersonCamera;

        /// <summary>
        /// A prefab to place when a raycast from a user touch hits a vertical plane.
        /// </summary>
        public GameObject GameObjectVerticalPlanePrefab;

        /// <summary>
        /// A prefab to place when a raycast from a user touch hits a horizontal plane.
        /// </summary>
        public GameObject GameObjectHorizontalPlanePrefab;

        /// <summary>
        /// A prefab to place when a raycast from a user touch hits a feature point.
        /// </summary>
        public GameObject GameObjectPointPrefab;

        /// <summary>
        /// The rotation in degrees need to apply to prefab when it is placed.
        /// </summary>
        private const float _prefabRotation = 180.0f;

        /// <summary>
        /// True if the app is in the process of quitting due to an ARCore connection error,
        /// otherwise false.
        /// </summary>
        private bool _isQuitting = false;

        //지우개 버튼 추가
        public Button Erasebutton;


        /// <summary>
        /// The Unity Awake() method.
        /// </summary>
        public void Awake()
        {
            // Enable ARCore to target 60fps camera capture frame rate on supported devices.
            // Note, Application.targetFrameRate is ignored when QualitySettings.vSyncCount != 0.
            Application.targetFrameRate = 60;
        }

        //-----추가-----
        private void Start()
        {
            //initialize arrays
            Points = new GameObject[10000];
            Lines = new GameObject[10000];
            Texts = new GameObject[10000];
        }
        //-----추가-----

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 pos;
                pos.x = Random.Range(-2.0f, 2.0f);
                pos.z = Random.Range(-2.0f, 2.0f);
                pos.y = 0;

                Points[p_index] = Instantiate(prefabPoint, pos, Quaternion.identity);


                if (p_index > 0)
                {
                    //Erasebutton = this.transform.GetComponent<Button>();
                    //Erasebutton.onClick.AddListener(Erase);

                    /////////////////////////////////
                    // MEASURE DISTANCES
                    ///////////////////////////////////
                    //direction and center of the line
                    Vector3 dir = Points[p_index].transform.position - Points[p_index - 1].transform.position;  //벡터
                    Vector3 center = Points[p_index - 1].transform.position + dir / 2;  //중간

                    Lines[l_index] = Instantiate(prefabLine, center, Quaternion.identity);
                    Lines[l_index].transform.rotation = Quaternion.LookRotation(dir, Points[p_index].transform.up); //점의 y축 방향으로
                    Lines[l_index].transform.localScale = new Vector3(Lines[l_index].transform.localScale[0], Lines[l_index].transform.localScale[1], dir.magnitude); //magnitude: 벡터 길이(거리)

                    //instantiate text and set value too
                    Texts[t_index] = Instantiate(prefabText, center, Quaternion.identity);
                    //Texts[t_index].transform.LookAt(Points[p_index].transform.position);
                    //Texts[t_index].transform.localEulerAngles = new Vector3(90, Texts[t_index].transform.localEulerAngles.y+90, 0);//line과 같은 방향 되도록 //Euler가 절대값으로 사용? local은 월드좌표계 말고 로컬 좌표계?
                    if (dir.x > 0)
                    {
                        Texts[t_index].transform.rotation = Quaternion.LookRotation(dir, Points[p_index].transform.up);
                    }
                    else
                    {
                        Texts[t_index].transform.rotation = Quaternion.LookRotation(-dir, Points[p_index].transform.up);
                    }
                    UnityEngine.UI.Text txScp = Texts[t_index].transform.GetChild(0).transform.GetChild(0).GetComponent<UnityEngine.UI.Text>();  //Texts-canvas-text
                    txScp.text = Mathf.Round(dir.magnitude * 1000) / 10 + "cm";  //유니티의 기본 단위는 m(미터)

                    t_index += 1;
                    l_index += 1;
                }

                exist = true;
                p_index += 1;

            }


            UpdateApplicationLifecycle();

            // If the player has not touched the screen, we are done with this update.
            Touch touch;
            if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
            {
                return;
            }

            // Should not handle input if the player is pointing on UI.
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return;
            }

            // Raycast against the location the player touched to search for planes.
            TrackableHit hit;
            bool foundHit = false;
            if (InstantPlacementMenu.IsInstantPlacementEnabled())
            {
                foundHit = Frame.RaycastInstantPlacement(
                    touch.position.x, touch.position.y, 1.0f, out hit);
            }
            else
            {
                TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                    TrackableHitFlags.FeaturePointWithSurfaceNormal;
                foundHit = Frame.Raycast(
                    touch.position.x, touch.position.y, raycastFilter, out hit);
            }

            if (foundHit)
            {
                // Use hit pose and camera pose to check if hittest is from the
                // back of the plane, if it is, no need to create the anchor.
                if ((hit.Trackable is DetectedPlane) &&
                    Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position,
                        hit.Pose.rotation * Vector3.up) < 0)
                {
                    Debug.Log("Hit at back of the current DetectedPlane");
                }
                else
                {
                    if (DepthMenu != null)
                    {
                        // Show depth card window if necessary.
                        DepthMenu.ConfigureDepthBeforePlacingFirstAsset();
                    }

                    // Choose the prefab based on the Trackable that got hit.
                    GameObject prefab;
                    if (hit.Trackable is InstantPlacementPoint)
                    {
                        prefab = InstantPlacementPrefab;
                    }
                    else if (hit.Trackable is FeaturePoint)
                    {
                        prefab = GameObjectPointPrefab;
                    }
                    else if (hit.Trackable is DetectedPlane)
                    {
                        DetectedPlane detectedPlane = hit.Trackable as DetectedPlane;
                        if (detectedPlane.PlaneType == DetectedPlaneType.Vertical)
                        {
                            prefab = GameObjectVerticalPlanePrefab;
                        }
                        else
                        {
                            prefab = GameObjectHorizontalPlanePrefab;
                        }
                    }
                    else
                    {
                        prefab = GameObjectHorizontalPlanePrefab;
                    }
                    /*
                    // Instantiate prefab at the hit pose.
                    var gameObject = Instantiate(prefab, hit.Pose.position, hit.Pose.rotation);

                    // Compensate for the hitPose rotation facing away from the raycast (i.e.
                    // camera).
                    gameObject.transform.Rotate(0, _prefabRotation, 0, Space.Self);*/

                    // Create an anchor to allow ARCore to track the hitpoint as understanding of
                    // the physical world evolves.
                    var anchor = hit.Trackable.CreateAnchor(hit.Pose);
                    /*
                    // Make game object a child of the anchor.
                    gameObject.transform.parent = anchor.transform;

                    // Initialize Instant Placement Effect.
                    if (hit.Trackable is InstantPlacementPoint)
                    {
                        gameObject.GetComponentInChildren<InstantPlacementEffect>()
                        .InitializeWithTrackable(hit.Trackable);
                    }*/

                    //-----추가-----
                    Points[p_index] = Instantiate(prefabPoint, hit.Pose.position, hit.Pose.rotation);

                    // Compensate for the hitPose rotation facing away from the raycast (i.e.
                    // camera).
                    Points[p_index].transform.Rotate(0, k_ModelRotation, 0, Space.Self);

                    // Create an anchor to allow ARCore to track the hitpoint as understanding of
                    // the physical world evolves.

                    // Make Andy model a child of the anchor.
                    Points[p_index].transform.parent = anchor.transform;



                    //isntantiate line if number of Points is greater than 2
                    /*if (p_index > 0)
                    {
                        /////////////////////////////////
                        // MEASURE DISTANCES
                        ///////////////////////////////////
                        //direction and center of the line
                        Vector3 dir = Points[p_index].transform.position - Points[p_index - 1].transform.position;  //벡터
                        Vector3 center = Points[p_index - 1].transform.position + dir / 2;  //중간

                        Lines[l_index] = Instantiate(prefabLine, center, hit.Pose.rotation);
                        Lines[l_index].transform.rotation = Quaternion.LookRotation(dir, Points[p_index].transform.up); //점의 y축 방향으로
                        Lines[l_index].transform.localScale = new Vector3(Lines[l_index].transform.localScale[0], Lines[l_index].transform.localScale[1], dir.magnitude); //magnitude: 벡터 길이(거리)

                        // Make line model a child of the anchor.
                        Lines[l_index].transform.parent = anchor.transform;


                        //instantiate text and set value too
                        Texts[t_index] = Instantiate(prefabText, center, hit.Pose.rotation);
                        //Texts[t_index].transform.LookAt(Points[p_index].transform.position);
                        //Texts[t_index].transform.localEulerAngles = new Vector3(90, Texts[t_index].transform.localEulerAngles.y + 90, 0);//line과 같은 방향 되도록 //Euler가 절대값으로 사용? local은 월드좌표계 말고 로컬 좌표계?
                        if (dir.x > 0)
                        {
                            Texts[t_index].transform.rotation = Quaternion.LookRotation(dir, Points[p_index].transform.up);
                        }
                        else
                        {
                            Texts[t_index].transform.rotation = Quaternion.LookRotation(-dir, Points[p_index].transform.up);
                        }
                        Text txScp = Texts[t_index].transform.GetChild(0).transform.GetChild(0).GetComponent<Text>();  //Texts-canvas-text
                        txScp.text = Mathf.Round(dir.magnitude * 1000) / 10 + "cm";  //유니티의 기본 단위는 m(미터)

                        // Make Texts model a child of the anchor.
                        Texts[t_index].transform.parent = anchor.transform;

                        t_index += 1;
                        l_index += 1;
                    }

                    exist = true;
                    p_index += 1;*/
                    //-----추가-----
                }
            }
        }

        //추가
        public void EraseButtonClick()
        {
            //print("erase_call");
            if (p_index > 0)
            {
                
                if(l_index < 1 || t_index < 1)
                {
                    Destroy(Points[p_index - 1]); //가장 최근의 점 삭제

                    p_index = 0;
                    l_index = 0;
                    t_index = 0;
                }
                else
                {
                    Destroy(Points[p_index - 1]); //가장 최근의 점 삭제
                    Destroy(Lines[l_index - 1]);  //가장 최근의 선 삭제
                    Destroy(Texts[t_index - 1]);  //가장 최근의 글자 삭제

                    p_index -= 1;
                    l_index -= 1;
                    t_index -= 1;
                }
                
            }

        }


        private void FixedUpdate()
        {
            if (exist == true)
            {
                exist = false;
            }

            foreach (GameObject p in Points)
            {
                if (p_index > 0)
                {
                    if (p == Points[p_index - 1])
                    {
                        p.GetComponent<MeshRenderer>().material.color = Color.blue;
                        break;
                    }
                    else
                    {
                        p.GetComponent<MeshRenderer>().material.color = Color.red;
                    }
                }
            }

        }

        /// <summary>
        /// Check and update the application lifecycle.
        /// </summary>
        private void UpdateApplicationLifecycle()
        {
            // Exit the app when the 'back' button is pressed.
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }

            // Only allow the screen to sleep when not tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                Screen.sleepTimeout = SleepTimeout.SystemSetting;
            }
            else
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }

            if (_isQuitting)
            {
                return;
            }

            // Quit if ARCore was unable to connect and give Unity some time for the toast to
            // appear.
            if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
            {
                ShowAndroidToastMessage("Camera permission is needed to run this application.");
                _isQuitting = true;
                Invoke("DoQuit", 0.5f);
            }
            else if (Session.Status.IsError())
            {
                ShowAndroidToastMessage(
                    "ARCore encountered a problem connecting.  Please start the app again.");
                _isQuitting = true;
                Invoke("DoQuit", 0.5f);
            }
        }

        /// <summary>
        /// Actually quit the application.
        /// </summary>
        private void DoQuit()
        {
            Application.Quit();
        }

        /// <summary>
        /// Show an Android toast message.
        /// </summary>
        /// <param name="message">Message string to show in the toast.</param>
        private void ShowAndroidToastMessage(string message)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity =
                unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (unityActivity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toastObject =
                        toastClass.CallStatic<AndroidJavaObject>(
                            "makeText", unityActivity, message, 0);
                    toastObject.Call("show");
                }));
            }
        }
    }
}
