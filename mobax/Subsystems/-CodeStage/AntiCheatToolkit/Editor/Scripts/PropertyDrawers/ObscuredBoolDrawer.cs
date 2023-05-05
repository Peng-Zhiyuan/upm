using CodeStage.AntiCheat.ObscuredTypes;
using UnityEditor;
using UnityEngine;

namespace CodeStage.AntiCheat.EditorCode.PropertyDrawers
{
	[CustomPropertyDrawer(typeof(ObscuredBool))]
	internal class ObscuredBoolDrawer : ObscuredPropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
		{
			SerializedProperty hiddenValue = prop.FindPropertyRelative("hiddenValue");
			SetBoldIfValueOverridePrefab(prop, hiddenValue);

			SerializedProperty cryptoKey = prop.FindPropertyRelative("currentCryptoKey");
			SerializedProperty fakeValue = prop.FindPropertyRelative("fakeValue");
			SerializedProperty fakeValueChanged = prop.FindPropertyRelative("fakeValueChanged");
			SerializedProperty inited = prop.FindPropertyRelative("inited");

			int currentCryptoKey = cryptoKey.intValue;
			bool val = false;

			if (!inited.boolValue)
			{
				if (currentCryptoKey == 0)
				{
					currentCryptoKey = cryptoKey.intValue = ObscuredBool.cryptoKeyEditor;
				}
				inited.boolValue = true;
				hiddenValue.intValue = ObscuredBool.Encrypt(false, (byte)currentCryptoKey);
			}
			else
			{
				val = ObscuredBool.Decrypt(hiddenValue.intValue, (byte)currentCryptoKey);
			}

			EditorGUI.BeginChangeCheck();
			val = EditorGUI.Toggle(position, label, val);
			if (EditorGUI.EndChangeCheck())
				hiddenValue.intValue = ObscuredBool.Encrypt(val, (byte)currentCryptoKey);

			fakeValue.boolValue = val;
			fakeValueChanged.boolValue = true;
			ResetBoldFont();
		}
	}
}