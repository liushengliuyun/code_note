using System;
using System.Collections.Generic;

//给定两个大小分别为 m 和 n 的正序（从小到大）数组 nums1 和 nums2。请你找出并返回这两个正序数组的 中位数 。 
//
// 算法的时间复杂度应该为 O(log (m+n)) 。 
//
// 
//
// 示例 1： 
//
// 
//输入：nums1 = [1,3], nums2 = [2]
//输出：2.00000
//解释：合并数组 = [1,2,3] ，中位数 2
// 
//
// 示例 2： 
//
// 
//输入：nums1 = [1,2], nums2 = [3,4]
//输出：2.50000
//解释：合并数组 = [1,2,3,4] ，中位数 (2 + 3) / 2 = 2.5
// 
//
// 
//
// 
//
// 提示： 
//
// 
// nums1.length == m 
// nums2.length == n 
// 0 <= m <= 1000 
// 0 <= n <= 1000 
// 1 <= m + n <= 2000 
// -10⁶ <= nums1[i], nums2[i] <= 10⁶ 
// 
//
// Related Topics 数组 二分查找 分治 👍 5647 👎 0


//todo 这题测试没有过, 找下原因
namespace _4
{
    public class Solution
    {
        //归并排序
        public static double FindMedianSortedArrays1(int[] nums1, int[] nums2)
        {
            List<int> list = new List<int>();
            int i = 0, j = 0;
            int nums1Length = nums1.Length;
            int nums2Length = nums2.Length;
            //遍历两个数组
            while (i < nums1Length && j < nums2Length)
            {
                //判断大小取数据
                if (nums1[i] < nums2[j])
                {
                    //这里可以看到++的优先级
                    list.Add(nums1[i++]);
                }
                else
                {
                    list.Add(nums2[j++]);
                }
            }

            while (i < nums1Length) list.Add(i++);
            while (j < nums2Length) list.Add(j++);
            //找出list的中位数
            int listLength = list.Count;
            int middleIndex = list.Count / 2;
            //长度为偶数
            if (listLength % 2 == 0)
            {
                return (list[middleIndex] + list[middleIndex - 1]) / 2;
            }
            else
            {
                return list[middleIndex];
            }
        }

        //不使用list
        public static double FindMedianSortedArrays2(int[] nums1, int[] nums2)
        {
            //取总长度,计算中位数 middle=len/2
            int i = 0, j = 0;
            int index = 0;
            int nums1Length = nums1.Length;
            int nums2Length = nums2.Length;
            int len = nums1Length + nums2Length;
            int middleIndex = len / 2;
            int beforeMiddle = middleIndex - 1;
            int temp = Int32.MinValue;
            //遍历数据
            //下面的while判断不会跳出循环
            //while(i <= nums1Length && j <= nums2Length){

            //这个while判断会跳出循环
            while (i < nums1Length || j < nums2Length)
            {
                int number1 = i < nums1Length ? nums1[i] : int.MaxValue;
                int number2 = j < nums1Length ? nums2[j] : int.MaxValue;
                //取一个更小的数,对应索引向前推进
                //判断index==middle,根据长度是偶数奇数返回结果
                //偶数可能要缓存前一次的数据
                if (number1 < number2)
                {
                    if (i + j == beforeMiddle)
                    {
                        temp = number1;
                    }

                    if (i + j == middleIndex)
                    {
                        if (len % 2 == 0)
                        {
                            return (temp + number1) / 2;
                        }
                        else
                        {
                            return number1;
                        }
                    }

                    i++;
                }
                else
                {
                    if (i + j == beforeMiddle)
                    {
                        temp = number2;
                    }

                    if (i + j == middleIndex)
                    {
                        if (len % 2 == 0)
                        {
                            return (temp + number2) / 2;
                        }
                        else
                        {
                            return number2;
                        }
                    }

                    j++;
                }
            }

            return 0;
        }

        public static double Test(int[] nums1, int[] nums2)
        {
            return 0;
        }
    }
}