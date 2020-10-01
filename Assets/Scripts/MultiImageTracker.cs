using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARTrackedImageManager))]
public class MultiImageTracker : MonoBehaviour, ISerializationCallbackReceiver
{

    GameObject Object;

    // TODO: Getting More Information on Scanning
    public Text trackingStateText; 

    // TODO: Spawn an Object on the Scanned Image with Touch Method
    public Vector3 touchPosition; 

    //Used to associate an 'XRReferenceImage' with a Prefab by using the 'XRReferenceImage's' guid as a unique identifier for a particular reference image.
    [Serializable]
    struct NamedPrefab
    {
        // System.Guid isn't serializable, so we store the Guid as a string. At runtime, this is converted back to a System.Guid
        public string imageGuid;
        public GameObject imagePrefab;
        public NamedPrefab(Guid guid, GameObject prefab)
        {
            imageGuid = guid.ToString();
            imagePrefab = prefab;
        }
    }

    [SerializeField]
    [HideInInspector]
    List<NamedPrefab> m_PrefabsList = new List<NamedPrefab>();

    Dictionary<Guid, GameObject> m_PrefabsDictionary = new Dictionary<Guid, GameObject>();
    Dictionary<Guid, GameObject> m_Instantiated = new Dictionary<Guid, GameObject>();
    ARTrackedImageManager m_TrackedImageManager;

    [SerializeField]
    [Tooltip("Reference Image Library")]
    XRReferenceImageLibrary m_ImageLibrary;

    //Get the XRReferenceImageLibrary
    public XRReferenceImageLibrary imageLibrary
    {
        get => m_ImageLibrary;
        set => m_ImageLibrary = value;
    }

    public void OnBeforeSerialize()
    {
        m_PrefabsList.Clear();
        foreach (var kvp in m_PrefabsDictionary)
        {
            m_PrefabsList.Add(new NamedPrefab(kvp.Key, kvp.Value));
        }
    }

    public void OnAfterDeserialize()
    {
        m_PrefabsDictionary = new Dictionary<Guid, GameObject>();
        foreach (var entry in m_PrefabsList)
        {
            m_PrefabsDictionary.Add(Guid.Parse(entry.imageGuid), entry.imagePrefab);
        }
    }

