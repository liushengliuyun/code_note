#import "BRShuShuTool.h"
#import "BRNativeTool.h"
#import "UnityAppController.h"

#import <ThinkingSDK.h>

@interface BRShuShuTool()

@property(nonatomic, strong)NSString *debug;

@property(nonatomic, strong)NSString *userid;

@property(nonatomic, strong)NSString *sessionid;

@property(nonatomic, strong)ThinkingAnalyticsSDK *releases;

@property(nonatomic, strong)ThinkingAnalyticsSDK *debugger;

@end

@implementation BRShuShuTool

static BRShuShuTool *shareInstance;

+(instancetype)shared {
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        shareInstance = [[BRShuShuTool alloc] init];
    });
    return shareInstance;
}

-(NSString *)BRGetDistinctIDiOS {
    if ([self.debug isEqualToString:@"1"] && [self BRCheckReaday:self.debugger])
    {
        return [self.debugger getDistinctId];
    }
    else if ([self.debug isEqualToString:@"2"] && [self BRCheckReaday:self.releases])
    {
        return [self.releases getDistinctId];
    }
    else
    {
        return @"Thinking not inited";
    }
}

-(void)BRFlushDatasiOS {
    if ([self.debug isEqualToString:@"1"] && [self BRCheckReaday:self.debugger])
    {
        [self.debugger flush];
    }
    else if ([self.debug isEqualToString:@"2"] && [self BRCheckReaday:self.releases])
    {
        [self.releases flush];
    }
}

-(void)BRCalibrateTimeiOS:(NSString *)time {
    [ThinkingAnalyticsSDK calibrateTime: [time doubleValue]];
}

-(void)BRInitThinkiOS:(NSString *)debug {
    self.debug = debug;
    if ([self.debug isEqualToString:@"1"] && !self.debugger)
    {
        self.debugger = [ThinkingAnalyticsSDK startWithAppId:@"908047df509947fb93a57e45f7f65802" withUrl:@"https://td.apexdynamics.club"];
    }
    else if ([self.debug isEqualToString:@"2"] && !self.releases)
    {
        self.releases = [ThinkingAnalyticsSDK startWithAppId:@"2c9519cdeee7457cb1cb88501a67385f" withUrl:@"https://td.apexdynamics.club"];
    }
}

-(void)BRLoginThinkiOS:(NSString *)userid {
    self.userid = userid;
    if ([self.debug isEqualToString:@"1"] && [self BRCheckReaday:self.debugger])
    {
        [self.debugger login:userid];
    }
    else if ([self.debug isEqualToString:@"2"] && [self BRCheckReaday:self.releases])
    {
        [self.releases login:userid];
    }
}

-(void)BRStartAutoEventiOS:(NSString *)time {
    self.sessionid = [NSString stringWithFormat:@"%@_%@", self.userid, time];
    if ([self.debug isEqualToString:@"1"] && [self BRCheckReaday:self.debugger])
    {
        NSMutableDictionary *dict1 = [NSMutableDictionary dictionary];
        dict1[@"session_id"] = self.sessionid;
        [self.debugger setAutoTrackProperties:ThinkingAnalyticsEventTypeAppStart properties:dict1];
        
        NSMutableDictionary *dict2 = [NSMutableDictionary dictionary];
        dict2[@"session_id"] = self.sessionid;
        dict2[@"game_rounds"] = [NSNumber numberWithInteger: 0];
        [self.debugger setAutoTrackProperties:ThinkingAnalyticsEventTypeAppEnd properties:dict2];
        
        [self.debugger enableAutoTrack:ThinkingAnalyticsEventTypeAppStart | ThinkingAnalyticsEventTypeAppEnd];
    }
    else if ([self.debug isEqualToString:@"2"] && [self BRCheckReaday:self.releases])
    {
        NSMutableDictionary *dict1 = [NSMutableDictionary dictionary];
        dict1[@"session_id"] = self.sessionid;
        [self.releases setAutoTrackProperties:ThinkingAnalyticsEventTypeAppStart properties:dict1];
        
        NSMutableDictionary *dict2 = [NSMutableDictionary dictionary];
        dict2[@"session_id"] = self.sessionid;
        dict2[@"game_rounds"] = [NSNumber numberWithInteger: 0];
        [self.releases setAutoTrackProperties:ThinkingAnalyticsEventTypeAppEnd properties:dict2];
        
        [self.releases enableAutoTrack:ThinkingAnalyticsEventTypeAppStart | ThinkingAnalyticsEventTypeAppEnd];
    }
}

-(void)BRSetGameRoundsiOS:(NSString *)value {
    if ([self.debug isEqualToString:@"1"] && [self BRCheckReaday:self.debugger])
    {
        NSMutableDictionary *dict = [NSMutableDictionary dictionary];
        dict[@"session_id"] = self.sessionid;
        dict[@"game_rounds"] = [NSNumber numberWithInteger: [value integerValue]];
        [self.debugger setAutoTrackProperties:ThinkingAnalyticsEventTypeAppEnd properties:dict];
    }
    else if ([self.debug isEqualToString:@"2"] && [self BRCheckReaday:self.releases])
    {
        NSMutableDictionary *dict = [NSMutableDictionary dictionary];
        dict[@"session_id"] = self.sessionid;
        dict[@"game_rounds"] = [NSNumber numberWithInteger: [value integerValue]];
        [self.releases setAutoTrackProperties:ThinkingAnalyticsEventTypeAppEnd properties:dict];
    }
}

-(void)BRThinkUserSetiOS:(NSString *)json {
    NSDictionary *dict = [BRNativeTool NativeStringTodictiOS:json];
    if ([self.debug isEqualToString:@"1"] && [self BRCheckReaday:self.debugger])
    {
        [self.debugger user_set:dict];
    }
    else if ([self.debug isEqualToString:@"2"] && [self BRCheckReaday:self.releases])
    {
        [self.releases user_set:dict];
    }
}

-(void)BRThinkUserSetOnceiOS:(NSString *)json {
    NSDictionary *dict = [BRNativeTool NativeStringTodictiOS:json];
    if ([self.debug isEqualToString:@"1"] && [self BRCheckReaday:self.debugger])
    {
        [self.debugger user_setOnce:dict];
    }
    else if ([self.debug isEqualToString:@"2"] && [self BRCheckReaday:self.releases])
    {
        [self.releases user_setOnce:dict];
    }
}

-(void)BRThinkTrackiOS:(NSString *)json {
    NSDictionary *dict = [BRNativeTool NativeStringTodictiOS:json];
    if ([self.debug isEqualToString:@"1"] && [self BRCheckReaday:self.debugger])
    {
        [self.debugger track:dict[@"name"] properties:dict[@"dict"]];
    }
    else if ([self.debug isEqualToString:@"2"] && [self BRCheckReaday:self.releases])
    {
        [self.releases track:dict[@"name"] properties:dict[@"dict"]];
    }
}

#pragma mark -- private
-(BOOL)BRCheckReaday:(ThinkingAnalyticsSDK *)sdk {
    if (!sdk) {
        NSLog(@"Think not inited");
        UIAlertController *ac = [UIAlertController alertControllerWithTitle:NULL message:@"Think not init" preferredStyle:UIAlertControllerStyleAlert];
        UIAlertAction *aa = [UIAlertAction actionWithTitle:@"ok" style:UIAlertActionStyleCancel handler:^(UIAlertAction * _Nonnull action) { }];
        [ac addAction:aa];
        [GetAppController().rootViewController presentViewController:ac animated:YES completion:nil];
        return NO;
    }
    return YES;
}

@end
