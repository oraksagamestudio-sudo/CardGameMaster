//Assets/Scripts/StaticCardController.cs
using System.Collections;
using System.Collections.Generic;
using Solitaire;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(StaticCardView))]
public class StaticCardController : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CardModel model;
    private StaticCardView view;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private Vector3 _dragStartWorldPos;
    // private Transform _originalParent;
    private Canvas _rootCanvas;

    private SlotController _slot;
    private List<StaticCardController> _dragGroup;

    private List<Vector3> _initialOffsets;
    private Vector3 _pointerStartWorldPos;


    void OnEnable()
    {
        view = GetComponent<StaticCardView>();
        _rootCanvas = GetComponentInParent<Canvas>();
    }
    public void Init(CardSuit suit, CardRank rank, bool faceUp = true)
    {
        model = new CardModel(suit, rank, faceUp);
        view.SetCardData(model);
        view.OnCardSizeChanged();
    }
    public void Init(CardModel m)
    {
        model = m;
        view.SetCardData(m);
        view.OnCardSizeChanged();
    }

    public void MoveTo(Transform targetSlot, float duration = 0.25f)
    {
        var rt = (RectTransform)transform;
        CardMotionService.Instance.MoveToSlot(rt, targetSlot, duration);
    }

    // --------------------------------------------------------------------
    // CLICK
    // --------------------------------------------------------------------
    public void OnPointerClick(PointerEventData eventData)
    {
        var slot = transform.parent?.GetComponent<SlotController>();
        var game = GameContext.Classic;
        if (slot == null || game == null || game.State == null)
        {
            Debug.Log($"[CARD TOUCH] {model}");
            return;
        }

        int cardIndex = slot.GetCardIndex(this);
        if (cardIndex < 0)
        {
            Debug.Log($"[CARD TOUCH] {model}");
            return;
        }

        int depthFromTop = slot.Cards.Count - 1 - cardIndex; // 0 == top
        var destinations = new List<string>();

        foreach (var move in game.State.GetLegalMoves())
        {
            if (!MoveMatchesSlot(move, slot.Model))
                continue;

            int moveCount = move.Count > 0 ? move.Count : 1;
            if (depthFromTop >= moveCount)
                continue;

            var target = ResolveTargetSlot(move);
            var targetType = GetTargetSlotType(move);
            if (target != null)
                destinations.Add($"{target.Model.Type}[{target.Model.Index}]");
            else if (targetType.HasValue)
                destinations.Add($"{targetType.Value}[{move.To}]");
        }

        if (destinations.Count == 0)
        {
            Debug.Log($"[CARD TOUCH] {model} -> no legal destinations");
            return;
        }

        Debug.Log($"[CARD TOUCH] {model} -> {string.Join(", ", destinations)}");
    }

    // --------------------------------------------------------------------
    // DRAG START
    // --------------------------------------------------------------------
    public void OnBeginDrag(PointerEventData eventData)
    {
        _slot = transform.parent.GetComponent<SlotController>();
        if (_slot == null) return;

        int index = _slot.GetCardIndex(this);
        if (index < 0) return;

        _dragGroup = _slot.GetCascade(index);
        if (!CanStartDrag(_dragGroup))
        {
            _dragGroup = null;
            return;
        }

        // 1) 슬롯에서 제거 먼저
        foreach (var c in _dragGroup)
            _slot.RemoveCard(c);

        // 2) 드래그 레이어 이동
        foreach (var c in _dragGroup)
            c.transform.SetParent(DragLayerManager.Instance.Rect, true);

        // ⭐ 3) 이 시점에서 RaycastTarget OFF
        foreach (var c in _dragGroup)
            c.GetComponentInChildren<UnityEngine.UI.Image>().raycastTarget = false;

        // 4) 드래그 오프셋 계산
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            _rootCanvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out _pointerStartWorldPos
        );

        _initialOffsets = new List<Vector3>();
        foreach (var c in _dragGroup)
            _initialOffsets.Add(c.transform.position - _pointerStartWorldPos);

        foreach (var c in _dragGroup)
            c.view.SetDraggingVisual(true);
        // foreach (var c in _dragGroup)
        // {
        //     _initialOffsets.Add(c.transform.position - _pointerStartWorldPos);
        //     c.view.SetDraggingVisual(true);
        // }
    }

    // --------------------------------------------------------------------
    // DRAGGING
    // --------------------------------------------------------------------
    public void OnDrag(PointerEventData eventData)
    {
        if (_dragGroup == null || _dragGroup.Count == 0)
            return;

        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            _rootCanvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector3 worldPos
        );

        // 초기 오프셋 기반으로 정확한 드래그 위치 반영
        for (int i = 0; i < _dragGroup.Count; i++)
        {
            _dragGroup[i].transform.position = worldPos + _initialOffsets[i];
        }
    }

    // --------------------------------------------------------------------
    // DRAG END
    // --------------------------------------------------------------------
    // public void OnEndDrag(PointerEventData eventData)
    // {
    //     if (_dragGroup == null || _dragGroup.Count == 0)
    //         return;

    //     // ★ 변경된 부분: 오버랩 기반 슬롯 탐색
    //     var cardRT = _dragGroup[0].GetComponent<RectTransform>();
    //     SlotController targetSlot = FreecellClassicLayoutManager.Instance.GetBestSlot(cardRT);


    //     foreach (var c in _dragGroup)
    //         c.GetComponentInChildren<UnityEngine.UI.Image>().raycastTarget = true;

    //     if (targetSlot == null)
    //     {
    //         RestoreToOriginal();
    //         return;
    //     }

    //     // ★ ApplyMove 결과를 기반으로 성공/실패 판단
    //     bool ok = GameContext.ApplyMove(_slot, targetSlot, _dragGroup.Count);
    //     if (!ok)
    //     {
    //         RestoreToOriginal();
    //         return;
    //     }

    //     // ★ 그룹 전체 Tween 이동 방식 A
    //     CardMotionService.Instance.MoveGroupToSlot(
    //         _dragGroup,
    //         targetSlot.transform,
    //         0.25f
    //     );
    //     // // 성공 → UI 등록
    //     // var rt = (RectTransform)_dragGroup[0].transform;
    //     // CardMotionService.Instance.MoveToSlot(rt, targetSlot.transform);
    // }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (_dragGroup == null || _dragGroup.Count == 0)
            return;

        RectTransform cardRT = _dragGroup[0].GetComponent<RectTransform>();

        // Raycast 복원
        foreach (var c in _dragGroup)
            c.GetComponentInChildren<UnityEngine.UI.Image>().raycastTarget = true;

        SlotController targetSlot = null;

        // ★ 1단계: TableausArea 내부인가?
        var layout = FreecellClassicLayoutManager.Instance;

        if (layout.IsInsideTableausArea(cardRT))
        {
            // ★ Tableaus 전용 판정
            targetSlot = layout.GetNearestTableau(cardRT);
        }
        else
        {
            // ★ 기존 방식 유지
            targetSlot = FreecellClassicLayoutManager.Instance.GetBestSlot(cardRT);
        }

        if (targetSlot == null)
        {
            RestoreToOriginal();
            return;
        }

        // ★ ApplyMove로 룰 체크
        bool ok = GameContext.ApplyMove(_slot, targetSlot, _dragGroup.Count);
        if (!ok)
        {
            RestoreToOriginal();
            return;
        }

        // ★ 그룹 이동
        CardMotionService.Instance.MoveGroupToSlot(
            _dragGroup,
            targetSlot.transform,
            0.25f
        );
    }

    private static bool MoveMatchesSlot(Move move, SlotModel slotModel)
    {
        if (slotModel == null || move.From != slotModel.Index)
            return false;

        switch (slotModel.Type)
        {
            case SlotType.Tableau:
                return move.Kind == MoveKind.TableauToTableau
                    || move.Kind == MoveKind.TableauToCell
                    || move.Kind == MoveKind.TableauToFoundation;
            case SlotType.Freecell:
                return move.Kind == MoveKind.CellToTableau
                    || move.Kind == MoveKind.CellToFoundation;
            case SlotType.Foundation:
                return move.Kind == MoveKind.FoundationToTableau
                    || move.Kind == MoveKind.FoundationToCell;
            default:
                return false;
        }
    }

    private static SlotController ResolveTargetSlot(Move move)
    {
        var registry = SlotManager.Instance;
        if (registry == null)
            return null;

        switch (move.Kind)
        {
            case MoveKind.TableauToTableau:
            case MoveKind.CellToTableau:
            case MoveKind.FoundationToTableau:
                return SafeFetch(registry.Tableaus, move.To);
            case MoveKind.TableauToCell:
            case MoveKind.FoundationToCell:
                return SafeFetch(registry.Freecells, move.To);
            case MoveKind.TableauToFoundation:
            case MoveKind.CellToFoundation:
                return SafeFetch(registry.Foundations, move.To);
            default:
                return null;
        }
    }

    private static SlotController SafeFetch(List<SlotController> list, int index)
    {
        if (list == null)
            return null;

        return index >= 0 && index < list.Count ? list[index] : null;
    }

    private static SlotType? GetTargetSlotType(Move move)
    {
        switch (move.Kind)
        {
            case MoveKind.TableauToTableau:
            case MoveKind.CellToTableau:
            case MoveKind.FoundationToTableau:
                return SlotType.Tableau;
            case MoveKind.TableauToCell:
            case MoveKind.FoundationToCell:
                return SlotType.Freecell;
            case MoveKind.TableauToFoundation:
            case MoveKind.CellToFoundation:
                return SlotType.Foundation;
            default:
                return null;
        }
    }

    private void ApplyMoveToUI(SlotController target)
    {
        foreach (var c in _dragGroup)
            target.AddCard(c);

        FreecellClassicLayoutManager.Instance.UpdateLayout(target.transform);
    }

    // private void ApplyMove(SlotController target)
    // {
    //     var from = _slot.Model;
    //     var to = target.Model;

    //     int count = _dragGroup.Count;

    //     MoveKind mk = MoveKind.TableauToTableau;

    //     if (from.Type == SlotType.Tableau && to.Type == SlotType.Freecell)
    //         mk = MoveKind.TableauToCell;

    //     else if (from.Type == SlotType.Freecell && to.Type == SlotType.Tableau)
    //         mk = MoveKind.CellToTableau;

    //     else if (from.Type == SlotType.Tableau && to.Type == SlotType.Foundation)
    //         mk = MoveKind.TableauToFoundation;

    //     else if (from.Type == SlotType.Freecell && to.Type == SlotType.Foundation)
    //         mk = MoveKind.CellToFoundation;

    //     // 엔진 Move 시도
    //     bool ok = GameContext.Classic.Move(mk, from.Index, to.Index, count);

    //     if (!ok)
    //     {
    //         RestoreToOriginal();
    //         return;
    //     }

    //     // 성공 → 슬롯에 카드 등록
    //     foreach (var c in _dragGroup)
    //         target.AddCard(c);

    //     SlotLayoutService.Instance.UpdateLayout(target.transform);
    // }
    
    private void RestoreToOriginal()
    {
        foreach (var c in _dragGroup)
        {
            _slot.AddCard(c);
            c.GetComponentInChildren<UnityEngine.UI.Image>().raycastTarget = true;
        }

        FreecellClassicLayoutManager.Instance.UpdateLayout(_slot.transform);
    }

    private bool CanStartDrag(List<StaticCardController> group)
    {
        if (group == null || group.Count == 0)
            return false;

        if (_slot?.Model?.Type != SlotType.Tableau || group.Count == 1)
            return true;

        for (int i = 0; i < group.Count - 1; i++)
        {
            if (!FormsSequence(group[i], group[i + 1]))
                return false;
        }

        return true;
    }

    private static bool FormsSequence(StaticCardController lower, StaticCardController upper)
    {
        var lowerModel = lower?.model;
        var upperModel = upper?.model;
        if (lowerModel == null || upperModel == null)
            return false;

        bool altColor = IsRed(lowerModel.Suit) != IsRed(upperModel.Suit);
        bool rankDescending = (int)lowerModel.Rank == (int)upperModel.Rank + 1;
        return altColor && rankDescending;
    }

    private static bool IsRed(CardSuit suit)
    {
        return suit == CardSuit.Heart || suit == CardSuit.Diamond;
    }
    // private IEnumerator ReturnToOriginal()
    // {
    //     yield return StartCoroutine(
    //         CardMotionService.Instance.MoveCardWorld(
    //             (RectTransform)transform,
    //             _dragStartWorldPos,
    //             0.15f
    //         )
    //     );

    //     transform.SetParent(_originalParent, worldPositionStays: true);
    //     SlotLayoutService.Instance.UpdateLayout(_originalParent);
    // }

}
