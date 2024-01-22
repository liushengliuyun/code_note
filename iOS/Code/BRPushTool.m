#import "BRPushTool.h"
#import "BRNativeTool.h"
#import "BRUnityFounctionConst.h"

#import <OneSignalFramework/OneSignalFramework.h>

@interface BRPushTool()

@property(nonatomic, strong)NSDictionary *options;

@end

@implementation BRPushTool

+(instancetype)shared {
    static BRPushTool *shareInstance;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        shareInstance = [[BRPushTool alloc] init];
    });
    return shareInstance;
}

-(void)BRSetOptions:(NSDictionary *)options {
    self.options = options;
}

-(NSString *)BRGetPushAuthiOS {
//    OSNotificationPermission status = [OneSignal getDeviceState].notificationPermissionStatus;
//    return [NSString stringWithFormat:@"%d", (int)status];
}

-(void)BRInitPushiOS {
//    [OneSignal setLogLevel:ONE_S_LL_NONE visualLevel:ONE_S_LL_NONE];
//    [OneSignal initWithLaunchOptions:self.options];
//    [OneSignal setAppId:@"8ca582c9-28e5-4973-9e7d-0c8c068ced50"];
//    [OneSignal promptForPushNotificationsWithUserResponse:^(BOOL accepted) {
//        [BRNativeTool NativeSendUnityMessageMethodiOS:CShapePushAuth msg: accepted ? @"2" : @"1"];
//    }];
}

-(void)BRSetTagsiOS:(NSString *)json {
//    NSDictionary *dict = [BRNativeTool NativeStringTodictiOS:json];
//    if (dict) {
//        [OneSignal sendTags:dict];
//    }
}

@end
