#import <Foundation/Foundation.h>

/// Unity Call Objective-C

NS_ASSUME_NONNULL_BEGIN

#if defined(__cplusplus)
extern "C"{
#endif

extern char *ObjcCharMemoryManagementUnity(NSString *param);

extern char *ObjcGetUDIDUnity();

extern char *ObjcGetTimeZoneUnity();

extern char *ObjcGetLanguageUnity();

extern char *ObjcGetATTStatusUnity();

extern char *ObjcGetDeviceInfoUnity();

extern char* ObjcGetIPAddressUnity();

extern char *ObjcGetPasteBoardTextUnity();

extern char *ObjcGetLocationAuthorizationUnity();

extern char *ObjcGetLastLocationUnity();

extern char *ObjcGetAppsFlyerIdUnity();

extern char *ObjcGetDistinctIDUnity();

extern char *ObjcGetPushAuthUnity();

extern char *ObjcGetLocalCountryCodeUnity();

extern char *ObjcGetSimInfoUnity();

extern void ObjcInitKycUnity(char *param);

extern void ObjcUpdateKycTokenUnity(char *param);

extern bool ObjcIsFullSecreenUnity();

extern bool ObjcIsEnableWIFIUnity();

extern bool ObjcCanMakePaymentsUnity();

extern bool ObjcCanSupportApplePayUnity();

extern bool ObjcSendEmailUnity(char *param);

extern void ObjcStartPayUnity(char *param);

extern void ObjcGetTongDunBlackBoxUnity();

extern void ObjcInitPushUnity();

extern void ObjcSetTagsUnity(char *param);

extern void ObjcGetNetworkAuthUnity();

extern void ObjcRequestATTUnity();

extern void ObjcFlushDatasUnity();

extern void ObjcGetLocationUnity();

extern void ObjcGotoSettingUnity();

extern void ObjcCloseWebViewUnity();

extern void ObjcOpenEmailAppUnity();

extern void ObjcRequestLocationAuthorizationUnity();

extern void ObjcCalibrateTimeUnity(char *param);

extern void ObjcInitThinkUnity(char *param);

extern void ObjcLoginThinkUnity(char *param);

extern void ObjcStartAutoEventUnity(char *param);

extern void ObjcSetGameRoundsUnity(char *param);

extern void ObjcThinkUserSetUnity(char *param);

extern void ObjcThinkUserSetOnceUnity(char *param);

extern void ObjcThinkTrackUnity(char *param);

extern void ObjcStartAppsFlyerUnity();

extern void ObjcSendAppsFlyerEventUnity(char *param);

extern void ObjcSendAppsFlyerValueUnity(char *param);

extern void ObjcGotoRateUnity(char *param);

extern void ObjcStartVibrationUnity(char *param);

extern void ObjcStartPickerImageUnity(char *param);

extern void ObjcSetPasteBoardTextUnity(char *param);

extern void ObjcSetupLocationUnity(char *param);

extern void ObjcShowWebViewInAppSafariUnity(char *param);

extern void ObjcShowWebViewSafariUnity(char *param);

extern void ObjcShowWebViewInAppUnity(char *param);

extern void ObjcEvaluateJavaScriptUnity(char *param);

extern void ObjcShowNewsVCUnity(char *param);

extern void ObjcShareToWhatsAppUnity(char *param);

extern void ObjcShareToMessageUnity(char *param);

extern void ObjcShareToDefaultUnity(char *param);

extern void ObjcForterSetupWithSiteIdUnity(char* param);

extern void ObjcForterSetDeviceUniqueIdentifierUnity();

extern void ObjcForterTrackActionUnity(char* param1, char* param2);

extern void ObjcRiskifiedStartBeaconUnity(char* account, char* token, bool debug);

extern void ObjcRiskifiedUpdateSessionTokenUnity(char* token);

extern void ObjcRiskifiedLogRequestUnity(char* url);



#if defined(__cplusplus)
}
#endif

NS_ASSUME_NONNULL_END
