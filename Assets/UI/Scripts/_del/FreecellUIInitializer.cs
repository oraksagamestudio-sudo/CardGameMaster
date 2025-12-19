using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 통합 UI 레이아웃 & 카드 사이즈 계산 매니저
/// - SafeArea를 기준으로 루트 UI 크기 확보
/// - Tableau 8칸 너비로 카드 너비/높이 확정
/// - Freecell / Temp / Foundation 슬롯 일괄 사이즈 조정
/// - TableausArea는 Dock.Fill 역할 (Top + Bottom 제외 전체 영역)
/// - Classic/Plus 모드 구분 확장 고려
/// - Layout 이벤트 의존 최소화(프레임 지연 없음)
/// </summary>
public class FreecellUIInitializer : MonoBehaviour
{
    [Header("Core References")]
    [Tooltip("SafeAreaFitter가 적용된 루트 컨테이너")]
    public RectTransform safeArea;

    [Tooltip("상단 탑 UI (InfoBar + SlotArea)")]
    public RectTransform topArea;

    [Tooltip("하단 바텀 UI (Classic: 빈 공간 / Plus: 아이템 슬롯 바 영역 등)")]
    public RectTransform bottomArea;

    [Tooltip("8개 Tableau 컬럼이 들어가는 영역")]
    public RectTransform tableusArea;

    [Tooltip("Tableau 샘플러(8개 중 아무거나)")]
    public RectTransform tableuSampler;


    [Header("Slot References")]
    [Tooltip("Freecells / Temp / Foundation 슬롯들 전체")]
    public RectTransform[] slotRects;


    [Header("Card Layout Settings")]
    [Tooltip("카드 종횡비 (가로 1 : 세로 1.5)")]
    public float cardAspect = 1.5f;

    [Tooltip("카드 사이즈 변경이 필요할 때 이벤트로 Notify")]
    public static System.Action<Vector2> OnCardSizeChanged;


    /// <summary>
    /// 외부에서 SafeArea 변화/회전 등 발생 시 수동 호출 가능
    /// </summary>
    private void OnEnable()
    {
        StartCoroutine(LateRebuild());
    }

    private IEnumerator LateRebuild()
    {
        yield return new WaitForEndOfFrame();
        yield return null;

        ApplyLayout();
    }

    /// <summary>
    /// 레이아웃 전체 계산을 통합 제어하는 함수
    /// SafeArea → 카드사이즈 → 슬롯사이즈 → 테이블로 Fill 사이즈 순서로 처리
    /// </summary>
    public void ApplyLayout()
    {
        if (safeArea == null) return;

        // ============================================
        // 1. SafeArea 기준 전체 너비 확보
        // ============================================
        float rootWidth = safeArea.rect.width;
        float rootHeight = safeArea.rect.height;


        // ============================================
        // 2. 카드 크기 산출 (가로→세로)
        // ============================================
        float tableauWidth = tableuSampler.rect.width;
        float cardWidth = tableauWidth;
        float cardHeight = cardWidth * cardAspect;
        Vector2 cardSize = new(cardWidth, cardHeight);
        Debug.Log($"tableauWidth={tableauWidth}, cardsize={cardSize}");

        // Notify
        OnCardSizeChanged?.Invoke(cardSize);


        // ============================================
        // 3. 슬롯(Freecell/Temp/Foundation) 반영
        // SlotRects 배열은 GridLayoutGroup 또는 RectTransform 혼합 가능
        // ============================================
        foreach (var rt in slotRects)
        {
            if (rt == null) continue;

            var grid = rt.GetComponent<GridLayoutGroup>();
            if (grid != null)
            {
                grid.cellSize = cardSize;
                continue;
            }

            // 일반 슬롯일 경우 크기 직접 적용
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cardWidth);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cardHeight);
        }


        // ============================================
        // 4. TableausArea 높이 = SafeArea - Top - Bottom
        // TopArea/BottomArea의 Layout은 이미 SafeArea 안에서 계산 완료된 상태
        // ============================================
        float topHeight = topArea.rect.height;
        float bottomHeight = bottomArea.rect.height;

        float tableusHeight = Mathf.Max(0, rootHeight - topHeight - bottomHeight);

        tableusArea.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, tableusHeight);


        // ============================================
        // 5. UI 전체 즉시 재빌드 (순서 충돌 방지)
        // ※ Canvas.ForceUpdateCanvases()는 한 번으로 충분
        // ============================================
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(safeArea);
    }
}