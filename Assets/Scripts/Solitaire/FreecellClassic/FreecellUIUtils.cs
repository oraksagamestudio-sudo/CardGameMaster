using System.Collections.Generic;

// public static class CardSizeProvider
// {
//     public static float CardWidth;
//     public static float CardHeight;
//     public static float CardGap;

//     public static void Init(float cardWidth, float cardHeight, float cardGap)
//     {
//         CardWidth = cardWidth;
//         CardHeight = cardHeight;
//         CardGap = cardGap;
//     }
// }

// public static class UILayoutBroadcaster
// {
//     private static List<StaticCardView> cardViewListeners = new();
//     private static List<SlotView> slotViewListeners = new();
//     private static List<LayoutGroupView> layoutGroupListener = new();
//     public static void Register(StaticCardView view)
//     {
//         if (!cardViewListeners.Contains(view))
//             cardViewListeners.Add(view);
//     }
//     public static void Register(SlotView view)
//     {
//         if (!slotViewListeners.Contains(view))
//             slotViewListeners.Add(view);
//     }
//     public static void Register(LayoutGroupView view)
//     {
//         if (!layoutGroupListener.Contains(view))
//             layoutGroupListener.Add(view);
//     }

//     public static void Broadcast()
//     {
//         cardViewListeners.RemoveAll(v => v == null);
//         slotViewListeners.RemoveAll(v => v == null);
//         layoutGroupListener.RemoveAll(v => v == null);

//         foreach (var v in cardViewListeners)
//             v.OnCardSizeChanged();
//         foreach (var v in slotViewListeners)
//             v.OnSlotSizeChanged();
//         foreach (var v in layoutGroupListener)
//             v.OnCellSizeChanged();

//         if (SlotLayoutService.Instance != null)
//             SlotLayoutService.Instance.UpdateAllSlots();
//     }
// }