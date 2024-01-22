//
//  BRKycTool.m
//  Unity-iPhone
//
//  Created by UnityDeveloper on 2023/3/23.
//
#import "UnityAppController.h"
#import "BRKycTool.h"
#import "BRNativeTool.h"
#import "BRUnityFounctionConst.h"
#import <IdensicMobileSDK/IdensicMobileSDK.h>

@interface BRTheme : SNSTheme

@end

@implementation BRTheme

-(instancetype)init {
    self = [super init];
    if (self)
    {
        
    }
    return self;
}

@end

typedef void(^KycUpdateToken)(NSString * token);

@interface BRKycTool()

@property(nonatomic, strong)SNSMobileSDK *kycsdk;

@property(nonatomic, strong)KycUpdateToken tokenblock;

@end

@implementation BRKycTool

static BRKycTool *shareInstance;
+(instancetype)shared {
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        shareInstance = [[BRKycTool alloc] init];
    });
    return shareInstance;
}

-(void)BRInitKyciOS:(NSString *)json {
    NSDictionary *brdict = [BRNativeTool NativeStringTodictiOS:json];
    self.kycsdk = [SNSMobileSDK setupWithAccessToken:brdict[@"token"]];
    
    [self setupLogging: brdict[@"debug"]];
    [self setupHandlers];
    [self setupCallbacks];
    [self setupSupportItems];
    [self setupLocalization];
    [self setupTheme];
    [self BRBeginKyciOS];
}

-(void)BRUpdateKycTokeniOS:(NSString *)token {
    if (self.tokenblock) {
        NSLog(@"KYCiOS 更新Token");
        self.tokenblock(token);
        self.tokenblock = nil;
    }
}

-(void)BRBeginKyciOS {
    if ([self.kycsdk isReady]) {
        [GetAppController().rootViewController presentViewController:self.kycsdk.mainVC animated:YES completion:nil];
    } else {
        [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeKYCFinish msg:@"0"];
    }
}

// 日志输出
-(void)setupLogging:(NSString *)debug {
    self.kycsdk.logLevel = [debug isEqualToString:@"1"] ? SNSLogLevel_Debug : SNSLogLevel_Off;
    [self.kycsdk logHandler:^(SNSLogLevel level, NSString * _Nonnull message) {
        NSLog(@"KYCLog %@", message);
    }];
}

// 操作回调
-(void)setupHandlers {
    // Token失效
    [self.kycsdk tokenExpirationHandler:^(void (^ _Nonnull onComplete)(NSString * _Nullable)) {
        NSLog(@"KYCiOS Token已失效，请求服务器更新Token");
        self.tokenblock = onComplete;
        [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeKYCUpdateToken msg:@""];
    }];
    
    // 认证结果
    [self.kycsdk verificationHandler:^(BOOL isApproved) {
        NSLog(@"KYCiOS 是否同意验证: %d", isApproved);
    }];
    
    // 关闭回调
    [self.kycsdk dismissHandler:^(SNSMobileSDK * _Nonnull sdk, UINavigationController * _Nonnull mainVC) {
        NSLog(@"KYCiOS 关闭验证弹框");
        [[mainVC presentingViewController] dismissViewControllerAnimated:YES completion:nil];
    }];
    
    // 结果回调
    [self.kycsdk actionResultHandler:^(SNSMobileSDK * _Nonnull sdk, SNSActionResult * _Nonnull result, void (^ _Nonnull onComplete)(SNSActionResultHandlerReaction)) {
        NSLog(@"KYCiOS 处理验证数据, id: %@ qa: %@", result.actionId, result.answer);
        onComplete(SNSActionResultHandlerReaction_Continue);
    }];
}

// 事件通知
-(void)setupCallbacks {
    // 状态改变
    [self.kycsdk onStatusDidChange:^(SNSMobileSDK * _Nonnull sdk, SNSMobileSDKStatus prevStatus) {
        switch (sdk.status) {
            case SNSMobileSDKStatus_Ready:
                NSLog(@"KYCiOS 初始化成功");
                break;
            case SNSMobileSDKStatus_Failed:
            {
                NSString *prevs = [sdk descriptionForStatus:prevStatus];
                NSString *error = [sdk descriptionForFailReason:sdk.failReason];
                NSLog(@"KYCiOS 初始化失败: %@, Error: %@", prevs, error);
            }
                break;
            case SNSMobileSDKStatus_Initial:
                NSLog(@"KYCiOS 完成初始化");
                break;
            case SNSMobileSDKStatus_Pending:
                NSLog(@"KYCiOS 等待审核");
                break;
            case SNSMobileSDKStatus_Approved:
                NSLog(@"KYCiOS 同意验证");
                break;
            case SNSMobileSDKStatus_Incomplete:
                NSLog(@"KYCiOS 部分完成");
                break;
            case SNSMobileSDKStatus_ActionCompleted:
                NSLog(@"KYCiOS 全部完成");
                break;
            case SNSMobileSDKStatus_FinallyRejected:
                NSLog(@"KYCiOS 最终验证失败");
                break;
            case SNSMobileSDKStatus_TemporarilyDeclined:
                NSLog(@"KYCiOS 暂时验证失败");
                break;
        }
    }];
    
    // 通知事件
    [self.kycsdk onEvent:^(SNSMobileSDK * _Nonnull sdk, SNSEvent * _Nonnull event) {
        switch (event.eventType) {
            case SNSEventType_Analytics:
                NSLog(@"KYCiOS Analytics");
                break;
            case SNSEventType_StepCompleted:
                NSLog(@"KYCiOS StepCompleted");
                break;
            case SNSEventType_StepInitiated:
                NSLog(@"KYCiOS StepInitiated");
                break;
            case SNSEventType_ApplicantLoaded:
                NSLog(@"KYCiOS ApplicantLoaded");
                break;
        }
    }];
    
    // 关闭事件
    [self.kycsdk onDidDismiss:^(SNSMobileSDK * _Nonnull sdk) {
        if (sdk.status == SNSMobileSDKStatus_ActionCompleted) {
            if (sdk.actionResult) {
                NSLog(@"KYCiOS 最终结果, id: %@, qa: %@", sdk.actionResult.actionId, sdk.actionResult.answer);
                [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeKYCFinish msg:@"1"];
            } else {
                NSLog(@"KYCiOS 取消验证");
                [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeKYCFinish msg:@"0"];
            }
        } else if (sdk.status == SNSMobileSDKStatus_Pending) {
            NSLog(@"KYCiOS 等待审核");
            [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeKYCFinish msg:@"1"];
        } else {
            if (sdk.isFailed) {
                NSString *status = [sdk descriptionForStatus:sdk.status];
                NSString *reason = [sdk descriptionForFailReason:sdk.failReason];
                NSLog(@"KYCiOS 验证失败: %@, Error: %@", status, reason);
            } else {
                NSLog(@"KYCiOS 当前状态: %@", [sdk descriptionForStatus:sdk.status]);
            }
            [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeKYCFinish msg:@"0"];
        }
    }];
}

// 设置外置链接
-(void)setupSupportItems {
//    [self.kycsdk addSupportItemWithBlock:^(SNSSupportItem * _Nonnull item) {
//
//    }];
}

// 设置多语言
-(void)setupLocalization {
//    self.kycsdk.locale = @"";
}

// 设置主题
-(void)setupTheme {
    self.kycsdk.theme = [[BRTheme alloc] init];
}

@end
