using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TempCellController : SlotController
{

    private bool _isActivate = false;
    public bool IsActivate
    {
        set
        {
            if (Model != null)
            {
                var color = GetComponent<Image>().color;
                    color.a = value?1f :0f;
            } 
            _isActivate = value;
        }
        get
        {
            return _isActivate;
        }
    }

    /// <summary>
    /// 슬롯 레지스트리에 등록하면 자동으로 초기화함. 안쓸거면 슬롯레지스트리에서 빼면 됨.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="index"></param>
    public override void Init(SlotType type, int index)
    {
        IsActivate = true;
        base.Init(type, index);
    }

}
