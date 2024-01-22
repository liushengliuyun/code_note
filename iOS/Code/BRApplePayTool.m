//
//  BRApplePayTool.m
//  Unity-iPhone
//
//  Created by UnityDeveloper on 2023/1/9.
//

#import "BRApplePayTool.h"
#import "BRNativeTool.h"
#import "UnityAppController.h"
#import "BRUnityFounctionConst.h"

#import <PassKit/PassKit.h>

@interface BRApplePayTool()<PKPaymentAuthorizationViewControllerDelegate>

@property (nonatomic, strong)NSArray *networks;

@property (nonatomic, assign)BOOL result;

@end

@implementation BRApplePayTool

+(instancetype)shared {
    static BRApplePayTool *shareInstance;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        shareInstance = [[BRApplePayTool alloc] init];
    });
    return shareInstance;
}

- (instancetype)init
{
    self = [super init];
    if (self) {
        self.networks =@[PKPaymentNetworkAmex,
                         PKPaymentNetworkMasterCard,
                         PKPaymentNetworkVisa,
                         PKPaymentNetworkChinaUnionPay,
                         PKPaymentNetworkDiscover,
                         PKPaymentNetworkInterac,
                         PKPaymentNetworkPrivateLabel,
                         PKPaymentNetworkIDCredit];
    }
    return self;
}

- (BOOL)BRCanMakePaymentsiOS {
    return [PKPaymentAuthorizationViewController canMakePayments];
}

- (BOOL)BRCanSupportApplePayiOS {
    return [self BRCanMakePaymentsiOS] && [PKPaymentAuthorizationViewController canMakePaymentsUsingNetworks:self.networks];
}

- (void)BRStartPayiOS:(NSString *)json {
    NSDictionary *brdict = [BRNativeTool NativeStringTodictiOS:json];
    
    self.result = NO;

    // 订单数值
    NSString *str = [NSString stringWithFormat:@"%@", brdict[@"amount"]];
    NSDecimalNumber *number = [NSDecimalNumber decimalNumberWithString:str];
    PKPaymentSummaryItem *total = [PKPaymentSummaryItem summaryItemWithLabel:brdict[@"title"] amount:number ];
    
    // 创建订单
    PKPaymentRequest *payRequest = [[PKPaymentRequest alloc]init];
    payRequest.countryCode = @"US";
    payRequest.currencyCode = @"USD";
    payRequest.supportedNetworks = self.networks;
    payRequest.merchantIdentifier = brdict[@"merchant"];
    payRequest.paymentSummaryItems = [NSMutableArray arrayWithArray:@[total]];
    payRequest.merchantCapabilities = PKMerchantCapability3DS|PKMerchantCapabilityEMV|PKMerchantCapabilityCredit|PKMerchantCapabilityDebit;
    
    // 弹出支付
    [BRNativeTool NativeRunMainTheardiOS:^{
        PKPaymentAuthorizationViewController *view = [[PKPaymentAuthorizationViewController alloc]initWithPaymentRequest:payRequest];
        view.delegate = self;
        [GetAppController().rootViewController presentViewController:view animated:YES completion:nil];
    }];
}

-(void)paymentAuthorizationViewController:(PKPaymentAuthorizationViewController *)controller didAuthorizePayment:(PKPayment *)payment handler:(void (^)(PKPaymentAuthorizationResult *))completion {

    self.result = YES;

    NSString *string = [[NSString alloc] initWithData:payment.token.paymentData encoding:NSUTF8StringEncoding];
    NSString *base64 = [payment.token.paymentData base64EncodedStringWithOptions:NSDataBase64EncodingEndLineWithLineFeed];
    
    NSMutableDictionary *dict = [NSMutableDictionary dictionary];
    dict[@"str"] = string;
    dict[@"bas"] = base64;
    
    [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeApplePayAuthorization msg:[BRNativeTool NativeDictToStriOS:dict]];

    completion(PKPaymentAuthorizationStatusSuccess);
}

-(void)paymentAuthorizationViewControllerDidFinish:(PKPaymentAuthorizationViewController *)controller{
    [controller dismissViewControllerAnimated:YES completion:^{
        [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeApplePayFinish msg:self.result ? @"1" : @"0"];
    }];
}

@end
