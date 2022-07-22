namespace Data
{
    public class ListNode {
        public int val;
        public ListNode next;
        public ListNode(int val=0, ListNode next=null) {
            this.val = val;
            this.next = next;
        }
    }


    public static class DataUtils
    {
        //要交换的是a和b后面的节点
        public static void SwitchListNode(ListNode a, ListNode b)
        {
            ListNode aNext = a.next;
            ListNode bNext = b.next;

            if (aNext == null || bNext == null)
            {
                return;
            }

            a.next = bNext;
            b.next = aNext;
        }

        public static ListNode reverseList(ListNode node, ListNode tail = null)
        {
            ListNode next = node.next;
            ListNode last = node;
            while (next != null && next != tail)
            {
                ListNode nNext = next.next;
                next.next = last;
                last = next;
                next = nNext;
            }

            return last;
        }
    }
}

