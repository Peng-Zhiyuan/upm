using UnityEngine;
using UnityEngine.UI;

public class SpriteAnim:MonoBehaviour{
	private float mTime;
	public bool mActive; 
	public float mFrameTime = 0.2f;
	private bool mReverse;
	private int mCurIndex;
	private int mRenderIndex = -1;
	public int mBeginIndex;
	public int mEndIndex;
	public bool mLoop = true;

	
	public string mImagePreName;
	private Image mImage;
	public string mPath;
    public int mFormatIndex = 1;
	//public Color color;
	public int mSubLoopCount = 0;
	public int mSubBeginIndex = 0;
	public int mSubEndIndex = 0;
	private int subLoopCount = 0;
	private AnimEnd mEndCallback;
	public delegate void AnimEnd(string msg);
    public Image SelfImage
    {
        get 
        {
            if (mImage == null)
            {
                mImage = gameObject.GetComponent<Image>();
            }
            return mImage;
 
         }
    }

	// void OnEnable()
	// {
	// 	this.color = ColorHelper.GetColorFrom16("e5f2ff");
	// 	this.SelfImage.color = this.color ;
	// }
    public bool Active
	{
		get
		{
			return mActive;
		}
		set
		{
			if(value)
			{
                if (SelfImage != null)
                {
                    mCurIndex = mBeginIndex;
                    RenderIndex = mBeginIndex;
                    mImage.enabled = true;
                    mActive = true;
					subLoopCount = 0 ;
                }
			}
			else 
			{
				if(SelfImage != null)mImage.enabled = value;
				mActive = value;
			}
		
		}
	}

    public string SpriteFullName
    {
        get 
        {
		
            return this.mImagePreName + string.Format("{0:D"+mFormatIndex+"}", mRenderIndex);
         }
    }

    public void SetInfo(string path, string imagePreName, int begin, int end,  bool loop, int frameRate, int formatIndex , AnimEnd EndCallback)
	{   
		Active = false;
		mReverse = false;
		mPath = path;
		mImagePreName = imagePreName;
		mBeginIndex = begin;
		mEndIndex = end;
		mFrameTime = 1/(float)frameRate;
		mLoop = loop;
		mFormatIndex = formatIndex;
		mEndCallback = EndCallback;
	}
	
	private int RenderIndex
	{
		get
		{
			return  mRenderIndex;
		}
		set
		{
			if(value !=  mRenderIndex)
			{
				mRenderIndex = value;
                if(SelfImage != null)
                {
					//Debug.LogError("SpriteFullName:"+SpriteFullName);
					//var name = mPath + "/" + SpriteFullName;
					//var sprite = AddressableRes.Get<Sprite>(name);
				    //var sprite = AssetCacher.Stuff.Get<Sprite>(SpriteFullName);

					var bucket = BucketManager.Stuff.GetBucket(UIEngine.LatestNavigatePageName);
					var sprite = bucket.Get<Sprite>(SpriteFullName);

					SelfImage.sprite = sprite;
					SelfImage.SetNativeSize();
                }
               
			}
		}
		
	}
	
	public void Update()
	{
		
		if(mActive)
		{
			mTime += Time.deltaTime;
			if(mTime >= mFrameTime)
			{
				mTime = 0.0f;
				if(mReverse)
				{
				
					if(subLoopCount < mSubLoopCount && this.mCurIndex == this.mSubBeginIndex)
					{
						this.mCurIndex = this.mSubEndIndex;
						this.subLoopCount++;
					}
					else mCurIndex--;
					if(mCurIndex < mBeginIndex)
					{
						if(mLoop)
						{
							mCurIndex = mEndIndex;
							this.subLoopCount = 0;
						}
						else
						{
							mCurIndex = mBeginIndex;
							if(mEndCallback != null)mEndCallback("");
							Active = false;
							//return;
						}
					}						
				}
				else
				{
					if(subLoopCount < mSubLoopCount && this.mCurIndex == this.mSubEndIndex)
					{
						this.mCurIndex = this.mSubBeginIndex;
						this.subLoopCount++;
					}
					else mCurIndex++;
					if(mCurIndex > mEndIndex)
					{
						if(mLoop)
						{
							mCurIndex = mBeginIndex;
							this.subLoopCount = 0;
						}
						else
						{
							mCurIndex = mEndIndex;
							if(mEndCallback != null)mEndCallback("");
							Active = false;
							//return;
						}
					}							
				}
				RenderIndex = mCurIndex;
				//flip_check();
			}
		}
		
	}
}
