using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GPC.SDK.Chat.VO;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using UnityEngine.EventSystems;

public partial class MiniChatView : MonoBehaviour, IPointerClickHandler
{
    public void Start()
    {
        this.DataList.ViewSetter = this.OnSetChatView;

        var mixConv = MixConversationManager.Stuff.MixConv;
        this.DataSource = mixConv;
    }



    void OnTotalUnreadCountChnaged()
    {
        this.RefreshUnreadCount();
    }

    [ShowInInspector, ReadOnly]
    GameChatConversation _dataSource;
    GameChatConversation DataSource
    {
        get
        {
            return _dataSource;
        }
        set
        {
            if(_dataSource == value)
            {
                return;
            }
            this.StopDataDriving();
            this._dataSource = value;
            this.StartDataDriving();
        }
    }

    void OnEnable()
    {
        this.StartDataDriving();
    }

    void OnDisable()
    {
        this.StopDataDriving();
    }

    void StopDataDriving()
    {
        if(this.DataSource != null)
        {
            this.DataSource.ChatInfoListChanged -= this.OnChatInfoListChanged;
            this.DataSource.UnreadCountChanged -= this.OnUnreadCountChnaged;
            GameConversationManager.Stuff.PrivateConversationTotalUnreadChnaged -= OnTotalUnreadCountChnaged;
        }
    }

    void StartDataDriving()
    {
        if (this.DataSource != null)
        {
            this.StopDataDriving();

            this.DataSource.ChatInfoListChanged += this.OnChatInfoListChanged;
            this.DataSource.UnreadCountChanged += this.OnUnreadCountChnaged;
            GameConversationManager.Stuff.PrivateConversationTotalUnreadChnaged += OnTotalUnreadCountChnaged;
        }
        this.Refresh();
    }

    void OnUnreadCountChnaged()
    {
        this.RefreshUnreadCount();
    }

    void OnSetChatView(object data, Transform viewTransform)
    {
        var chatInfo = data as ChatInfo;
        var view = viewTransform.GetComponent<MiniChatMessageView>();
        view.Set(chatInfo);
    }

    void OnChatInfoListChanged(ListChnagingType type, ChatInfo chatInfo)
    {
        this.Refresh();
    }

    void Refresh()
    {
        this.RebuildScrollView();
        this.RefreshUnreadCount();
    }

    void RefreshUnreadCount()
    {
        var count = this.DataSource?.UnreadCount ?? 0;
        this.MessageCountView.Count = count;
    }

    void RebuildScrollView()
    {
        var dataList = CreateData();
        this.DataList.DataList = dataList;
    }

    List<ChatInfo> tempList = new List<ChatInfo>();
    List<ChatInfo> CreateData()
    {
        tempList.Clear();
        if(this.DataSource != null)
        {
            var allList = this.DataSource.GetChatMessageList();
            var startIndex = allList.Count - 3;
            if (startIndex < 0)
            {
                startIndex = 0;
            }
            for (int i = startIndex; i < allList.Count; i++)
            {
                tempList.Add(allList[i]);
            }
        }
     
        return tempList;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        WwiseEventManager.SendEvent(TransformTable.UiControls, "ui_hallChat");
        UIEngine.Stuff.ForwardOrBackTo<ChatPageV2>();
    }
}
