using System.Collections.Generic;

namespace MyNotch
{
    public static class MyNotchHelp
    {
        public static void GeneratePermutations(List<int[]> resultList, int[] result, int[] digits, int index)
        {
            if (index == result.Length)
            {
                // 排除数组中所有元素都相同的情况
                if (!AllElementsSame(result))
                {
                    resultList.Add(result.Clone() as int[]);
                }
                return;
            }

            for (int i = 0; i < digits.Length; i++)
            {
                result[index] = digits[i];
                GeneratePermutations(resultList, result, digits, index + 1);
            }
        }
        
        private static bool AllElementsSame(int[] array)
        {
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i] != array[0])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// row是从下往上, 服务器的是从上往下0 - 2
        /// </summary>
        public static void ConvertPosId(int posId, out int row, out int col)
        {
            row = posId % 3;
            col = posId / 3;
        }
    }
}