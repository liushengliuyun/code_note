namespace ConsoleApp1
{
    public class Solution {

        public bool IsPalindrome(int x) {
            //0 不是回文数 ,能被10整除的也不是回文数
            if (x < 0 || (x % 10 == 0 && x != 0)) {
                return false;
            }

            int reversNumber = 0;
            while (x > reversNumber)
            {
                reversNumber = x % 10 + reversNumber * 10;
                x /= 10; //1221 1 122 12 12  
                //12121 1 122 12 121 121 12 
            }

            //存在奇数偶数的情况
            return reversNumber == x || reversNumber / 10 == x;
        }
    }
}
