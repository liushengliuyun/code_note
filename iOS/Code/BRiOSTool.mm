#import <UIKit/UIKit.h>
#import <StoreKit/StoreKit.h>
#import <Foundation/Foundation.h>
#import <CoreTelephony/CTCarrier.h>
#import <AudioToolbox/AudioToolbox.h>
#import <CoreTelephony/CTCellularData.h>
#import <SystemConfiguration/CaptiveNetwork.h>
#import <CoreTelephony/CTTelephonyNetworkInfo.h>
#import <AppTrackingTransparency/AppTrackingTransparency.h>
#import <ifaddrs.h>
#import <arpa/inet.h>

#import "sys/utsname.h"
#import "UnityAppController.h"

#import "BRiOSTool.h"
#import "BRNativeTool.h"
#import "BRUnityFounctionConst.h"

@implementation BRiOSTool

+(NSString *)NativeGetATTStatusiOS {
    if (@available(iOS 14, *)) {
        ATTrackingManagerAuthorizationStatus brstatus = ATTrackingManager.trackingAuthorizationStatus;
        if (brstatus == ATTrackingManagerAuthorizationStatusDenied) {
            return @"2";
        } else if (brstatus == ATTrackingManagerAuthorizationStatusAuthorized) {
            return @"1";
        } else if (brstatus == ATTrackingManagerAuthorizationStatusRestricted) {
            return @"3";
        } else if (brstatus == ATTrackingManagerAuthorizationStatusNotDetermined) {
            return @"0";
        }
    }
    return @"1";
}

+(NSString *)NativeGetUDIDiOS {
    NSString *bruuid = [self NativeGetUUID];
    if (bruuid) {
        return bruuid;
    } else {
        [self NativeSaveUUID];
        NSString *nbruuid = [self NativeGetUUID];
        if (nbruuid) {
            return nbruuid;
        } else {
            return @"get_udid_error";
        }
    }
}

+(NSString *)NativeGetTimeZoneiOS {
    return [NSTimeZone localTimeZone].name;
}

+(NSString *)NativeGetLanguageiOS {
    //[NSLocale currentLocale].languageCode;基于地区获取语言
    return  [NSLocale preferredLanguages][0]; // 最近设置的语言
}

+(NSString *)NativeGetDeviceInfoiOS {
    struct utsname system;
    uname(&system);
    NSString *machine = [NSString stringWithCString:system.machine encoding:NSUTF8StringEncoding];
    NSString *version = [[UIDevice currentDevice] systemVersion];
    NSString *model = [[UIDevice currentDevice] model];
    return  [NSString stringWithFormat: @"Machine:%@, System:%@, Model:%@", machine, version, model];
}

+(NSString *)NativeGetLocalCountryCodeiOS {
    return  [NSLocale currentLocale].countryCode;
}

+(NSString *)NativeGetSimInfoiOS {
    CTTelephonyNetworkInfo *telephonyInfo = [[CTTelephonyNetworkInfo alloc] init];
    NSMutableDictionary *sims = [NSMutableDictionary dictionary];
    if (@available(iOS 12.0, *)) {
        NSDictionary<NSString *, CTCarrier *> *carriers = [telephonyInfo serviceSubscriberCellularProviders];
        if (carriers) {
            for (int i = 0; i < [carriers allValues].count; i++) {
                CTCarrier *carrier = [carriers allValues][i];
                if (carrier && carrier.mobileNetworkCode) {
                    NSMutableDictionary *dict = [NSMutableDictionary dictionary];
                    dict[@"sim_name"] = carrier.carrierName;
                    dict[@"sim_country_code"] = carrier.isoCountryCode;
                    dict[@"sim_allow_voip"] = carrier.allowsVOIP ? @"1" : @"0";
                    dict[@"network_country_code"] = carrier.mobileCountryCode;
                    dict[@"network_name"] = carrier.mobileNetworkCode;
                    sims[[NSString stringWithFormat:@"isp%d", i + 1]] = dict;
                }
            }
        }
    } else {
        CTCarrier *carrier = [telephonyInfo subscriberCellularProvider];
        if (carrier && carrier.mobileNetworkCode) {
            NSMutableDictionary *dict = [NSMutableDictionary dictionary];
            dict[@"sim_name"] = carrier.carrierName;
            dict[@"sim_country_code"] = carrier.isoCountryCode;
            dict[@"sim_allow_voip"] = carrier.allowsVOIP ? @"1" : @"0";
            dict[@"network_country_code"] = carrier.mobileCountryCode;
            dict[@"network_name"] = carrier.mobileNetworkCode;
            sims[@"isp1"] = dict;
        }
    }
    return [BRNativeTool NativeDictToStriOS:sims];
}

