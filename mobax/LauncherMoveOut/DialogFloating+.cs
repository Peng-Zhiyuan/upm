using UnityEngine.UI;
using UnityEngine;

// This file was auto generated by CodeLinker
// update time : 2023/02/14 22:48:08
public partial class DialogFloating : Floating
{
	private Text _text_content;
	protected Text Text_content
	{
		get
		{
			if(_text_content == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "root/$text_content");
				_text_content = t.gameObject.GetComponent<Text>();
			}
			return _text_content;
		}
	}

	private Text _text_title;
	protected Text Text_title
	{
		get
		{
			if(_text_title == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "root/$text_title");
				_text_title = t.gameObject.GetComponent<Text>();
			}
			return _text_title;
		}
	}

	private CanvasRenderer _group_ConfirmCancel;
	protected CanvasRenderer Group_ConfirmCancel
	{
		get
		{
			if(_group_ConfirmCancel == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "node1/$group_ConfirmCancel");
				_group_ConfirmCancel = t.gameObject.GetComponent<CanvasRenderer>();
			}
			return _group_ConfirmCancel;
		}
	}

	private CanvasRenderer _group_ConfirmOnly;
	protected CanvasRenderer Group_ConfirmOnly
	{
		get
		{
			if(_group_ConfirmOnly == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "node2/$group_ConfirmOnly");
				_group_ConfirmOnly = t.gameObject.GetComponent<CanvasRenderer>();
			}
			return _group_ConfirmOnly;
		}
	}

	private Image _group_NoButton;
	protected Image Group_NoButton
	{
		get
		{
			if(_group_NoButton == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "node3/$group_NoButton");
				_group_NoButton = t.gameObject.GetComponent<Image>();
			}
			return _group_NoButton;
		}
	}

}
