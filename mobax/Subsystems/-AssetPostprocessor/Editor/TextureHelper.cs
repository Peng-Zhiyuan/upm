using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Reflection;

public static class TextureHelper
{
	static MethodInfo mGetWidthAndHeightMethod = null;
	public static bool GetImageSize(TextureImporter importer, out int width, out int height) 
	{
        if (importer != null) {
            object[] args = new object[2] { 0, 0 };
			if (mGetWidthAndHeightMethod == null)
				mGetWidthAndHeightMethod = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
			mGetWidthAndHeightMethod.Invoke(importer, args);

            width = (int)args[0];
            height = (int)args[1];

            return true;
        }

	    height = width = 0;
	    return false;
	}
}

