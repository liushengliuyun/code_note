#import <AppsFlyerLib/AppsFlyerLib.h>

#import "BRiOSTool.h"
#import "BRNativeTool.h"
#import "BRAppsflyerTool.h"
#import "BRUnityFounctionConst.h"

@interface BRAppsflyerTool()<AppsFlyerLibDelegate>

@end

@implementation BRAppsflyerTool

-(void)BRStartAppsFlyeriOS {
    NSLog(@"wjs BRStartAppsFlyeriOS");
    [AppsFlyerLib shared].appsFlyerDevKey = @"fbFzQ4SPxJXoYBmHhaZbAa";
    [AppsFlyerLib shared].appleAppID = @"6474658561";
    [AppsFlyerLib shared].delegate = self;
    [AppsFlyerLib shared].isDebug = NO;
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(BRSendLaunchiOS:) name:UIApplicationDidBecomeActiveNotification object:nil];
}

+(instancetype)shared {
    static BRAppsflyerTool *shareInstance;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        shareInstance = [[BRAppsflyerTool alloc] init];
    });
    return shareInstance;
}

+(NSString *)BRGetAppsFlyerIdiOS {
    return [[AppsFlyerLib shared] getAppsFlyerUID];
}

+(void)BRSendAppsFlyerEventiOS:(NSString *)json {
    NSDictionary *dict = [BRNativeTool NativeStringTodictiOS:json];
    [[AppsFlyerLib shared] logEvent:dict[@"name"] withValues:dict[@"dict"]];
}

+(void)BRSendAppsFlyerValueiOS:(NSString *)json {
    NSDictionary *dictjson = [BRNativeTool NativeStringTodictiOS:json];
    NSMutableDictionary *dict = [NSMutableDictionary dictionary];
    dict[@"af_currency"] = @"USD";
    dict[@"af_revenue"] = dictjson[@"value"];
    dict[@"af_quantity"] = @"1";
    [[AppsFlyerLib shared] logEvent:dictjson[@"name"] withValues:dict];
}

#pragma mark -- private
-(void)BRSendLaunchiOS:(UIApplication *)application {
    [self onAFWaitATT];
    [self performSelector:@selector(onRequestATT) withObject:nil afterDelay:2];
    [[AppsFlyerLib shared] start];
}

- (void)onConversionDataSuccess:(nonnull NSDictionary *)conversionInfo {
    NSLog(@"wjs onConversionDataSuccess");
    id status = [conversionInfo objectForKey:@"af_status"];
    NSMutableDictionary *dict = [NSMutableDictionary dictionary];
    if([status isEqualToString:@"Non-organic"]) {
        id sourceID = [conversionInfo objectForKey:@"media_source"];
        id campaign = [conversionInfo objectForKey:@"campaign"];
        id inviteCD = [conversionInfo objectForKey:@"invite_code"];
        NSLog(@"[AF Non-organic] source: %@ campaign: %@, invite: %@",sourceID, campaign, inviteCD);
        dict[@"status"] = status;
        dict[@"source"] = sourceID ? sourceID : @"";
        dict[@"invite"] = inviteCD ? inviteCD : @"";
        dict[@"campaign"] = campaign ? campaign : @"";
    } else if([status isEqualToString:@"Organic"]) {
        NSLog(@"[AF Organic]");
        dict[@"status"] = status;
        dict[@"source"] = @"";
        dict[@"invite"] = @"";
        dict[@"campaign"] = @"";
    }
    NSString *json = [BRNativeTool NativeDictToStriOS:dict];
    [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeAppsflyerSuccess msg:json];
    [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeAppsflyerSuccessJson msg:json];
}

- (void)onConversionDataFail:(nonnull NSError *)error {
    NSLog(@"wjs onConversionDataFail");
    NSString *msg = [NSString stringWithFormat:@"%@", error];
    [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeAppsflyerFailure msg:msg];
}

- (void)onAppOpenAttribution:(NSDictionary *)attributionData {
    NSLog(@"%@",attributionData);
}

- (void)onAppOpenAttributionFailure:(NSError *)error {
    NSLog(@"%@",error);
}

- (void)onAFWaitATT {
    if (@available(iOS 14, *)) {
        [[AppsFlyerLib shared] waitForATTUserAuthorizationWithTimeoutInterval:60];
    }
}

- (void)onRequestATT {
    NSString *status = [BRiOSTool NativeGetATTStatusiOS];
    if ([status isEqualToString:@"0"]) {
        NSLog(@"ATT Request");
        [BRiOSTool NativeRequestATTiOS];
        [self performSelector:@selector(onRequestATT) withObject:nil afterDelay:1];
    } else {
        NSLog(@"ATT Did Show");
    }
}

@end
