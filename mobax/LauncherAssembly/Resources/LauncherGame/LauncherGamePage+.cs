using UnityEngine;
using UnityEngine.UI;

// This file was auto generated by CodeLinker
// update time : 2023/03/04 23:23:49
public partial class LauncherGamePage : MonoBehaviour
{
	private Image _button_skip;
	protected Image Button_skip
	{
		get
		{
			if(_button_skip == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "$Button_skip");
				_button_skip = t.gameObject.GetComponent<Image>();
			}
			return _button_skip;
		}
	}

	private LauncherCountdownBehaviour _txt_countDown;
	protected LauncherCountdownBehaviour Txt_countDown
	{
		get
		{
			if(_txt_countDown == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "Node_countDown/$Txt_countDown");
				_txt_countDown = t.gameObject.GetComponent<LauncherCountdownBehaviour>();
			}
			return _txt_countDown;
		}
	}

	private Text _txt_talk;
	protected Text Txt_talk
	{
		get
		{
			if(_txt_talk == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "Node_main/Node_talk/Node_talkFitter/$Txt_talk");
				_txt_talk = t.gameObject.GetComponent<Text>();
			}
			return _txt_talk;
		}
	}

	private RectTransform _node_customers;
	protected RectTransform Node_customers
	{
		get
		{
			if(_node_customers == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "Node_main/$Node_customers");
				_node_customers = t.gameObject.GetComponent<RectTransform>();
			}
			return _node_customers;
		}
	}

	private LauncherShadowProgressBar _progress_aim;
	protected LauncherShadowProgressBar Progress_aim
	{
		get
		{
			if(_progress_aim == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "Node_main/Node_aim/$Progress_aim");
				_progress_aim = t.gameObject.GetComponent<LauncherShadowProgressBar>();
			}
			return _progress_aim;
		}
	}

	private Text _txt_collect;
	protected Text Txt_collect
	{
		get
		{
			if(_txt_collect == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "Node_main/Node_aim/$Txt_collect");
				_txt_collect = t.gameObject.GetComponent<Text>();
			}
			return _txt_collect;
		}
	}

	private LauncherShadowProgressBar _progress_abandon;
	protected LauncherShadowProgressBar Progress_abandon
	{
		get
		{
			if(_progress_abandon == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "Node_main/Node_abandon/$Progress_abandon");
				_progress_abandon = t.gameObject.GetComponent<LauncherShadowProgressBar>();
			}
			return _progress_abandon;
		}
	}

	private Text _txt_abandon;
	protected Text Txt_abandon
	{
		get
		{
			if(_txt_abandon == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "Node_main/Node_abandon/$Txt_abandon");
				_txt_abandon = t.gameObject.GetComponent<Text>();
			}
			return _txt_abandon;
		}
	}

	private LauncherGameZone _gameZone;
	protected LauncherGameZone GameZone
	{
		get
		{
			if(_gameZone == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "Node_main/$GameZone");
				_gameZone = t.gameObject.GetComponent<LauncherGameZone>();
			}
			return _gameZone;
		}
	}

	private RectTransform _node_effects;
	protected RectTransform Node_effects
	{
		get
		{
			if(_node_effects == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "Node_main/$Node_effects");
				_node_effects = t.gameObject.GetComponent<RectTransform>();
			}
			return _node_effects;
		}
	}

	private Text _tip_loading;
	protected Text Tip_loading
	{
		get
		{
			if(_tip_loading == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "$tip_loading");
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
				var t = CodeLinkerUtil.FindByPath(transform, "offset/$progress_text");
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

	private AudioSource _audio_clear;
	protected AudioSource Audio_clear
	{
		get
		{
			if(_audio_clear == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "$Audio_clear");
				_audio_clear = t.gameObject.GetComponent<AudioSource>();
			}
			return _audio_clear;
		}
	}

	private AudioSource _audio_score;
	protected AudioSource Audio_score
	{
		get
		{
			if(_audio_score == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "$Audio_score");
				_audio_score = t.gameObject.GetComponent<AudioSource>();
			}
			return _audio_score;
		}
	}

	private AudioSource _audio_finish;
	protected AudioSource Audio_finish
	{
		get
		{
			if(_audio_finish == null)
			{
				var t = CodeLinkerUtil.FindByPath(transform, "$Audio_finish");
				_audio_finish = t.gameObject.GetComponent<AudioSource>();
			}
			return _audio_finish;
		}
	}

}