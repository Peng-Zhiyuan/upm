using CodeStage.AntiCheat.ObscuredTypes;
using UnityEditor;
using UnityEngine;

namespace CodeStage.AntiCheat.EditorCode.PropertyDrawers
{
	[CustomPropertyDrawer(typeof(ObscuredVector3))]
	internal class ObscuredVector3Drawer : ObscuredPropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
		{
			SerializedProperty hiddenValue = prop.FindPropertyRelative("hiddenValue");
			SerializedProperty hiddenValueX = hiddenValue.FindPropertyRelative("x");
			SerializedProperty hiddenValueY = hiddenValue.FindPropertyRelative("y");
			SerializedProperty hiddenValueZ = hiddenValue.FindPropertyRelative("z");
			SetBoldIfValueOverridePrefab(prop, hiddenValue);

			SerializedProperty cryptoKey = prop.FindPropertyRelative("currentCryptoKey");
			SerializedProperty fakeValue = prop.FindPropertyRelative("fakeValue");
			SerializedProperty inited = prop.FindPropertyRelative("inited");

			int currentCryptoKey = cryptoKey.intValue;
			Vector3 val = Vector3.zero;

			if (!inited.boolValue)
			{
				if (currentCryptoKey == 0)
				{
					currentCryptoKey = cryptoKey.intValue = ObscuredVector3.cryptoKeyEditor;
				}
				ObscuredVector3.RawEncryptedVector3 ev = ObscuredVector3.Encrypt(Vector3.zero, currentCryptoKey);
				hiddenValueX.intValue = ev.x;
				hiddenValueY.intValue = ev.y;
				hiddenValueZ.intValue = ev.z;
                inited.boolValue = true;
			}
			else
			{
				ObscuredVector3.RawEncryptedVector3 ev = new ObscuredVector3.RawEncryptedVector3();
				ev.x = hiddenValueX.intValue;
				ev.y = hiddenValueY.intValue;
				ev.z = hiddenValueZ.intValue;
				val = ObscuredVector3.Decrypt(ev, currentCryptoKey);
			}

			EditorGUI.BeginChangeCheck();
			val = EditorGUI.Vector3Field(position, label, val);
			if (EditorGUI.EndChangeCheck())
			{
				ObscuredVector3.RawEncryptedVector3 ev = ObscuredVector3.Encrypt(val, currentCryptoKey);
				hiddenValueX.intValue = ev.x;
				hiddenValueY.intValue = ev.y;
				hiddenValueZ.intValue = ev.z;
			}
			fakeValue.vector3Value = val;
			ResetBoldFont();
        }

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.wideMode ? EditorGUIUtility.singleLineHeight : EditorGUIUtility.singleLineHeight * 2f;
		}
	}
}