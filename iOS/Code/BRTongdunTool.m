//
//  BRTongdunTool.m
//  Unity-iPhone
//
//  Created by UnityDeveloper on 2023/3/23.
//

#import "BRTongdunTool.h"
#import "BRNativeTool.h"
#import "BRUnityFounctionConst.h"

//#import <FMDeviceManagerFramework/FMDeviceManager.h>

@implementation BRTongdunTool

+(instancetype)shared {
    static BRTongdunTool *shareInstance;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        shareInstance = [[BRTongdunTool alloc] init];
    });
    return shareInstance;
}

-(void)BRGetTongDunBlackBoxiOS {
    // 获取设备管理器实例
//    FMDeviceManager_t *manager = [FMDeviceManager sharedManager];
    
    // SDK初始化配置
    NSMutableDictionary *options = [NSMutableDictionary dictionary];
    
    /*************************** 必传 ***************************/
    [options setValue:@"winnerstudio" forKey:@"partner"];
    [options setValue:@"https://usfp.tongdun.net" forKey:@"domain"];
    
    /*************************** 选传 ***************************/
    [options setValue:@"7193754d5ddc43a2dd1964200d0a1d10" forKey:@"clientKey"];
    
    // 连线调试，需打开此行代码，上线请注释掉
//    [options setValue:@"allowd" forKey:@"allowd"];
    
    [options setValue:^(NSString *blackBox) {
        [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeGetTongDunBlackBoxFinish msg:blackBox];
    } forKey:@"callback"];
    
//    manager->initWithOptions(options);
}

@end
