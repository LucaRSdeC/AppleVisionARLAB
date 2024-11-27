using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;

namespace UnityEngine.XR.ARFoundation.Samples
{
    [RequireComponent(typeof(ARTrackedImageManager))]
    public class TrackedImageInfoManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The camera to set on the world space UI canvas for each instantiated image info.")]
        Camera m_WorldSpaceCanvasCamera;

        public Camera worldSpaceCanvasCamera
        {
            get => m_WorldSpaceCanvasCamera;
            set => m_WorldSpaceCanvasCamera = value;
        }

        [SerializeField]
        [Tooltip("If an image is detected but no source texture can be found, this texture is used instead.")]
        Texture2D m_DefaultTexture;

        public Texture2D defaultTexture
        {
            get => m_DefaultTexture;
            set => m_DefaultTexture = value;
        }

    [SerializeField]
    [Tooltip("The prefab to instantiate when the QR code is detected.")]
    private GameObject prefabToSpawn;

    [SerializeField]
    [Tooltip("The name of the QR code image to look for.")]
    private string targetQRCodeName;

    private ARTrackedImageManager m_TrackedImageManager;

    private bool prefabSpawned = false;
        void Awake()
        {
            m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
            Debug.Log("Image info manager is workirng");
        }

        void OnEnable()
        {
            m_TrackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
            Debug.Log("The listner is being enabled");
        }

        void OnDisable()
        {
            m_TrackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
        }

        void UpdateInfo(ARTrackedImage trackedImage)
        {
            var canvas = trackedImage.GetComponentInChildren<Canvas>();
            canvas.worldCamera = worldSpaceCanvasCamera;

            var text = canvas.GetComponentInChildren<Text>();
            text.text = string.Format(
                "{0}\ntrackingState: {1}\nGUID: {2}\nReference size: {3} cm\nDetected size: {4} cm",
                trackedImage.referenceImage.name,
                trackedImage.trackingState,
                trackedImage.referenceImage.guid,
                trackedImage.referenceImage.size * 100f,
                trackedImage.size * 100f);

            var planeParentGo = trackedImage.transform.GetChild(0).gameObject;
            var planeGo = planeParentGo.transform.GetChild(0).gameObject;
            TrySpawnPrefab(trackedImage);
            if (trackedImage.trackingState != TrackingState.None)
            {
                planeGo.SetActive(true);
                trackedImage.transform.localScale = new Vector3(trackedImage.size.x, 1f, trackedImage.size.y);

                var material = planeGo.GetComponentInChildren<MeshRenderer>().material;
                var textureToUse = trackedImage.referenceImage.texture != null ? trackedImage.referenceImage.texture : defaultTexture;

                if (textureToUse != null && textureToUse.isReadable)
                {
                    material.mainTexture = textureToUse;
                }
                else
                {
                    Debug.LogWarning($"Texture for {trackedImage.referenceImage.name} is not readable.");
                    material.mainTexture = defaultTexture;
                }
            }
            else
            {
                planeGo.SetActive(false);
            }
        }

        void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
        {
            foreach (var trackedImage in eventArgs.added)
            {
                //TrySpawnPrefab(trackedImage);
                //trackedImage.transform.localScale = new Vector3(0.01f, 1f, 0.01f);
                UpdateInfo(trackedImage);
                
            }

            foreach (var trackedImage in eventArgs.updated)
            {
                //TrySpawnPrefab(trackedImage);
                UpdateInfo(trackedImage);
                
            }
        }
        private void TrySpawnPrefab(ARTrackedImage trackedImage)
        {
            // Set position to match the tracked image
            prefabToSpawn.transform.position = trackedImage.transform.position;

            // Calculate the current rotation with a 180-degree Y-axis rotation
            Quaternion targetRotation = Quaternion.Euler(
                prefabToSpawn.transform.eulerAngles.x,              // Keep current X rotation
                trackedImage.transform.eulerAngles.y + 180f,        // Rotate 180 degrees on Y-axis
                prefabToSpawn.transform.eulerAngles.z               // Keep current Z rotation
            );

            // Apply the calculated rotation
            prefabToSpawn.transform.rotation = targetRotation;

            // Activate the prefab if it is not already active
            if (!prefabToSpawn.activeSelf)
            {
                prefabToSpawn.SetActive(true);
            }

            Debug.Log($"Prefab instantiated at {trackedImage.transform.position} with 180-degree Y rotation adjustment.");
        }



    }
}
