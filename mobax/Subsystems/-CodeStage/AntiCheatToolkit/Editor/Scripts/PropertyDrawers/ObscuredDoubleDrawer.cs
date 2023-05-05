#define UNITY_5_0_PLUS
#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9
#undef UNITY_5_0_PLUS
#endif

#if UNITY_5_0_PLUS
using System.Runtime.InteropServices;
#endif

using CodeStage.AntiCheat.ObscuredTypes;
using UnityEditor;
using UnityEngine;

namespace CodeStage.AntiCheat.EditorCode.PropertyDrawers
{
	[CustomPropertyDrawer(typeof(ObscuredDouble))]
	internal class ObscuredDoubleDrawer : ObscuredPropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
		{
#if !UNITY_5_0_PLUS
			EditorGUI.LabelField(position, label.text + " [works in Unity 5+]");
		}
#else
			SerializedProperty hiddenValue = prop.FindPropertyRelative("hiddenValue");
			SetBoldIfValueOverridePrefab(prop, hiddenValue);

			SerializedProperty cryptoKey = prop.FindPropertyRelative("currentCryptoKey");
			SerializedProperty fakeValue = prop.FindPropertyRelative("fakeValue");
			SerializedProperty inited = prop.FindPropertyRelative("inited");

			long currentCryptoKey = cryptoKey.longValue;

			LongBytesUnion union = new LongBytesUnion();
			double val = 0;

			if (!inited.boolValue)
			{
				if (currentCryptoKey == 0)
				{
					currentCryptoKey = cryptoKey.longValue = ObscuredDouble.cryptoKeyEditor;
				}
				hiddenValue.arraySize = 8;
				inited.boolValue = true;

				union.l = ObscuredDouble.Encrypt(0, currentCryptoKey);

				hiddenValue.GetArrayElementAtIndex(0).intValue = union.b1;
				hiddenValue.GetArrayElementAtIndex(1).intValue = union.b2;
				hiddenValue.GetArrayElementAtIndex(2).intValue = union.b3;
				hiddenValue.GetArrayElementAtIndex(3).intValue = union.b4;
				hiddenValue.GetArrayElementAtIndex(4).intValue = union.b5;
				hiddenValue.GetArrayElementAtIndex(5).intValue = union.b6;
				hiddenValue.GetArrayElementAtIndex(6).intValue = union.b7;
				hiddenValue.GetArrayElementAtIndex(7).intValue = union.b8;
			}
			else
			{
				int arraySize = hiddenValue.arraySize;
				byte[] hiddenValueArray = new byte[arraySize];
				for (int i = 0; i < arraySize; i++)
				{
					hiddenValueArray[i] = (byte)hiddenValue.GetArrayElementAtIndex(i).intValue;
				}

				union.b1 = hiddenValueArray[0];
				union.b2 = hiddenValueArray[1];
				union.b3 = hiddenValueArray[2];
				union.b4 = hiddenValueArray[3];
				union.b5 = hiddenValueArray[4];
				union.b6 = hiddenValueArray[5];
				union.b7 = hiddenValueArray[6];
				union.b8 = hiddenValueArray[7];

				val = ObscuredDouble.Decrypt(union.l, currentCryptoKey);
			}

			EditorGUI.BeginChangeCheck();
			val = EditorGUI.DoubleField(position, label, val);
			if (EditorGUI.EndChangeCheck())
			{
				union.l = ObscuredDouble.Encrypt(val, currentCryptoKey);

				hiddenValue.GetArrayElementAtIndex(0).intValue = union.b1;
				hiddenValue.GetArrayElementAtIndex(1).intValue = union.b2;
				hiddenValue.GetArrayElementAtIndex(2).intValue = union.b3;
				hiddenValue.GetArrayElementAtIndex(3).intValue = union.b4;
				hiddenValue.GetArrayElementAtIndex(4).intValue = union.b5;
				hiddenValue.GetArrayElementAtIndex(5).intValue = union.b6;
				hiddenValue.GetArrayElementAtIndex(6).intValue = union.b7;
				hiddenValue.GetArrayElementAtIndex(7).intValue = union.b8;
			}

			fakeValue.doubleValue = val;
			ResetBoldFont();
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct LongBytesUnion
		{
			[FieldOffset(0)]
			public long l;

			[FieldOffset(0)]
			public byte b1;

			[FieldOffset(1)]
			public byte b2;

			[FieldOffset(2)]
			public byte b3;

			[FieldOffset(3)]
			public byte b4;

			[FieldOffset(4)]
			public byte b5;

			[FieldOffset(5)]
			public byte b6;

			[FieldOffset(6)]
			public byte b7;

			[FieldOffset(7)]
			public byte b8;
		}
#endif
	}
}