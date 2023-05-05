using UnityEngine.UI;

// This file was auto generated by CodeLinker
// update time : 2023/03/29 18:24:29
public partial class LoadingPage : Page
{
	private Text _tip_loading;
	protected Text Tip_loading
	{
		get
		{
			if(_tip_loading == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "offset/$tip_loading");
				_tip_loading = t.gameObject.GetComponent<Text>();
			}
			return _tip_loading;
		}
	}

	private Text _progress_text;
	protected Text Progress_text
	{
		get
		{
			if(_progress_text == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "offset/Layout/$progress_text");
				_progress_text = t.gameObject.GetComponent<Text>();
			}
			return _progress_text;
		}
	}

	private SmoothProgressBar _progress;
	protected SmoothProgressBar Progress
	{
		get
		{
			if(_progress == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "offset/$Progress");
				_progress = t.gameObject.GetComponent<SmoothProgressBar>();
			}
			return _progress;
		}
	}

	private CsButtonView _button_cs;
	protected CsButtonView Button_cs
	{
		get
		{
			if(_button_cs == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "$button_cs");
				_button_cs = t.gameObject.GetComponent<CsButtonView>();
			}
			return _button_cs;
		}
	}

	private Text _customerService;
	protected Text CustomerService
	{
		get
		{
			if(_customerService == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "$button_cs/$CustomerService");
				_customerService = t.gameObject.GetComponent<Text>();
			}
			return _customerService;
		}
	}

	private Text _label_version;
	protected Text Label_version
	{
		get
		{
			if(_label_version == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "$label_version");
				_label_version = t.gameObject.GetComponent<Text>();
			}
			return _label_version;
		}
	}

	private Image _comp_Mask;
	protected Image Comp_Mask
	{
		get
		{
			if(_comp_Mask == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "$Comp_Mask");
				_comp_Mask = t.gameObject.GetComponent<Image>();
			}
			return _comp_Mask;
		}
	}

	private Text _tip;
	protected Text Tip
	{
		get
		{
			if(_tip == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "$tip");
				_tip = t.gameObject.GetComponent<Text>();
			}
			return _tip;
		}
	}

}
