using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bamboo.Utility
{
    public abstract class MathUtil
    {
        public static bool IsWithinRangeInclusive(int value, int min, int max)
        {
            return value >= min && value <= max;
        }
        /// <summary>
        /// Test if a is close enough to b
        /// </summary>
        /// <param name="x">The value that you want to test</param>
        /// <param name="y">The value that you want to test against </param>
        /// <param name="maxDeviation">The maximum difference that a can have from b</param>
        /// <returns></returns>
        public static bool IsNearbyValue(int x, int y, int maxDeviation)
        {
            return x >= y - maxDeviation && x <= y + maxDeviation;
        }
    }
}