// Get IP Address
- (NSString *)NativeGetIPAddress {
    NSString *address = @"error";
    struct ifaddrs *interfaces = NULL;
    struct ifaddrs *temp_addr = NULL;
    int success = 0;
    // retrieve the current interfaces - returns 0 on success
    success = getifaddrs(&interfaces);
    if (success == 0) {
        // Loop through linked list of interfaces
        temp_addr = interfaces;
        while(temp_addr != NULL) {
            if(temp_addr->ifa_addr->sa_family == AF_INET) {
                // Check if interface is en0 which is the wifi connection on the iPhone
                if([[NSString stringWithUTF8String:temp_addr->ifa_name] isEqualToString:@"en0"]) {
                    // Get NSString from C String
                    address = [NSString stringWithUTF8String:inet_ntoa(((struct sockaddr_in *)temp_addr->ifa_addr)->sin_addr)];               
                }
            }
            temp_addr = temp_addr->ifa_next;
        }
    }
    // Free memory
    freeifaddrs(interfaces);
    return address;
}

+(BOOL)NativeIsFullSecreeniOS {
    if (UIDevice.currentDevice.userInterfaceIdiom != UIUserInterfaceIdiomPhone) {
        return NO;
    }
    if (@available(iOS 11.0, *)) {
        UIWindow *mainWindow = GetAppController().window;
        if (mainWindow.safeAreaInsets.bottom > 0.0) {
            return YES;
        }
    }
    return NO;
}

+(BOOL)NativeIsEnableWIFI {
    NSArray *interfaces = (__bridge_transfer NSArray *)CNCopySupportedInterfaces();
    if (!interfaces) {
        return NO;
    }
    NSDictionary *info = nil;
    for (NSString *ifnam in interfaces) {
        info = (__bridge_transfer NSDictionary *)CNCopyCurrentNetworkInfo((__bridge CFStringRef)ifnam);
        if (info && [info count]) { break; }
    }
    return (info != nil);
}

+(void)NativeGetNetworkAuth {
    CTCellularData *cellular = [[CTCellularData alloc] init];
    cellular.cellularDataRestrictionDidUpdateNotifier = ^(CTCellularDataRestrictedState restrictedState) {
        switch (restrictedState) {
            case kCTCellularDataRestrictedStateUnknown:
                [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeUserNetworkAuth msg:@"0"];
                break;
            case kCTCellularDataRestricted:
                [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeUserNetworkAuth msg:@"1"];
                break;
            default:
                [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeUserNetworkAuth msg:@"2"];
                break;
        }
    };
}

+(void)NativeRequestATTiOS {
    if (@available(iOS 14, *)) {
        [BRNativeTool NativeRunMainTheardiOS:^{
            [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler: ^(ATTrackingManagerAuthorizationStatus status) {
                if (status == ATTrackingManagerAuthorizationStatusDenied) {
                    [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeTrackingAuthorizationWithCompletion msg:@"2"];
                } else if (status == ATTrackingManagerAuthorizationStatusAuthorized) {
                    [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeTrackingAuthorizationWithCompletion msg:@"1"];
                } else if (status == ATTrackingManagerAuthorizationStatusRestricted) {
                    [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeTrackingAuthorizationWithCompletion msg:@"3"];
                } else if (status == ATTrackingManagerAuthorizationStatusNotDetermined) {
                    [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeTrackingAuthorizationWithCompletion msg:@"0"];
                }
            }];
        }];
    }
}

+(void)NativeOpenEmailAppiOS {
    [BRNativeTool NativeRunMainTheardiOS:^{
        [[UIApplication sharedApplication] openURL: [NSURL URLWithString:@"message://"] options:@{} completionHandler:nil];
    }];
}

