
#import "BRForterTool.h"
#import <ForterSDK/ForterSDK.h>

@implementation BRForterTool

+(instancetype)shared {
    static BRForterTool *shareInstance;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        shareInstance = [[BRForterTool alloc] init];
    });
    return shareInstance;
}

-(void)SetupWithSiteId:(NSString*)param {
    [ForterSDK setupWithSiteId:param];
}

-(void)SetDeviceUniqueIdentifier {
    [[ForterSDK sharedInstance] setDeviceUniqueIdentifier:UIDevice.currentDevice.identifierForVendor.UUIDString];
}

-(void)TrackAction:(NSString*)eventName :(NSString*)eventParams{
    if ([eventName isEqualToString:@"Account_Login"])
    {
        [[ForterSDK sharedInstance] trackAction:FTRSDKActionTypeAccountLogin
                                    withMessage:eventParams];
    }else if ([eventName isEqualToString:@"Account_Id_Added"])
    {
        [[ForterSDK sharedInstance] trackAction:FTRSDKActionTypeAccountIdAdded
                                    withMessage:eventParams];
    }else if ([eventName isEqualToString:@"Add_To_Cart"])
    {
        [[ForterSDK sharedInstance] trackAction:FTRSDKActionTypeAddToCart
                                    withMessage:eventParams];
    }else if ([eventName isEqualToString:@"Payment_Info"])
    {
        [[ForterSDK sharedInstance] trackAction:FTRSDKActionTypePaymentInfo
                                    withMessage:eventParams];
    }
    else
    {
        [[ForterSDK sharedInstance] trackAction:FTRSDKActionTypeOther
                                    withMessage:eventParams];
    }
}


@end
