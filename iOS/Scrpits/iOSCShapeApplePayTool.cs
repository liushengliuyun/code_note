// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using System.Runtime.InteropServices;
// using Core.Controllers;
// using Utils;
//
// namespace iOSCShape
// {
//     public class iOSCShapeApplePayTool : YZBaseController<iOSCShapeApplePayTool>
//     {
// #if UNITY_IOS && !UNITY_EDITOR
//     [DllImport("__Internal")] private static extern bool ObjcCanMakePaymentsUnity();
//     [DllImport("__Internal")] private static extern bool ObjcCanSupportApplePayUnity();
//     [DllImport("__Internal")] private static extern void ObjcStartPayUnity(string param);
// #endif
//
//         private YZAppleAuth apple_pay_authorization;
//         private bool is_succesed;
//
//         // true if the device is generally capable of making in-app payments.
//         // false if the device cannot make in-app payments or if the user is restricted from authorizing payments
//         public bool IOSCanMakePayments()
//         {
// #if UNITY_IOS && !UNITY_EDITOR
//         return ObjcCanMakePaymentsUnity();
// #endif
//             return false;
//         }
//
//         // 注: 该方法返回true 则IOSCanMakePayments一定返回true
//         // true if the user can authorize payments on this device using one of the payment networks supported by the merchant.
//         // false if the user cannot authorize payments on these networks or if the user is restricted from authorizing payments.
//         // 当前检测了卡类型: American Express、MasterCard、Visa、China Union、Discover、Interac、iD、Store credit and debit cards
//         public bool IOSCanSupportApplePay()
//         {
// #if UNITY_IOS && !UNITY_EDITOR
//         return ObjcCanSupportApplePayUnity();
// #endif
//             return false;
//         }
//
//         // 开始支付
//         public void IOSStartPay(YZApplePay pay)
//         {
//             apple_pay_authorization = null;
//             is_succesed = false;
// #if UNITY_IOS && !UNITY_EDITOR
//         string json = JsonUtility.ToJson(pay);
//         ObjcStartPayUnity(json);
// #endif
//         }
//
//         public void CShapeApplePayAuthorization(string json)
//         {
//             // 支付中心传 base64 服务器传 string
//             apple_pay_authorization = YZGameUtil.JsonYZToObject<YZAppleAuth>(json);
//             YZDebug.LogConcat("Token String: ", apple_pay_authorization.str, " Token Base64: ", apple_pay_authorization.bas);
//         }
//
//         // 支付结果 "0": 取消了或失败了 或 "1": 支付成功
//         public void CShapeApplePayFinish(string succ)
//         {
//             YZDebug.LogConcat("Apple Pay 支付结果: ", succ);
//             if (apple_pay_authorization != null && succ == "1")
//             {
//                 if (!is_succesed)
//                 {
//                     is_succesed = true;
//                     YZServerApiCharge.Shared.YZDepositApplePay(apple_pay_authorization.bas);
//                 }
//                 else
//                     YZDebug.LogConcat("Apple Pay 支付结果成功回调重复");
//             }
//             else
//             {
//                 YZChargeErrorUICtrler.Shared().YZOnOpenUI();
//             }
//         }
//     }
// }