    void Awake()
    {
        //ar_RaycastManager = GetComponent<ARRaycastManager>(); //Detected features in the physical environmen
        trackingStateText = GameObject.Find("trackingStateText").GetComponent<Text>();
        m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    //bool TryGetTouchPosition(out Vector2 touchPosition) {
    //    // TODO: Spawn an Object on the Scanned Image with Touch Method 
    //    if (Input.touchCount > 0)
    //    {
    //        touchPosition = Input.GetTouch(0).position;
    //        return true;
    //    }
    //    touchPosition = default;
    //    return false;
    //}

    private void Update() {
        Object = GameObject.FindGameObjectWithTag("Compass"); // Returns one active Compass Object tagged tag.
    }

    void OnEnable() {
        m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable() {
        m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs) {
        foreach (var trackedImage in eventArgs.added) {
            // Give the initial image a reasonable default scale
            var minLocalScalar = Mathf.Min(trackedImage.size.x, trackedImage.size.y) / 2;
            trackedImage.transform.localScale = new Vector3(minLocalScalar, minLocalScalar, minLocalScalar);
            AssignPrefab(trackedImage);
        }
        foreach (ARTrackedImage updatedImage in eventArgs.updated) {
            var minLocalScalar = Mathf.Min(updatedImage.size.x, updatedImage.size.y) / 2;
            updatedImage.transform.localScale = new Vector3(minLocalScalar, minLocalScalar, minLocalScalar);

            if (updatedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking) {
                //Tracking is working normally.
                updatePrefab(updatedImage);
            }
            else if (updatedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Limited) {
                //Some tracking information is available, but it is limited or of poor quality.
                updateLimitedPrefab(updatedImage);
            }
            else if (updatedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.None) {
                notTracking(updatedImage);
            }
        }
        foreach (ARTrackedImage removedImage in eventArgs.removed) {
            // TODO: Apply Method to Destroy Object 
        }
    }

    void AssignPrefab(ARTrackedImage trackedImage) {
        if (m_PrefabsDictionary.TryGetValue(trackedImage.referenceImage.guid, out var prefab))
            m_Instantiated[trackedImage.referenceImage.guid] = Instantiate(prefab, trackedImage.transform);
    }

    void updatePrefab(ARTrackedImage updatedImage) {
        
        trackingStateText.text = "Marker Tracked";
        trackingStateText.color = Color.green;
        m_Instantiated[updatedImage.referenceImage.guid].SetActive(true);

        if (updatedImage.referenceImage.name == "pointerMarker") {
            Object.GetComponent<GPSManager>().trackedImageTransform = updatedImage.transform;
            Object.GetComponent<GPSManager>().pointerMarkerDetection = true;
        }
    }

    // TODO: Spawn an Object on the Scanned Image with Touch Method
    static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    //private ARRaycastManager ar_RaycastManager;


    void updateLimitedPrefab(ARTrackedImage updatedImage) {

        if (m_PrefabsDictionary.TryGetValue(updatedImage.referenceImage.guid, out var prefab)) {
            trackingStateText.text = "Marker Not Tracked";
            trackingStateText.color = Color.red;
            m_Instantiated[updatedImage.referenceImage.guid].SetActive(false);
        }
    }

    void notTracking(ARTrackedImage updatedImage) {

    }

    void destroyObject(ARTrackedImage removedImage) {

    }
    
    public GameObject GetPrefabForReferenceImage(XRReferenceImage referenceImage)
        => m_PrefabsDictionary.TryGetValue(referenceImage.guid, out var prefab) ? prefab : null;

    public void SetPrefabForReferenceImage(XRReferenceImage referenceImage, GameObject alternativePrefab) {
        m_PrefabsDictionary[referenceImage.guid] = alternativePrefab;
        if (m_Instantiated.TryGetValue(referenceImage.guid, out var instantiatedPrefab)) {
            m_Instantiated[referenceImage.guid] = Instantiate(alternativePrefab, instantiatedPrefab.transform.parent);
            Destroy(instantiatedPrefab);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MultiImageTracker))]
    class PrefabImagePairManagerInspector : Editor
    {
        List<XRReferenceImage> m_ReferenceImages = new List<XRReferenceImage>();
        bool m_IsExpanded = true;

        bool HasLibraryChanged(XRReferenceImageLibrary library)
        {
            if (library == null)
                return m_ReferenceImages.Count == 0;

            if (m_ReferenceImages.Count != library.count)
                return true;

            for (int i = 0; i < library.count; i++)
            {
                if (m_ReferenceImages[i] != library[i])
                    return true;
            }

            return false;
        }

        public override void OnInspectorGUI()
        {
            //customized inspector
            var behaviour = serializedObject.targetObject as MultiImageTracker;

            serializedObject.Update();
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            }

            var libraryProperty = serializedObject.FindProperty(nameof(m_ImageLibrary));
            EditorGUILayout.PropertyField(libraryProperty);
            var library = libraryProperty.objectReferenceValue as XRReferenceImageLibrary;

            //check library changes
            if (HasLibraryChanged(library))
            {
                if (library)
                {
                    var tempDictionary = new Dictionary<Guid, GameObject>();
                    foreach (var referenceImage in library)
                    {
                        tempDictionary.Add(referenceImage.guid, behaviour.GetPrefabForReferenceImage(referenceImage));
                    }
                    behaviour.m_PrefabsDictionary = tempDictionary;
                }
            }

            // update current
            m_ReferenceImages.Clear();
            if (library)
            {
                foreach (var referenceImage in library)
                {
                    m_ReferenceImages.Add(referenceImage);
                }
            }

            //show prefab list
            m_IsExpanded = EditorGUILayout.Foldout(m_IsExpanded, "Prefab List");
            if (m_IsExpanded)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUI.BeginChangeCheck();

                    var tempDictionary = new Dictionary<Guid, GameObject>();
                    foreach (var image in library)
                    {
                        var prefab = (GameObject)EditorGUILayout.ObjectField(image.name, behaviour.m_PrefabsDictionary[image.guid], typeof(GameObject), false);
                        tempDictionary.Add(image.guid, prefab);
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Update Prefab");
                        behaviour.m_PrefabsDictionary = tempDictionary;
                        EditorUtility.SetDirty(target);
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}


