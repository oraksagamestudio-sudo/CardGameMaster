//Assets/Scripts/GridLayoutGroupView.cs
using UnityEngine;
using UnityEngine.UI;
// #if UNITY_EDITOR
// [ExecuteAlways]
// #endif
[RequireComponent(typeof(GridLayoutGroup))]
public class GridLayoutGroupView : LayoutGroupView
{

    // private GridLayoutGroup _grid;
    // private RectTransform _rect;
    // private Vector2 _lastRectSize;
    // private bool _dirty = false;

//     private void Awake()
//     {
//         _grid = GetComponent<GridLayoutGroup>();
//         _rect = (RectTransform)transform;
//         _lastRectSize = _rect.rect.size;
//     }

//     private void OnEnable()
//     {
// #if UNITY_EDITOR
//         if (!Application.isPlaying)
//         {
//             UpdateCellSize();
//             return;
//         }
// #endif
//     }

//     public override void UpdateCellSize()
//     {
//         if (_grid == null)
//             _grid = GetComponent<GridLayoutGroup>();

//         if (CardSizeProvider.CardWidth <= 0f || CardSizeProvider.CardHeight <= 0f)
//             return;

//         _grid.cellSize = new Vector2(CardSizeProvider.CardWidth, CardSizeProvider.CardHeight);

// #if UNITY_EDITOR
//         if (!Application.isPlaying)
//         {
//             LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
//             _dirty = false;
//             return;
//         }
// #endif
//         _dirty = true;
//     }

    // private void OnRectTransformDimensionsChange()
    // {
    //     if (_rect == null)
    //         _rect = (RectTransform)transform;

    //     Vector2 currentSize = _rect.rect.size;
    //     if (currentSize == _lastRectSize)
    //         return;

    //     _lastRectSize = currentSize;
    //     UpdateCellSize();
    // }

    // public void LateUpdate()
    // {
    //     if (_dirty)
    //     {
    //         LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    //         _dirty = false;
    //     }
    // }

    



}
