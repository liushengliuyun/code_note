public class Solution {
    //归并排序
    public double FindMedianSortedArrays(int[] nums1, int[] nums2) {
        List<int> list=new List<int>();
        int i=0,j=0;
        int nums1Length=nums1.Length;
        int nums2Length=nums2.Length;
        //遍历两个数组
        while(i<nums1Length && j<nums2Length){
            //判断大小取数据
            if(nums1[i] < nums2[j]){
                //这里可以看到++的优先级
                list.Add(nums1[i++]);
            }else{
                list.Add(nums2[j++]);
            }
        }
        while(i<nums1Length) list.Add(i++);
        while(j<nums2Length) list.Add(j++);
        //找出list的中位数
        int listLength=list.Count;
        int middleIndex=list.Count/2;
        //长度为偶数
        if(listLength%2 == 0){
            return (list[middleIndex]+list[middleIndex-1])/2;
        }
        else{
            return list[middleIndex];
        }
    }
}



//不适用list
public class Solution {
    public double FindMedianSortedArrays(int[] nums1, int[] nums2) {
        //取总长度,计算中位数 middle=len/2
        int i=0, j=0;
        int index=0;
        int nums1Length=nums1.Length;
        int nums2Length=nums2.Length; 
        int len=nums1Length + nums2Length;
        int middleIndex = len/2;
        int beforeMiddle=middleIndex-1;
        int temp;
        //遍历数据
        //下面的while判断不会跳出循环
        //while(i <= nums1Length && j <= nums2Length){

        //这个while判断会跳出循环
        while(i < nums1Length || j < nums2Length){
            vat number1=i<nums1Length ?nums1[i]:int.MaxValue;
            vat number2=j<nums1Length ?nums2[j] : int.MaxValue;
            //取一个更小的数,对应索引向前推进
            //判断index==middle,根据长度是偶数奇数返回结果
            //偶数可能要缓存前一次的数据
            if(number1 < number2){
                if(i+j == beforeMiddle){
                     temp=number1;
                }
                if(i+j == middleIndex){
                    if(len%2 == 0){
                        return (temp+number1)/2;
                    }
                    else{
                        return number1;
                    }
                }
                i++;
            }
            else{
                if(i+j == beforeMiddle){
                     temp=number2;
                }
                if(i+j == middleIndex){
                    if(len%2 == 0){
                        return (temp+number2)/2;
                    }
                    else{
                        return number2;
                    }
                }
                j++;
            }
        }
        return 0;
    }
}
