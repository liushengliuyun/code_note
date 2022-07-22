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

            if (reverse && firstL != null )
            {
                //将链表反转
                ListNode next = firstL.next;
                firstL.next = head;
                ListNode last = firstL;
                while (next != null && next != tail)
                {
                    ListNode nNext = next.next;
                    next.next = last;
                    last = next;
                    next = nNext;
                }
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
        
        public static void Test()
        {

            int[] input = new[] {3, 2, 1};
            ListNode nextNode = null;
            for (int i = input.Length - 1; i >= 0 ; i--)
            {
                ListNode node = new ListNode(input[i], nextNode);
                nextNode = node;
                Console.WriteLine(input[i]);
            }

            ListNode result = SortList(nextNode);
        }
    }
}