+(void)NativeStartVibrationiOS:(NSString *)param {
    if ([param isEqualToString:@"Light"]) {
        UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc] initWithStyle: UIImpactFeedbackStyleLight];
        [generator prepare];
        [generator impactOccurred];
    } else if ([param isEqualToString:@"Medium"]) {
        UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc] initWithStyle: UIImpactFeedbackStyleMedium];
        [generator prepare];
        [generator impactOccurred];
    } else if ([param isEqualToString:@"Heavy"]) {
        UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc] initWithStyle: UIImpactFeedbackStyleHeavy];
        [generator prepare];
        [generator impactOccurred];
    } else if ([param isEqualToString:@"Success"]) {
        UINotificationFeedbackGenerator *generator = [[UINotificationFeedbackGenerator alloc] init];
        [generator notificationOccurred:UINotificationFeedbackTypeSuccess];
    } else if ([param isEqualToString:@"Warning"]) {
        UINotificationFeedbackGenerator *generator = [[UINotificationFeedbackGenerator alloc] init];
        [generator notificationOccurred:UINotificationFeedbackTypeWarning];
    } else if ([param isEqualToString:@"Error"]) {
        UINotificationFeedbackGenerator *generator = [[UINotificationFeedbackGenerator alloc] init];
        [generator notificationOccurred:UINotificationFeedbackTypeError];
    } else if ([param isEqualToString:@"Changed"]) {
        UISelectionFeedbackGenerator *generator = [[UISelectionFeedbackGenerator alloc] init];
        [generator selectionChanged];
    } else {
        AudioServicesPlaySystemSound(kSystemSoundID_Vibrate);
    }
}

+(void)NativeGotoSettingiOS {
    if (@available(iOS 10 , *)) {
        NSURL * brurl = [NSURL URLWithString:UIApplicationOpenSettingsURLString];
        if ([[UIApplication sharedApplication] canOpenURL:brurl]) {
            [[UIApplication sharedApplication] openURL:brurl options:@{} completionHandler:^(BOOL success) {}];
        }
    }
}

#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wdeprecated-declarations"
+(void)NativeGotoRateiOS:(NSString *)param {
    [BRNativeTool NativeRunMainTheardiOS:^{
        if (@available(iOS 10.3, *))
        {
            [SKStoreReviewController requestReview];
        }
        else
        {
            NSString *appURL = [NSString stringWithFormat: @"https://itunes.apple.com/us/app/id%@?action=write-review", param];
            [[UIApplication sharedApplication] openURL:[NSURL URLWithString:appURL]];
        }
    }];
}
#pragma clang diagnostic pop

#pragma mark -- private
+(NSString *)NativeGetUUID {
    NSDictionary *query = @{(__bridge id)kSecClass : (__bridge id)kSecClassGenericPassword,
                            (__bridge id)kSecReturnData : @YES,
                            (__bridge id)kSecMatchLimit : (__bridge id)kSecMatchLimitOne,
                            (__bridge id)kSecAttrAccount : ConstAccount,
                            (__bridge id)kSecAttrService : ConstPassword};
    CFTypeRef dataTypeRef = NULL;
    OSStatus status = SecItemCopyMatching((__bridge CFDictionaryRef)query, &dataTypeRef);
    if (status == errSecSuccess) {
        NSString *uuid = [[NSString alloc] initWithData:(__bridge NSData * _Nonnull)(dataTypeRef) encoding:NSUTF8StringEncoding];
        NSLog(@"UUID: %@", uuid);
        return uuid;
    } else {
        return nil;
    }
}

+(void)NativeSaveUUID {
    NSDictionary *query = @{(__bridge id)kSecAttrAccessible : (__bridge id)kSecAttrAccessibleWhenUnlocked,
                            (__bridge id)kSecClass : (__bridge id)kSecClassGenericPassword,
                            (__bridge id)kSecValueData : [[[NSUUID UUID] UUIDString] dataUsingEncoding:NSUTF8StringEncoding],
                            (__bridge id)kSecAttrAccount : ConstAccount,
                            (__bridge id)kSecAttrService : ConstPassword};
    SecItemAdd((__bridge CFDictionaryRef)query, nil);
}

@end
