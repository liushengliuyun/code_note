#import <Foundation/Foundation.h>

/// iOS Method Tool

NS_ASSUME_NONNULL_BEGIN

@interface BRiOSTool : NSObject

+(NSString *)NativeGetUDIDiOS;

+(NSString *)NativeGetTimeZoneiOS;

+(NSString *)NativeGetLanguageiOS;

+(NSString *)NativeGetATTStatusiOS;

+(NSString *)NativeGetDeviceInfoiOS;

+(NSString *)NativeGetLocalCountryCodeiOS;

+(NSString *)NativeGetSimInfoiOS;

+(NSString*)NativeGetIPAddress;

+(BOOL)NativeIsFullSecreeniOS;

+(BOOL)NativeIsEnableWIFI;

+(void)NativeGetNetworkAuth;

+(void)NativeStartVibrationiOS:(NSString *)param;

+(void)NativeRequestATTiOS;

+(void)NativeOpenEmailAppiOS;

+(void)NativeGotoRateiOS:(NSString *)param;

+(void)NativeGotoSettingiOS;

@end

NS_ASSUME_NONNULL_END
