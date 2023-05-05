using CodeStage.AntiCheat.ObscuredTypes; 
using UnityEditor;
using UnityEngine;

namespace CodeStage.AntiCheat.EditorCode.PropertyDrawers
{
	[CustomPropertyDrawer(typeof(ObscuredVector2))]
	internal class ObscuredVector2Drawer : ObscuredPropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
		{
			SerializedProperty hiddenValue = prop.FindPropertyRelative("hiddenValue");
			SerializedProperty hiddenValueX = hiddenValue.FindPropertyRelative("x");
			SerializedProperty hiddenValueY = hiddenValue.FindPropertyRelative("y");
			SetBoldIfValueOverridePrefab(prop, hiddenValue);

			SerializedProperty cryptoKey = prop.FindPropertyRelative("currentCryptoKey");
			SerializedProperty fakeValue = prop.FindPropertyRelative("fakeValue");
			SerializedProperty inited = prop.FindPropertyRelative("inited");

			int currentCryptoKey = cryptoKey.intValue;
			Vector2 val = Vector2.zero;

			if (!inited.boolValue)
			{
				if (currentCryptoKey == 0)
				{
					currentCryptoKey = cryptoKey.intValue = ObscuredVector2.cryptoKeyEditor;
				}
				ObscuredVector2.RawEncryptedVector2 ev = ObscuredVector2.Encrypt(Vector2.zero, currentCryptoKey);
				hiddenValueX.intValue = ev.x;
				hiddenValueY.intValue = ev.y;
                inited.boolValue = true;
			}
			else
			{
				ObscuredVector2.RawEncryptedVector2 ev = new ObscuredVector2.RawEncryptedVector2();
				ev.x = hiddenValueX.intValue;
				ev.y = hiddenValueY.intValue;
				val = ObscuredVector2.Decrypt(ev, currentCryptoKey);
			}

			EditorGUI.BeginChangeCheck();
			val = EditorGUI.Vector2Field(position, label, val);
			if (EditorGUI.EndChangeCheck())
			{
				ObscuredVector2.RawEncryptedVector2 ev = ObscuredVector2.Encrypt(val, currentCryptoKey);
				hiddenValueX.intValue = ev.x;
				hiddenValueY.intValue = ev.y;
			}
			fakeValue.vector2Value = val;
			ResetBoldFont();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.wideMode ? EditorGUIUtility.singleLineHeight : EditorGUIUtility.singleLineHeight * 2f;
		}
	}
}