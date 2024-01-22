#import "BRUnityOC.h"
#import "BRiOSTool.h"
#import "BRShareTool.h"
#import "BRShuShuTool.h"
#import "BRWebViewTool.h"
#import "BRAppsflyerTool.h"
#import "BRLocationManager.h"
#import "BRPickerVC.h"
#import "BRPushTool.h"
#import "BRApplePayTool.h"
#import "BREmailTool.h"
#import "BRTongdunTool.h"
#import "BRKycTool.h"
#import "BRForterTool.h"
#import "BRRiskifiedTool.h"

#if defined(__cplusplus)
extern "C"{
#endif

char* ObjcCharMemoryManagementUnity(NSString *text)
{
    const char* string = [text UTF8String];
    return string ? strdup(string) : NULL;
}

char *ObjcGetUDIDUnity()
{
    NSString *str = [BRiOSTool NativeGetUDIDiOS];
    return ObjcCharMemoryManagementUnity(str);
}

char *ObjcGetTimeZoneUnity()
{
    NSString *str = [BRiOSTool NativeGetTimeZoneiOS];
    return ObjcCharMemoryManagementUnity(str);
}

char *ObjcGetLanguageUnity()
{
    NSString *str = [BRiOSTool NativeGetLanguageiOS];
    return ObjcCharMemoryManagementUnity(str);
}

char *ObjcGetATTStatusUnity()
{
    NSString *str = [BRiOSTool NativeGetATTStatusiOS];
    return ObjcCharMemoryManagementUnity(str);
}

char *ObjcGetDeviceInfoUnity()
{
    NSString *str = [BRiOSTool NativeGetDeviceInfoiOS];
    return ObjcCharMemoryManagementUnity(str);
}

char *ObjcGetIPAddressUnity()
{
    NSString *str = [BRiOSTool NativeGetIPAddress];
    return ObjcCharMemoryManagementUnity(str);
}

char *ObjcGetPasteBoardTextUnity()
{
    UIPasteboard *pasteboard = [UIPasteboard generalPasteboard];
    if ([pasteboard.string isKindOfClass:[NSString class]]) {
        return ObjcCharMemoryManagementUnity(pasteboard.string);
    }
    return ObjcCharMemoryManagementUnity(@"");
}

char *ObjcGetLocationAuthorizationUnity()
{
    NSString *str = [[BRLocationManager shared] BRGetLocationAuthorizationiOS];
    return ObjcCharMemoryManagementUnity(str);
}

char *ObjcGetLastLocationUnity()
{
    NSString *str = [[BRLocationManager shared] BRGetLastLocationiOS];
    return ObjcCharMemoryManagementUnity(str);
}

char *ObjcGetAppsFlyerIdUnity()
{
    NSString *str = [BRAppsflyerTool BRGetAppsFlyerIdiOS];
    return ObjcCharMemoryManagementUnity(str);
}

char *ObjcGetDistinctIDUnity()
{
    NSString *str = [[BRShuShuTool shared] BRGetDistinctIDiOS];
    return ObjcCharMemoryManagementUnity(str);
}

char *ObjcGetPushAuthUnity()
{
    NSString *str = [[BRPushTool shared] BRGetPushAuthiOS];
    return ObjcCharMemoryManagementUnity(str);
}

char *ObjcGetLocalCountryCodeUnity()
{
    NSString *str = [BRiOSTool NativeGetLocalCountryCodeiOS];
    return ObjcCharMemoryManagementUnity(str);
}

char *ObjcGetSimInfoUnity()
{
    NSString *str = [BRiOSTool NativeGetSimInfoiOS];
    return ObjcCharMemoryManagementUnity(str);
}

void ObjcInitKycUnity(char *param)
{
    [[BRKycTool shared] BRInitKyciOS:[NSString stringWithUTF8String:param]];
}

void ObjcUpdateKycTokenUnity(char *param)
{
    [[BRKycTool shared] BRUpdateKycTokeniOS:[NSString stringWithUTF8String:param]];
}

bool ObjcIsFullSecreenUnity()
{
    return [BRiOSTool NativeIsFullSecreeniOS];
}

bool ObjcIsEnableWIFIUnity()
{
    return [BRiOSTool NativeIsEnableWIFI];
}

bool ObjcCanMakePaymentsUnity()
{
    return [[BRApplePayTool shared] BRCanMakePaymentsiOS];
}

bool ObjcCanSupportApplePayUnity()
{
    return [[BRApplePayTool shared] BRCanSupportApplePayiOS];
}

bool ObjcSendEmailUnity(char *param)
{
    return [[BREmailTool shared] BRSendEmailiOS:[NSString stringWithUTF8String:param]];
}

void ObjcStartPayUnity(char *param)
{
    [[BRApplePayTool shared] BRStartPayiOS:[NSString stringWithUTF8String:param]];
}

void ObjcInitPushUnity()
{
    [[BRPushTool shared] BRInitPushiOS];
}

void ObjcSetTagsUnity(char *param)
{
    [[BRPushTool shared] BRSetTagsiOS:[NSString stringWithUTF8String:param]];
}

void ObjcGetNetworkAuthUnity()
{
    [BRiOSTool NativeGetNetworkAuth];
}

void ObjcFlushDatasUnity()
{
    [[BRShuShuTool shared] BRFlushDatasiOS];
}

void ObjcOpenEmailAppUnity()
{
    [BRiOSTool NativeOpenEmailAppiOS];
}
// 获取ATT权限
void ObjcRequestATTUnity()
{
    [BRiOSTool NativeRequestATTiOS];
}

void ObjcGotoSettingUnity()
{
    [BRiOSTool NativeGotoSettingiOS];
}

void ObjcGetLocationUnity()
{
    [[BRLocationManager shared] BRGetLocationiOS];
}

void ObjcCloseWebViewUnity()
{
    [[BRWebViewTool shared] BRCloseWebViewiOS];
}

//获取定位权限
void ObjcRequestLocationAuthorizationUnity()
{
    [[BRLocationManager shared] BRRequestLocationAuthorizationiOS];
}

void ObjcGetTongDunBlackBoxUnity()
{
    [[BRTongdunTool shared] BRGetTongDunBlackBoxiOS];
}

void ObjcCalibrateTimeUnity(char *param)
{
    [[BRShuShuTool shared] BRCalibrateTimeiOS:[NSString stringWithUTF8String:param]];
}

void ObjcInitThinkUnity(char *param)
{
    [[BRShuShuTool shared] BRInitThinkiOS:[NSString stringWithUTF8String:param]];
}

void ObjcLoginThinkUnity(char *param)
{
    [[BRShuShuTool shared] BRLoginThinkiOS:[NSString stringWithUTF8String:param]];
}

void ObjcStartAutoEventUnity(char *param)
{
    [[BRShuShuTool shared] BRStartAutoEventiOS:[NSString stringWithUTF8String:param]];
}

void ObjcSetGameRoundsUnity(char *param)
{
    [[BRShuShuTool shared] BRSetGameRoundsiOS:[NSString stringWithUTF8String:param]];
}

void ObjcThinkUserSetUnity(char *param)
{
    [[BRShuShuTool shared] BRThinkUserSetiOS:[NSString stringWithUTF8String:param]];
}

void ObjcThinkUserSetOnceUnity(char *param)
{
    [[BRShuShuTool shared] BRThinkUserSetOnceiOS:[NSString stringWithUTF8String:param]];
}

void ObjcThinkTrackUnity(char *param)
{
    [[BRShuShuTool shared] BRThinkTrackiOS:[NSString stringWithUTF8String:param]];
}

void ObjcStartAppsFlyerUnity()
{
    [[BRAppsflyerTool shared] BRStartAppsFlyeriOS];
}

void ObjcSendAppsFlyerEventUnity(char *param)
{
    [BRAppsflyerTool BRSendAppsFlyerEventiOS:[NSString stringWithUTF8String:param]];
}

void ObjcSendAppsFlyerValueUnity(char *param)
{
    [BRAppsflyerTool BRSendAppsFlyerValueiOS:[NSString stringWithUTF8String:param]];
}

void ObjcGotoRateUnity(char *param)
{
    [BRiOSTool NativeGotoRateiOS:[NSString stringWithUTF8String:param]];
}

void ObjcStartVibrationUnity(char *param)
{
    [BRiOSTool NativeStartVibrationiOS:[NSString stringWithUTF8String:param]];
}

void ObjcStartPickerImageUnity(char *param)
{
    [[BRPickerVC shared] BRBeginUploadHeadiOS:[NSString stringWithUTF8String:param]];
}

void ObjcSetPasteBoardTextUnity(char *param)
{
    UIPasteboard *pasteboard = [UIPasteboard generalPasteboard];
    [pasteboard setString:[NSString stringWithUTF8String:param]];
}

void ObjcSetupLocationUnity(char *param)
{
    [[BRLocationManager shared] BRSetupLoactioniOS:[NSString stringWithUTF8String:param]];
}

void ObjcShowWebViewInAppSafariUnity(char *param)
{
    [[BRWebViewTool shared] BRShowWebViewInAppSafariiOS:[NSString stringWithUTF8String:param]];
}

void ObjcShowWebViewSafariUnity(char *param)
{
    [[BRWebViewTool shared] BRShowWebViewSafariiOS:[NSString stringWithUTF8String:param]];
}

void ObjcShowWebViewInAppUnity(char *param)
{
    [[BRWebViewTool shared] BRShowWebViewInAppiOS:[NSString stringWithUTF8String:param]];
}

void ObjcEvaluateJavaScriptUnity(char *param)
{
    [[BRWebViewTool shared] BREvaluateJavaScriptiOS:[NSString stringWithUTF8String:param]];
}

void ObjcShowNewsVCUnity(char *param)
{
    [[BRWebViewTool shared] BRShowNewsVCiOS:[NSString stringWithUTF8String:param]];
}

void ObjcShareToWhatsAppUnity(char *param)
{
    [[BRShareTool shared] BRShareToWhatsAppiOS:[NSString stringWithUTF8String:param]];
}

void ObjcShareToMessageUnity(char *param)
{
    [[BRShareTool shared] BRShareToMessageiOS:[NSString stringWithUTF8String:param]];
}

void ObjcShareToDefaultUnity(char *param)
{
    [[BRShareTool shared] BRShareToDefaultiOS:[NSString stringWithUTF8String:param]];
}

void ObjcForterSetupWithSiteIdUnity(char* param)
{
    [[BRForterTool shared] SetupWithSiteId:[NSString stringWithUTF8String:param]];
}

void ObjcForterSetDeviceUniqueIdentifierUnity()
{
    [[BRForterTool shared] SetDeviceUniqueIdentifier];
}

void ObjcForterTrackActionUnity(char* param1, char* param2)
{
    [[BRForterTool shared] TrackAction:[NSString stringWithUTF8String:param1]:[NSString stringWithUTF8String:param2]];
}

void ObjcRiskifiedStartBeaconUnity(char* account, char* token, bool debug)
{
    [[BRRiskifiedTool shared] StartBeacon:[NSString stringWithUTF8String:account]:[NSString stringWithUTF8String:token]:debug];
}

void ObjcRiskifiedUpdateSessionTokenUnity(char* token)
{
    [[BRRiskifiedTool shared] UpdateSessionToken:[NSString stringWithUTF8String:token]];
}

void ObjcRiskifiedLogRequestUnity(char* url)
{
    NSString *urlstring = [NSString stringWithUTF8String:url];
    NSURL *newurl = [NSURL URLWithString:[urlstring stringByAddingPercentEncodingWithAllowedCharacters:[NSCharacterSet URLQueryAllowedCharacterSet]]];
    [[BRRiskifiedTool shared] LogRequest:newurl];
}




#if defined(__cplusplus)
}
#endif
