using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtils
{
	public static Vector2 GetNormal(this Vector2 src)
	{
		float tx = src.x;
		src.x = -src.y;
		src.y = tx;
		src.Normalize();
		return src;
	}
}
