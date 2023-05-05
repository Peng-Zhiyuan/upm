using UnityEngine;
using UnityEngine.UI;

// This file was auto generated by CodeLinker
// update time : 2023/03/23 16:11:51
public partial class LauncherSimpleItem : MonoBehaviour
{
	private Image _image_bg;
	protected Image Image_bg
	{
		get
		{
			if(_image_bg == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "$Image_bg");
				_image_bg = t.gameObject.GetComponent<Image>();
			}
			return _image_bg;
		}
	}

	private Image _image_icon;
	protected Image Image_icon
	{
		get
		{
			if(_image_icon == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "$image_icon");
				_image_icon = t.gameObject.GetComponent<Image>();
			}
			return _image_icon;
		}
	}

	private HorizontalLayoutGroup _itemGroup;
	protected HorizontalLayoutGroup ItemGroup
	{
		get
		{
			if(_itemGroup == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "$itemGroup");
				_itemGroup = t.gameObject.GetComponent<HorizontalLayoutGroup>();
			}
			return _itemGroup;
		}
	}

	private Text _text_count;
	protected Text Text_count
	{
		get
		{
			if(_text_count == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "$itemGroup/$text_count");
				_text_count = t.gameObject.GetComponent<Text>();
			}
			return _text_count;
		}
	}

}
