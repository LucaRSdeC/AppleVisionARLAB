using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class QRPrefabSpawner : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The prefab to instantiate when the QR code is detected.")]
    private GameObject prefabToSpawn;

    [SerializeField]
    [Tooltip("The name of the QR code image to look for.")]
    private string targetQRCodeName;

    private ARTrackedImageManager trackedImageManager;
    private bool prefabSpawned = false;

    void Awake()
    {
        //prefabToSpawn.SetActive(false);
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }

    void OnDisable()
    {
        trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
    }

    private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        if (prefabSpawned) return;

        foreach (var trackedImage in eventArgs.added)
        {
            TrySpawnPrefab(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            TrySpawnPrefab(trackedImage);
        }
    }

    private void TrySpawnPrefab(ARTrackedImage trackedImage)
    {
        if (!prefabSpawned && trackedImage.referenceImage.name == targetQRCodeName && trackedImage.trackingState == TrackingState.Tracking)
        {
            prefabToSpawn.transform.position = trackedImage.transform.position;
            prefabToSpawn.transform.rotation = Quaternion.LookRotation(trackedImage.transform.up, trackedImage.transform.forward);
            prefabToSpawn.SetActive(true);

            Debug.Log($"Prefab instantiated at {trackedImage.transform.position} with rotation {trackedImage.transform.rotation}.");
            prefabSpawned = true;
        }
    }
}
