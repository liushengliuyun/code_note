using System;
using System.Collections.Generic;
using Data;

//给你链表的头结点 head ，请将其按 升序 排列并返回 排序后的链表 。 
//
// 
// 
//
// 
//
// 示例 1： 
//
// 
//输入：head = [4,2,1,3]
//输出：[1,2,3,4]
// 
//
// 示例 2： 
//
// 
//输入：head = [-1,5,3,4,0]
//输出：[-1,0,3,4,5]
// 
//
// 示例 3： 
//
// 
//输入：head = []
//输出：[]
// 
//
// 
//
// 提示： 
//
// 
// 链表中节点的数目在范围 [0, 5 * 10⁴] 内 
// -10⁵ <= Node.val <= 10⁵ 
// 
//
// 
//
// 进阶：你可以在 O(n log n) 时间复杂度和常数级空间复杂度下，对链表进行排序吗？ 
// Related Topics 链表 双指针 分治 排序 归并排序 👍 1696 👎 0


//平均时间复杂度为O(n log n)的排序算法, 常见的有快速排序, 归并排序, 堆排序
namespace _148
{
    public class _148_排序链表
    {
        #region 快速排序 交换链接结点

        //目标是要找到两个node , 能够切割两段的
        //dummyNode保证next是切割后链表的头节点, head保证是分段的那个点
        public static ListNode partition(ListNode dummyNode, ListNode head, ListNode tail, out bool isOrder)
        {
            ListNode left = new ListNode();
            ListNode right = new ListNode();

            ListNode firstL = null;
            ListNode firstR = null;
            //有人称这个基准值为枢椎
            int compare = head.val;
            ListNode nextNode = head.next;
            isOrder = true;
            bool reverse = true;
            int max = compare;
            int min = compare;
            while (nextNode != null && nextNode != tail)
            {
                if (nextNode.val >= max)
                {
                    max = nextNode.val;
                }
                else
                {
                    isOrder = false;
                }


                //是否恰好是反转的链表
                if (nextNode.val > min)
                {
                    reverse = false;
                }
                else
                {
                    min = nextNode.val;
                }

                if (nextNode.val > compare)
                {
                    if (firstR == null)
                    {
                        firstR = nextNode;
                    }

                    right.next = nextNode;
                    right = nextNode;
                }
                else
                {
                    if (firstL == null)
                    {
                        firstL = nextNode;
                    }

                    left.next = nextNode;
                    left = nextNode;
                }

                nextNode = nextNode.next;
            }

            if (reverse && firstL != null)
            {
                //将链表反转
                ListNode last = DataUtils.reverseList(firstL, tail);
                firstL.next = head;
                dummyNode.next = last;
                isOrder = true;
            }
            else
            {
                //dummyNode 指向链表头节点
                if (firstL == null)
                {
                    firstL = head;
                }

                dummyNode.next = firstL;
                left.next = head;
            }

            right.next = tail;
            //比枢椎小的链接到head

            if (firstR == null)
            {
                firstR = tail;
            }

            head.next = firstR;

            return head;
        }

        public static void Quick_Sort(ListNode dummyNode, ListNode head, ListNode tail)
        {
            if (head != tail)
            {
                bool isOrder;
                ListNode pivot = partition(dummyNode, head, tail, out isOrder);
                //如果是有序的,不用再递归
                if (!isOrder)
                {
                    Quick_Sort(dummyNode, dummyNode.next, pivot);
                    Quick_Sort(pivot, pivot.next, tail);
                }
            }
        }

        public static ListNode SortList(ListNode head)
        {
            ListNode dummyNode = new ListNode();
            dummyNode.next = head;
            Quick_Sort(dummyNode, head, null);
            return dummyNode.next;
        }

        #endregion


        
        //执行耗时:1268 ms,击败了5.23% 的C#用户
        //内存消耗:53.9 MB,击败了59.88% 的C#用户
        //比交换链接要慢10倍, 原因不明, 快慢节点的设计很巧妙
        #region 快速排序 交换值

        public static ListNode partition_value(ListNode dummyNode, ListNode head, ListNode tail, out bool isOrder)
        {
            ListNode slow = head;
            ListNode fast = head.next;

            //有人称这个基准值为枢椎
            int compare = head.val;
            int max = compare;
            int min = compare;

            isOrder = true;
            //这个函数的变量越少越好
            bool reverse = true;
            while (fast != null && fast != tail)
            {
                if (fast.val >= max)
                {
                    max = fast.val;
                }
                else
                {
                    isOrder = false;
                }

                //是否恰好是反转的链表
                if (fast.val <= min)
                {
                    min = fast.val;
                }

                if (fast.val < compare)
                {
                    slow = slow.next;
                    // if (fast.val != slow.val)
                    // val 为 int 的情况,不做比较判断要快一点
                    
                    //在外部使用temp , 没有什么区别
                    // temp = slow.val;
                    // slow.val = fast.val;
                    // fast.val = temp;
                    (fast.val, slow.val) = (slow.val, fast.val);
                }

                if (fast.val > min)
                {
                    reverse = false;
                }

                fast = fast.next;
            }

            //反转链表
            if (reverse)
            {
                ListNode last = DataUtils.reverseList(head, tail);
                head.next = tail;
                dummyNode.next = last;
                isOrder = true;
            }
            else if (!isOrder && slow != head)
            {
                // 会导致 10  , 1, 2, 3, 4,5, 6 这种结构的消耗很多, 因为每次总把最后面次大的数与最大的数交换
                // (head.val, slow.val) = (slow.val, head.val);
                dummyNode.next = head.next;
                ListNode slowNext = slow.next;
                slow.next = head;
                head.next = slowNext;
            }

            return head;
        }


        public static void Quick_Sort_Value(ListNode dummyNode, ListNode head, ListNode tail)
        {
            if (head != tail)
            {
                bool isOrder;
                ListNode pivot = partition_value(dummyNode, head, tail, out isOrder);
                //如果是有序的,不用再递归
                if (!isOrder)
                {
                    Quick_Sort_Value(dummyNode, dummyNode.next, pivot);
                    Quick_Sort_Value(pivot, pivot.next, tail);
                }
            }
        }

        public static ListNode SortList_Value(ListNode head)
        {
            if (head == null || head.next == null)
            {
                return head;
            }

            ListNode dummyNode = new ListNode();
            dummyNode.next = head;
            Quick_Sort_Value(dummyNode, head, null);
            return dummyNode.next;
        }

        #endregion

        public static void Test()
        {
            //4,2,1,3,5,6,6,3
            // int[] input = {50000,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29};
            int[] input = {-1,5,3,4,0};
            // int[] input = {4, 2, 1, 3};
            ListNode nextNode = null;
            for (int i = input.Length - 1; i >= 0; i--)
            {
                ListNode node = new ListNode(input[i], nextNode);
                nextNode = node;
                // Console.WriteLine(input[i]);
            }

            ListNode result = SortList_Value(nextNode);
            Console.WriteLine(result.val);
            while (result.next != null)
            {
                Console.WriteLine(result.next.val);
                result = result.next;
            }
        }
    }
}