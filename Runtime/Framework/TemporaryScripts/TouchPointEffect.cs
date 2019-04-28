using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TouchPointEffect : MonoBehaviour
{
    public bool _isUseParticle = false;
    public GameObject _particleRoot;
    public GameObject _particleInstance;
    public GameObject _imageRoot;
    public GameObject _imageInstance;
    public bool _isShowTrail = false;
    public GameObject _trailPrefab;
    public int _trailCount = 10;
    public float _trailCheckInterval = 0.1f;

    private GameObject[] _trailCache;
    private int _trailCurrentIndex = 1;
    private float _trailCheckTimer = 0;
    private float _timeAtLastFrame;
    private ParticleSystem[] _particleSystem;
    private float _deltaTime;

    private int NextTrailUseIndex
    {
        get
        {
            return (_trailCurrentIndex + 1) % _trailCount; 
        }
    }

    void Awake()
    {
        Capstones.UnityFramework.ResManager.DontDestroyOnLoad(gameObject);
        Capstones.UnityFramework.ResManager.CanDestroyAll(gameObject);

        _trailCache = new GameObject[_trailCount];
        for (int i = 0; i < _trailCount; i++)
        {
            _trailCache[i] = Instantiate(_trailPrefab) as GameObject;
            _trailCache[i].transform.SetParent(_imageRoot.transform, false);
            _trailCache[i].FastSetActive(false);
        }
    }

    void Start()
    {
        if (_isUseParticle)
        {
            _particleInstance.FastSetActive(false);
            _timeAtLastFrame = Time.realtimeSinceStartup;
            _particleSystem = gameObject.GetComponentsInChildren<ParticleSystem>(true);
            _particleRoot.FastSetActive(true);
            _imageRoot.FastSetActive(false);
        }
        else
        {
            _particleRoot.FastSetActive(false);
            _imageRoot.FastSetActive(true);
        }
    }

    void UpdateClickEffect()
    {
        bool isTouch = false;
        Vector3 touchPosition = Vector3.zero;
        if (Input.GetMouseButtonDown(0))
        {
            isTouch = true;
            touchPosition = Input.mousePosition;
        }
        if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            isTouch = true;
            touchPosition = Input.GetTouch(0).position;
        }

        if (!isTouch) return;

        if (_isUseParticle)
        {
            _particleInstance.GetComponent<RectTransform>().anchoredPosition3D = touchPosition;
            _particleInstance.FastSetActive(false);
            _particleInstance.FastSetActive(true);
        }
        else
        {
            _imageInstance.GetComponent<RectTransform>().position = touchPosition;
            _imageInstance.FastSetActive(false);
            _imageInstance.FastSetActive(true);
        }

        if (!_isUseParticle) return;

        _deltaTime = Time.realtimeSinceStartup - _timeAtLastFrame;
        _timeAtLastFrame = Time.realtimeSinceStartup;
        if (Time.timeScale <= 0.01)
        {
            for (int i = 0; i < _particleSystem.Length; i++)
            {
                _particleSystem[i].Simulate(_deltaTime, false, false);
                _particleSystem[i].Play();
            }
        }
    }

    void UpdateTrailEffect()
    {
        if (!_isShowTrail) return;

        if (_trailCheckTimer < _trailCheckInterval)
        {
            _trailCheckTimer += Time.unscaledDeltaTime;
            return;
        }

        _trailCheckTimer = 0;
        bool isTouch = false;
        Vector3 touchPosition = Vector3.zero;

        if (Input.GetMouseButton(0))
        {
            isTouch = true;
            touchPosition = Input.mousePosition;
        }
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            isTouch = true;
            touchPosition = Input.GetTouch(0).position;
        }

        if (!isTouch) return;

        _trailCurrentIndex = NextTrailUseIndex;
        _trailCache[_trailCurrentIndex].GetComponent<RectTransform>().position = touchPosition;
        _trailCache[_trailCurrentIndex].FastSetActive(false);
        _trailCache[_trailCurrentIndex].FastSetActive(true);
    }

    void Update()
    {
        UpdateClickEffect();
        UpdateTrailEffect();
    }
}
