using System.Collections;
using UnityEditor;

namespace Swing.Editor
{
	public static class EditorCoroutineExtensions
	{
		public static EditorCoroutines.EditorCoroutine StartCoroutine<T>(this T thisRef, IEnumerator coroutine)
		{
			return EditorCoroutines.StartCoroutine(coroutine, thisRef);
		}

		public static EditorCoroutines.EditorCoroutine StartCoroutine<T>(this T thisRef, string methodName)
		{
			return EditorCoroutines.StartCoroutine(methodName, thisRef);
		}

		public static EditorCoroutines.EditorCoroutine StartCoroutine<T>(this T thisRef, string methodName, object value)
		{
			return EditorCoroutines.StartCoroutine(methodName, value, thisRef);
		}

		public static void StopCoroutine<T>(this T thisRef, IEnumerator coroutine)
		{
			EditorCoroutines.StopCoroutine(coroutine, thisRef);
		}

		public static void StopCoroutine<T>(this T thisRef, string methodName)
		{
			EditorCoroutines.StopCoroutine(methodName, thisRef);
		}

		public static void StopAllCoroutines<T>(this T thisRef)
		{
			EditorCoroutines.StopAllCoroutines(thisRef);
		}
	}
}