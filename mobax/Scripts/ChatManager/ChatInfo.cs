using GPC.SDK.Chat;
using GPC.SDK.Chat.VO;
using System;
using CustomLitJson;
using System.Threading.Tasks;
using GPC.Helper.Jingwei.Script.Translate;
using UnityEngine;
using Sirenix.OdinInspector;

public class ChatInfo
{
    public GameChatConversation conversation;
    public readonly ChatMessage rawMessage;

    public ChatInfo(ChatMessage rawMessage)
    {
        this.rawMessage = rawMessage;
    }

    public string SenderId
    {
        get
        {
            if(this.rawMessage.Sender == null)
            {
                return Database.Stuff.roleDatabase.Me._id;
            }
            return this.rawMessage.Sender.ID;
        }
    }

    public string SenderGuid
    {
        get
        {
            var roleInfo = ChatUtil.GetRoleInfoFromMeta(this);
            var guid = roleInfo.guid;
            return guid;
        }
    }


    [ShowInInspector]
    public string NativeContent
    {
        get
        {
            return rawMessage.Content;
        }
    }

    void AssetType(ChatInfoType type)
    {
        if(this.Type != type)
        {
            throw new Exception("[ChatInfo] type must be : " + type);
        }
    }

    /// <summary>
    /// 文本聊天的内容
    /// </summary>
    public string Text
    {
        get
        {
            this.AssetType(ChatInfoType.Text);
            var customInfo = this.GameMessage;
            var text = customInfo.text;
            return text;
        }
    }

    string _replacedText;
    /// <summary>
    /// 文本聊天的内容
    /// </summary>
    public string ReplacedText
    {
        get
        {
            if(_replacedText == null)
            {
                this.AssetType(ChatInfoType.Text);
                var customInfo = this.GameMessage;
                var text = customInfo.text;
                _replacedText = Badword.Replace(text);
            }
            return _replacedText;
        }
    }

    /// <summary>
    /// 表情
    /// </summary>
    public string Expression
    {
        get
        {
            this.AssetType(ChatInfoType.Expresion);
            var customInfo = this.GameMessage;
            var text = customInfo.expression;
            return text;
        }
    }

    /// <summary>
    /// 邀请的数据行 id
    /// </summary>
    public int InviteRowId
    {
        get
        {
            this.AssetType(ChatInfoType.Invite);
            var customInfo = this.GameMessage;
            var text = customInfo.inviteRowId;
            return text;
        }
    }

    /// <summary>
    /// 透传
    /// </summary>
    public string Extra
    {
        get
        {
            var customInfo = this.GameMessage;
            var text = customInfo.extra;
            return text;
        }
    }
    
    public string InvisibleContent
    {
        get
        {
            this.AssetType(ChatInfoType.Invisible);
            var customInfo = this.GameMessage;
            var text = customInfo.invisibleContent;
            return text;
        }
    }

    public async Task<string> GetTextOrTranslateAsync()
    {
        if(!this.useTranslate)
        {
            return this.ReplacedText;
        }
        else
        {
            var translate = await TranslateAsync();
            return translate;
        }
    }

    public bool useTranslate;
    string _translate;
    public Task<string> TranslateAsync()
    {
        var tcs = new TaskCompletionSource<string>();
        if(this._translate != null)
        {
            tcs.SetResult(this._translate);
            return tcs.Task;
        }

        var sorce = this.Text;
        Debug.Log("[ChatInfo] translate: " + sorce);
        TranslateHelper.SharedInstance().TranslateText(sorce, new TranslateHelper.OnTranslateResultListener(onTranslateSuccess : (string sourceText, string translatedText) =>
        {
            Debug.Log("[ChatInfo] translate result: " + translatedText);
            var finalText = Badword.Replace(translatedText);
            this._translate = finalText;
            tcs.SetResult(this._translate);

        }, onTranslateFailed: (string errorCode) =>
        {
            Debug.Log("[ChatInfo] translate fail: " + errorCode);
            tcs.SetResult("error: " + errorCode);
        }));
        return tcs.Task;
    }

    bool _gameMessageParsed;
    GameMessage _gameMessage;

    [ShowInInspector]
    public GameMessage GameMessage
    {
        get
        {
            if (!this._gameMessageParsed)
            {
                try
                {
                    var json = this.NativeContent;
                    this._gameMessage = JsonMapper.Instance.ToObject<GameMessage>(json);
                }
                catch
                {
                    this._gameMessage = null;
                }
                this._gameMessageParsed = true;
            }
            return this._gameMessage;
        }
    }


    [ShowInInspector]
    public ChatInfoType Type
    {
        get
        {
            var customInfo = this.GameMessage;
            if (customInfo == null)
            {
                return ChatInfoType.Crushed;
            }
            if (customInfo.type == GameMessageType.Text)
            {
                return ChatInfoType.Text;
            }
            else if (customInfo.type == GameMessageType.Expresion)
            {
                return ChatInfoType.Expresion;
            }
            else if (customInfo.type == GameMessageType.Invite)
            {
                return ChatInfoType.Invite;
            }
            else if (customInfo.type == GameMessageType.Invisible)
            {
                return ChatInfoType.Invisible;
            }
            return ChatInfoType.Crushed;
        }
    }
}
