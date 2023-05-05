using CodeStage.AntiCheat.ObscuredTypes;
using UnityEditor;
using UnityEngine;

namespace CodeStage.AntiCheat.EditorCode.PropertyDrawers
{
	[CustomPropertyDrawer(typeof(ObscuredString))]
	internal class ObscuredStringDrawer : ObscuredPropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
		{
			SerializedProperty hiddenValue = prop.FindPropertyRelative("hiddenValue");
			SetBoldIfValueOverridePrefab(prop, hiddenValue);

			SerializedProperty cryptoKey = prop.FindPropertyRelative("currentCryptoKey");
			SerializedProperty fakeValue = prop.FindPropertyRelative("fakeValue");
			SerializedProperty inited = prop.FindPropertyRelative("inited");

			string currentCryptoKey = cryptoKey.stringValue;
			string val = "";

			if (!inited.boolValue)
			{
				if (string.IsNullOrEmpty(currentCryptoKey))
				{
					currentCryptoKey = cryptoKey.stringValue = ObscuredString.cryptoKeyEditor;
				}
				inited.boolValue = true;
				EncryptAndSetBytes(val, hiddenValue, currentCryptoKey);
			}
			else
			{
				SerializedProperty size = hiddenValue.FindPropertyRelative("Array.size");
				bool showMixed = size.hasMultipleDifferentValues;

				if (!showMixed)
				{
					for (int i = 0; i < hiddenValue.arraySize; i++)
					{
						showMixed |= hiddenValue.GetArrayElementAtIndex(i).hasMultipleDifferentValues;
						if (showMixed) break;
					}
				}

				if (!showMixed)
				{
					byte[] bytes = new byte[hiddenValue.arraySize];
					for (int i = 0; i < hiddenValue.arraySize; i++)
					{
						bytes[i] = (byte)hiddenValue.GetArrayElementAtIndex(i).intValue;
					}

					val = ObscuredString.EncryptDecrypt(GetString(bytes), currentCryptoKey);
				}
				else
				{
					EditorGUI.showMixedValue = true;
				}
			}

			int dataIndex = prop.propertyPath.IndexOf("Array.data[");

			if (dataIndex >= 0)
			{
				dataIndex += 11;

				string index = "Element " + prop.propertyPath.Substring(dataIndex, prop.propertyPath.IndexOf("]", dataIndex, System.StringComparison.Ordinal) - dataIndex);
				label.text = index;
			}

			EditorGUI.BeginChangeCheck();
			val = EditorGUI.TextField(position, label, val);
			if (EditorGUI.EndChangeCheck())
			{
				EncryptAndSetBytes(val, hiddenValue, currentCryptoKey);
			}
			fakeValue.stringValue = val;

			EditorGUI.showMixedValue = false;

			/*if (prop.isInstantiatedPrefab)
			{
				SetBoldDefaultFont(prop.prefabOverride);
			}*/
			ResetBoldFont();
		}
 
		private static void EncryptAndSetBytes(string val, SerializedProperty prop, string key)
		{
			string encrypted = ObscuredString.EncryptDecrypt(val, key);
			byte[] encryptedBytes = GetBytes(encrypted);

			prop.ClearArray();
			prop.arraySize = encryptedBytes.Length;

			for (int i = 0; i < encryptedBytes.Length; i++)
			{
				prop.GetArrayElementAtIndex(i).intValue = encryptedBytes[i];
			}
		}

		private static byte[] GetBytes(string str)
		{
			byte[] bytes = new byte[str.Length * sizeof(char)];
			System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
			return bytes;
		}

		private static string GetString(byte[] bytes)
		{
			char[] chars = new char[bytes.Length / sizeof(char)];
			System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
			return new string(chars);
		}
	}
}