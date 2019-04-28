using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SortingOrder : MonoBehaviour
{
    public int order;
    public bool isUI = true;
    public string sortingLayerName = "Default";
    public bool isNeedRootSort;

    private int rorder;
    private Canvas parentCanvas;

    private int _order;
    private bool _isUI = true;
    private string _sortingLayerName = "Default";
    private bool _isNeedRootSort;

    private void RecordLastValue()
    {
        _order = rorder;
        _isUI = isUI;
        _sortingLayerName = sortingLayerName;
        _isNeedRootSort = isNeedRootSort;
    }

    private void GetRootOrder()
    {
        rorder = order;
        if (isNeedRootSort)
        {
            if (transform.parent)
            {
                var rootCanvas = transform.parent.GetComponentInParent<Canvas>();
                parentCanvas = rootCanvas;
                if (rootCanvas)
                {
                    var rootSortOrder = rootCanvas.sortingOrder;
                    rorder += rootSortOrder;
                }
            }
        }
    }

    private IEnumerator WaitForSort(Canvas canvasComponent)
    {
        yield return new WaitForEndOfFrame();
        if (isNeedRootSort)
        {
            Canvas rootCanvas = null;
            if (transform.parent)
            {
                rootCanvas = transform.parent.GetComponentInParent<Canvas>();
            }
            if (rootCanvas != parentCanvas)
            {
                parentCanvas = rootCanvas;
                if (rootCanvas)
                {
                    var rootSortOrder = rootCanvas.sortingOrder;
                    rorder = order + rootSortOrder;
                }
            }
        }
    }

    private void ResetOrder()
    {
        if (isUI)
        {
            Canvas canvasComponent = this.gameObject.GetComponent<Canvas>();
            if (canvasComponent == null)
            {
                canvasComponent = gameObject.AddComponent<Canvas>();
            }
            canvasComponent.overrideSorting = true;
            canvasComponent.sortingOrder = rorder;
            canvasComponent.sortingLayerName = sortingLayerName;
            StartCoroutine(WaitForSort(canvasComponent));
        }
        else
        {
            Renderer[] renders = GetComponentsInChildren<Renderer>();
            foreach (Renderer render in renders)
            {
                render.sortingOrder = rorder;
                render.sortingLayerName = sortingLayerName;
            }
            StartCoroutine(WaitForSort(null));
        }
    }

    public void ApplySortingOrder()
    {
        GetRootOrder();
        RecordLastValue();
        ResetOrder();
    }

    public void ApplyChildrenSortingOrder()
    {
        if (transform.parent)
        {
            SortingOrder parentSortingOrder = this.transform.parent.GetComponentInParent<SortingOrder>();
            if (parentSortingOrder)
            {
                return;
            }
        }

        SortingOrder[] childrenSortingOrder = GetComponentsInChildren<SortingOrder>();
        for (int i = 0; i < childrenSortingOrder.Length; i++)
        {
            childrenSortingOrder[i].ApplySortingOrder();
        }
    }

    void Start()
    {
        ApplyChildrenSortingOrder();
    }

    void OnEnable()
    {
        ApplyChildrenSortingOrder();
    }

    private bool IsDirty()
    {
        return rorder != _order || isUI != _isUI || sortingLayerName != _sortingLayerName || isNeedRootSort != _isNeedRootSort;
    }

    void Update()
    {
        if (IsDirty())
        {
            ApplyChildrenSortingOrder();
        }
    }
}
