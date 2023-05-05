﻿#if AK_WWISE_ADDRESSABLES && UNITY_ADDRESSABLES

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AK.Wwise.Unity.WwiseAddressables
{
	public class AkAddressableBankManager
	{

		private ConcurrentDictionary<string, WwiseAddressableSoundBank> m_AddressableBanks =
			new ConcurrentDictionary<string, WwiseAddressableSoundBank>();

		private ConcurrentDictionary<string, string> m_banksToUnload =
			new ConcurrentDictionary<string, string>();

		private ConcurrentDictionary<uint, EventContainer> m_eventsToFireOnBankLoad =
			new ConcurrentDictionary<uint, EventContainer>();

		public const uint INVALID_SOUND_BANK_ID = 0;

		private WwiseAddressableSoundBank initBank;
		private WwiseAddressableSoundBank InitBank
		{
			get
			{
				if (initBank == null)
				{
					initBank = FindInitBank();
				}
				return initBank;
			}
		}
		private static AkAddressableBankManager instance;
		public static AkAddressableBankManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new AkAddressableBankManager();
				}
				return instance;
			}
			private set { Instance = value; }
		}

		private static WwiseAddressableSoundBank FindInitBank()
		{
			var foundBank = UnityEngine.MonoBehaviour.FindObjectsOfType<InitBankHolder>();
			if (foundBank.Count() == 0)
			{
				UnityEngine.Debug.LogError("Wwise Addressables : There is no InitBankHolder in the scene, please add one for Wwise to function properly.");
				return null;
			}

			if (foundBank.Count() > 1)
			{
				UnityEngine.Debug.LogError("Wwise Addressables : There is more than one InitBankHolder in the scene, which is not recommended.");
			}

			if (foundBank[0].InitBank == null)
			{
				UnityEngine.Debug.LogError("Wwise Addressables : The InitBankHolder does not hold a valid reference to the Init bank.");
				return null;

			}
			return foundBank[0].InitBank;
		}

		struct EventContainer
		{
			public string eventName;
			public object eventObject;
			public string methodName;
			public object[] methodArgs;
			public Type[] methodArgTypes;
		}

		bool InitBankLoaded
		{
			get { return (InitBank != null && InitBank.LoadState == BankLoadState.Loaded); }
		}

		public void UnloadAllBanks(bool clearBankDictionary = true)
		{
			foreach (var bank in m_AddressableBanks.Values)
			{
				UnloadBank(bank, ignoreRefCount: true, removeFromBankDictionary: false); ;
			}
			if (clearBankDictionary)
			{
				m_AddressableBanks.Clear();
			}

		}

		public void ReloadAllBanks()
		{
			var m_banksToReload = new ConcurrentDictionary<string, WwiseAddressableSoundBank>(m_AddressableBanks);
			UnloadAllBanks();
			UnloadInitBank();
			LoadInitBank();

			foreach (var bank in m_banksToReload.Values)
			{
				LoadBank(bank, bank.decodeBank, bank.saveDecodedBank);
			}
		}

		public void SetLanguageAndReloadLocalizedBanks(string language)
		{
			var banksToReload = new List<WwiseAddressableSoundBank>();
			foreach (var bank in m_AddressableBanks.Values)
			{
				if (bank.currentLanguage == "SFX" || bank.currentLanguage == language)
					continue;
				banksToReload.Add(bank);
			}
			if (banksToReload.Count == 0)
			{
				return;
			}
			foreach (var bank in banksToReload)
			{
				UnloadBank(bank, ignoreRefCount: true, removeFromBankDictionary: true);
			}
			UnloadInitBank();
			AkSoundEngine.SetCurrentLanguage(language);
			AkSoundEngine.RenderAudio();
			LoadInitBank();

			foreach (var bank in banksToReload)
			{
				LoadBank(bank, bank.decodeBank, bank.saveDecodedBank);
			}
		}

		public void LoadInitBank()
		{
			LoadBank(InitBank, addToBankDictionary: false);
		}

		public void UnloadInitBank()
		{
			UnloadBank(InitBank, ignoreRefCount: true, removeFromBankDictionary: false);
		}

		//Todo : support decoding banks and saving decoded banks
		public void LoadBank(WwiseAddressableSoundBank bank, bool decodeBank = false, bool saveDecodedBank = false, bool addToBankDictionary = true, WwiseSoundBankAsset wwiseData = null)
		{
			bank.decodeBank = decodeBank;
			bank.saveDecodedBank = saveDecodedBank;
			if (m_AddressableBanks.ContainsKey(bank.name))
			{
				m_AddressableBanks.TryGetValue(bank.name, out bank);
			}
			else if (addToBankDictionary)
			{
				m_AddressableBanks.TryAdd(bank.name, bank);
			}

			if (bank.LoadState == BankLoadState.Unloaded || bank.LoadState == BankLoadState.WaitingForInitBankToLoad)
			{
				if (!InitBankLoaded && bank.name != "Init")
				{
					UnityEngine.Debug.Log($"Wwise Addressable Bank Manager: {bank.name} bank will be loaded after the init bank is loaded");
					bank.LoadState = BankLoadState.WaitingForInitBankToLoad;
					return;
				}
			}
			if (bank.LoadState == BankLoadState.Loading)
			{
				bank.refCount += 1;
				return;
			}

			if (bank.LoadState == BankLoadState.Loaded)
			{
				bank.refCount += 1;
				return;
			}

			bank.refCount += 1;
			bank.LoadState = BankLoadState.Loading;

			if (bank.Data == null)
			{
				UnityEngine.Debug.LogError($"Wwise Addressable Bank Manager : {bank.name} could not be loaded - Bank reference not set");
				m_AddressableBanks.TryRemove(bank.name, out _);
				return;
			}

			AssetReferenceWwiseBankData bankData;
			if (bank.Data.ContainsKey("SFX"))
			{
				UnityEngine.Debug.Log($"Wwise Addressable Bank Manager: Loading {bank.name} bank");
				bankData = bank.Data["SFX"];
				bank.currentLanguage = "SFX";
			}
			else
			{
				var currentLanguage = AkSoundEngine.GetCurrentLanguage();
				if (bank.Data.ContainsKey(currentLanguage))
				{
					bankData = bank.Data[currentLanguage];
					bank.currentLanguage = currentLanguage;
					UnityEngine.Debug.Log($"Wwise Addressable Bank Manager: Loading {bank.name} - {currentLanguage}");
				}
				else
				{
					UnityEngine.Debug.LogError($"Wwise Addressable Bank Manager: {bank.name} could not be loaded in {currentLanguage} language ");
					m_AddressableBanks.TryRemove(bank.name, out _);
					return;
				}
			}
#if UNITY_EDITOR
			if (AkBasePathGetter.IsSettingWwisePath()) 
			{
				LoadBankEditorAsync(bank, bank.wwisebankData);
			}
            else
#endif
			{ 
				LoadBankAsync(bank, bankData); 
			}
//#if UNITY_EDITOR
//			LoadBankEditorAsync(bank, bank.wwisebankData);
//#else
//			LoadBankAsync(bank, bankData);
//#endif

		}

		public async void LoadBankAsync(WwiseAddressableSoundBank bank, AssetReferenceWwiseBankData bankData)
		{
			var AsyncHandle = bankData.LoadAssetAsync();
			await AsyncHandle.Task;

			if (AsyncHandle.IsValid() && AsyncHandle.Status == AsyncOperationStatus.Succeeded)
			{
				bank.eventNames = new HashSet<string>(AsyncHandle.Result.eventNames);
				var data = AsyncHandle.Result.RawData;
				bank.GCHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
				var result = AkSoundEngine.LoadBankMemoryCopy(bank.GCHandle.AddrOfPinnedObject(), (uint)data.Length, out uint bankID);
				if (result == AKRESULT.AK_Success)
				{
					bank.soundbankId = bankID;
					bank.LoadState = BankLoadState.Loaded;
				}
				else
				{
					bank.soundbankId = INVALID_SOUND_BANK_ID;
					bank.LoadState = BankLoadState.LoadFailed;
					UnityEngine.Debug.Log($"Wwise Addressable Bank Manager : Sound Engine failed to load {bank.name} SoundBank");
				}
				bank.GCHandle.Free();
				Addressables.Release(AsyncHandle);

				string destinationDir;
				if (bank.StreamingMedia != null)
				{
					foreach (var language in bank.StreamingMedia.Keys)
					{
						if (language == "SFX")
						{
							destinationDir = UnityEngine.Application.persistentDataPath; ;
						}
						else
						{
							destinationDir = Path.Combine(UnityEngine.Application.persistentDataPath, language);
						}

						if (!Directory.Exists(destinationDir))
						{
							Directory.CreateDirectory(destinationDir);
						}

						foreach (var streamedAsset in bank.StreamingMedia[language].media)
						{
							if (streamedAsset == null)
							{
								UnityEngine.Debug.LogError($"Wwise Addressable Bank Manager: Streaming media asset referenced in {bank.name} SoundBank is null");
								continue;
							}
							var loadHandle = streamedAsset.LoadAssetAsync();
							await loadHandle.Task;

							if (loadHandle.IsValid() && loadHandle.Status == AsyncOperationStatus.Succeeded)
							{
								AkAssetUtilities.UpdateWwiseFileIfNecessary(destinationDir, loadHandle.Result);
								Addressables.Release(loadHandle);
							}
							else
							{
								UnityEngine.Debug.LogError($"Wwise Addressable Bank Manager: Failed to load streaming media asset {streamedAsset.id} in {bank.name} SoundBank");
							}
						}
					}
				}
			}
			else
			{
				UnityEngine.Debug.Log($"Wwise Addressable Bank Manager : Failed to load {bank.name} SoundBank");
				bank.LoadState = BankLoadState.LoadFailed;
				Addressables.Release(AsyncHandle);
			}

			//Make sure the asset is cleared from memory
			UnityEngine.Resources.UnloadUnusedAssets();
			OnBankLoaded(bank);
		}

		public async void LoadBankEditorAsync(WwiseAddressableSoundBank bank, WwiseSoundBankAsset asset)
		{
			//var AsyncHandle = bankData.LoadAssetAsync();
			//await AsyncHandle.Task;
				var Result = asset;
			
				bank.eventNames = new HashSet<string>(Result.eventNames);
				var data = Result.RawData;
				bank.GCHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
				var result = AkSoundEngine.LoadBankMemoryCopy(bank.GCHandle.AddrOfPinnedObject(), (uint)data.Length, out uint bankID);
				if (result == AKRESULT.AK_Success)
				{
					bank.soundbankId = bankID;
					bank.LoadState = BankLoadState.Loaded;
				}
				else
				{
					bank.soundbankId = INVALID_SOUND_BANK_ID;
					bank.LoadState = BankLoadState.LoadFailed;
					UnityEngine.Debug.Log($"Wwise Addressable Bank Manager : Sound Engine failed to load {bank.name} SoundBank");
				}
				bank.GCHandle.Free();
				//Addressables.Release(AsyncHandle);

				string destinationDir;
				if (bank.StreamingMedia != null)
				{
					foreach (var language in bank.StreamingMedia.Keys)
					{
						if (language == "SFX")
						{
							destinationDir = UnityEngine.Application.persistentDataPath; ;
						}
						else
						{
							destinationDir = Path.Combine(UnityEngine.Application.persistentDataPath, language);
						}

						if (!Directory.Exists(destinationDir))
						{
							Directory.CreateDirectory(destinationDir);
						}

						foreach (var streamedAsset in bank.StreamingMedia[language].media)
						{
							if (streamedAsset == null)
							{
								UnityEngine.Debug.LogError($"Wwise Addressable Bank Manager: Streaming media asset referenced in {bank.name} SoundBank is null");
								continue;
							}
							var loadHandle = streamedAsset.LoadAssetAsync();
							await loadHandle.Task;

							if (loadHandle.IsValid() && loadHandle.Status == AsyncOperationStatus.Succeeded)
							{
								AkAssetUtilities.UpdateWwiseFileIfNecessary(destinationDir, loadHandle.Result);
								Addressables.Release(loadHandle);
							}
							else
							{
								UnityEngine.Debug.LogError($"Wwise Addressable Bank Manager: Failed to load streaming media asset {streamedAsset.id} in {bank.name} SoundBank");
							}
						}
					}
				}

			//Make sure the asset is cleared from memory
			UnityEngine.Resources.UnloadUnusedAssets();
			OnBankLoaded(bank);
		}

		public void UnloadBank(WwiseAddressableSoundBank bank, bool ignoreRefCount = false, bool removeFromBankDictionary = true)
		{
			if (!ignoreRefCount)
			{
				bank.refCount = Math.Max(0, bank.refCount - 1);
				if (bank.refCount != 0)
				{
					return;
				}
			}

			if (bank.LoadState == BankLoadState.Loading)
			{
				UnityEngine.Debug.Log($"Wwise Addressable Bank Manager: {bank.name} will be unloaded after it is done loading");
				m_banksToUnload.TryAdd(bank.name, bank.name);
				return;
			}

			if (bank.LoadState == BankLoadState.Loaded)
			{
				UnityEngine.Debug.Log($"Wwise Addressable Bank Manager: Unloading {bank.name} sound bank - Bank ID : {bank.soundbankId}");
				AkSoundEngine.UnloadBank(bank.soundbankId, System.IntPtr.Zero);

			}

			m_banksToUnload.TryRemove(bank.name, out _);
			bank.soundbankId = 0;
			bank.refCount = 0;
			bank.LoadState = BankLoadState.Unloaded;

			if (removeFromBankDictionary)
			{
				if (!m_AddressableBanks.TryRemove(bank.name, out _))
				{
#if UNITY_EDITOR
					// Don't unnecessarily log messages when caused by domain reload
					if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode && !UnityEditor.EditorApplication.isPlaying)
					{
						return;
					}
#endif
					if (InitBank && bank.name != InitBank.name)
					{
						UnityEngine.Debug.LogError($"Wwise Addressable Bank Manager: Unloaded {bank.name}, but it was not in the list of loaded banks");
					}
				}
			}
		}

		public bool LoadedBankContainsEvent(string eventName, uint eventId, object eventObject, string methodName, Type[] methodArgTypes, object[] methodArgs)
		{
			foreach (var bank in m_AddressableBanks.Values)
			{
				if (bank.LoadState == BankLoadState.Loaded && bank.eventNames.Contains(eventName))
				{
					return true;
				}
			}

			UnityEngine.Debug.LogWarning($"Wwise Addressables : {eventName} will be delayed, because its soundbank has not been loaded.");
			m_eventsToFireOnBankLoad.TryAdd(eventId, new EventContainer { eventName = eventName, eventObject = eventObject, methodName = methodName, methodArgTypes = methodArgTypes, methodArgs = methodArgs });
			return false;
		}

		private Type m_AkEventType;
		private Type EventType
		{
			get
			{
				if (m_AkEventType == null)
				{
					var assembly = Assembly.Load("AK.Wwise.Unity.API.WwiseTypes");
					m_AkEventType = assembly.GetType("AK.Wwise.Event");
				}
				return m_AkEventType;
			}
		}

		private void OnBankLoaded(WwiseAddressableSoundBank bank)
		{
			if (bank.LoadState == BankLoadState.Loaded)
			{
				UnityEngine.Debug.Log($"Wwise Addressable Bank Manager : Loaded {bank.name} bank -  Bank ID : {bank.soundbankId}");

				if (InitBankLoaded && bank.name == InitBank.name)
				{
					foreach (var b in m_AddressableBanks.Values)
					{
						if (b.LoadState == BankLoadState.WaitingForInitBankToLoad)
						{
							LoadBank(b, b.decodeBank, b.saveDecodedBank);
						}
					}
				}

				//Fire any events that were waiting on the bank load
				var eventsToRemove = new List<uint>();
				foreach (var e in m_eventsToFireOnBankLoad)
				{
					if (bank.eventNames.Contains(e.Value.eventName))
					{
						UnityEngine.Debug.Log($"Wwise Addressable Bank Manager: Triggering delayed event {e.Value.eventName}");
						MethodInfo handleEvent = EventType.GetMethod(e.Value.methodName, e.Value.methodArgTypes);
						handleEvent.Invoke(e.Value.eventObject, e.Value.methodArgs);
						eventsToRemove.Add(e.Key);
					}
				}

				foreach (var e in eventsToRemove)
				{
					m_eventsToFireOnBankLoad.TryRemove(e, out _);
				}
			}

			//Reset bank state if load failed
			if (bank.loadState == BankLoadState.LoadFailed)
			{
				UnloadBank(bank, ignoreRefCount : true);
			}
			
			if (m_banksToUnload.Keys.Contains(bank.name))
			{
				UnloadBank(bank);
			}
		}

		~AkAddressableBankManager()
		{
			AkSoundEngine.ClearBanks();
		}
	}
}

#endif // AK_WWISE_ADDRESSABLES