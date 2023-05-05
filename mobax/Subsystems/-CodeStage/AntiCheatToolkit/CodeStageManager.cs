using UnityEngine;
using System.Collections.Generic;
// If you're going to use any obscured types / prefs from code you'll need to use this name space:
using CodeStage.AntiCheat.ObscuredTypes;
using System;
// If you're going to use detectors from code you'll need to use this name space:
using CodeStage.AntiCheat.Detectors;
public class CodeStageManager : Single<CodeStageManager> 
{
	public ObscuredInt cheat_count=0;
	private void OnCheatingAction()
	{
		cheat_count++;
		Debug.Log("OnCheatingAction");
		throw new Exception("Memory exception!");
	
	}

	public void StartDetection()
	{
		ObscuredCheatingDetector.StartDetection(OnCheatingAction);
	}

	public void StopDetection()
	{
		ObscuredCheatingDetector.StopDetection();
	}


    //定期更新内存中的密钥，防止三个参数同时被锁定，默认可以不开启
    public async void LoopRefreshMemorySecret(int seconds)
	{
		//int t = UnityEngine.Random.Range(10, 20);
		await TimerMgr.Instance.DelaySecondAsync(seconds);
		RefreshMemorySecret();
	}
	private int lastKey=0;
	private  void  RefreshMemorySecret()
	{
		if(ObscuredInt.CryptoKey == lastKey 
		|| ObscuredLong.CryptoKey == lastKey 
		|| ObscuredFloat.CryptoKey == lastKey 
		|| ObscuredDouble.CryptoKey == lastKey)
		{
			//secret被锁定了
			OnCheatingAction();
		}

		lastKey = ObscuredInt.CryptoKey;

		int newkey =  0;
		do
		{
			newkey = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
		}while(newkey==lastKey);

		ObscuredInt.SetNewCryptoKey(newkey);
		ObscuredFloat.SetNewCryptoKey(newkey);
		ObscuredLong.SetNewCryptoKey(newkey);
		ObscuredDouble.SetNewCryptoKey(newkey);
		ObscuredBool.SetNewCryptoKey((byte)newkey);
	}
		
}

