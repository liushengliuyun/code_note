// using UnityEngine;
// using System.Runtime.InteropServices;
// using Core.Controllers;
// using Utils;
//
// namespace iOSCShape
// {
//     public class iOSCShapeShareTool : YZBaseController<iOSCShapeShareTool>
//     {
//
// #if UNITY_IOS && !UNITY_EDITOR
//     [DllImport("__Internal")] private static extern void ObjcShareToWhatsAppUnity(string param);
//     [DllImport("__Internal")] private static extern void ObjcShareToMessageUnity(string param);
//     [DllImport("__Internal")] private static extern void ObjcShareToDefaultUnity(string param);
// #endif
//
//         public void IOSYZShareToWhatsApp(string param)
//         {
// #if UNITY_IOS && !UNITY_EDITOR
//         ObjcShareToWhatsAppUnity(param);
// #endif
//         }
//
//         public void IOSYZShareToMessage(string param)
//         {
// #if UNITY_IOS && !UNITY_EDITOR
//         ObjcShareToMessageUnity(param);
// #endif
//         }
//
//         public void IOSYZShareToDefault(ShareYZDefaultParams param)
//         {
// #if UNITY_IOS && !UNITY_EDITOR
//         ObjcShareToDefaultUnity(JsonUtility.ToJson(param));
// #endif
//         }
//
//         public void CShapeMessageShareNotSupport(string msg)
//         {
//             YZDebug.Log("设备不支持短信分享");
//             if (msg == YZNativeErrorCode.shared_sms)
//             {
//                 YZTopControl.YZShowAutoHideTips(YZLocal.GetLocal(YZLocalID.key_shared_sms_error));
//             }
//             else if (msg == YZNativeErrorCode.shared_msg)
//             {
//                 YZTopControl.YZShowAutoHideTips(YZLocal.GetLocal(YZLocalID.key_shared_msg_error));
//             }
//             else if (msg == YZNativeErrorCode.shared_wtp)
//             {
//                 YZTopControl.YZShowAutoHideTips(YZLocal.GetLocal(YZLocalID.key_shared_wap_error));
//             }
//             else if (msg == YZNativeErrorCode.shared_dft)
//             {
//                 YZTopControl.YZShowAutoHideTips(YZLocal.GetLocal(YZLocalID.key_shared_dft_error));
//             }
//         }
//
//         // 完成Message分享
//         public void CShapeMessageShareDismissed(string msg)
//         {
//             if (msg == "100")
//             {
//                 YZDebug.Log("取消分享");
//             }
//             else if (msg == "200")
//             {
//                 YZDebug.Log("分享成功");
//             }
//             else
//             {
//                 YZDebug.Log("分享失败");
//             }
//         }
//
//         // 完成系统默认分享
//         public void CShapeDefaultShareDismissed(string json)
//         {
//             ShareYZDefaultResponse response = YZGameUtil.JsonYZToObject<ShareYZDefaultResponse>(json);
//             YZDebug.LogConcat("系统分享完成: ", response.completed);
//         }
//     }
// }