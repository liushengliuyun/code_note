#import <MessageUI/MessageUI.h>

#import "BRiOSTool.h"
#import "BRShareTool.h"
#import "BRNativeTool.h"
#import "BRUnityFounctionConst.h"

#import "UnityAppController.h"

@interface BRShareTool()<MFMessageComposeViewControllerDelegate>

@end

@implementation BRShareTool

+(instancetype)shared {
    static BRShareTool *shareInstance;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        shareInstance = [[BRShareTool alloc] init];
    });
    return shareInstance;
}

-(void)BRShareToWhatsAppiOS:(NSString *)param
{
    [BRNativeTool NativeRunMainTheardiOS:^{
        NSString *brtext = [param stringByAddingPercentEncodingWithAllowedCharacters:[NSCharacterSet URLQueryAllowedCharacterSet]];
        brtext = [brtext stringByReplacingOccurrencesOfString:@"&" withString:@"%26"];
        NSString *brurl = [NSString stringWithFormat:@"whatsapp://send?text=%@", brtext];
        NSString *brweb = [NSString stringWithFormat:@"https://wa.me/?text=%@", brtext];
        if ([[UIApplication sharedApplication] canOpenURL: [NSURL URLWithString:brurl]]) {
            [[UIApplication sharedApplication] openURL:[NSURL URLWithString:brurl] options:@{} completionHandler:nil];
        } else {
            [[UIApplication sharedApplication] openURL:[NSURL URLWithString:brweb] options:@{} completionHandler:nil];
        }
    }];
}

-(void)BRShareToMessageiOS:(NSString *)param
{
    [BRNativeTool NativeRunMainTheardiOS:^{
        if ([MFMessageComposeViewController canSendText]) {
            MFMessageComposeViewController *brmessageViewController = [[MFMessageComposeViewController alloc]init];
            brmessageViewController.messageComposeDelegate =self;
            brmessageViewController.body = param;
            [GetAppController().rootViewController presentViewController:brmessageViewController animated:YES completion:nil];
        } else {
            [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeMessageShareNotSupport msg:@"s1"];
        }
    }];
}

-(void)BRShareToDefaultiOS:(NSString *)param
{
    [BRNativeTool NativeRunMainTheardiOS:^{
        NSDictionary *brdict = [BRNativeTool NativeStringTodictiOS:param];
        if (brdict) {
            NSArray *activityItemsArray = @[brdict[@"content"]];
            UIActivityViewController *bractivityVC = [[UIActivityViewController alloc] initWithActivityItems:activityItemsArray applicationActivities:nil];
            bractivityVC.title = brdict[@"title"];
            bractivityVC.modalInPopover = YES;
            bractivityVC.completionWithItemsHandler = ^(UIActivityType  _Nullable activityType, BOOL completed, NSArray * _Nullable returnedItems, NSError * _Nullable activityError) {
                NSMutableDictionary *brdict = [NSMutableDictionary dictionary];
                if (activityType) {
                    brdict[@"activityType"] = [NSString stringWithFormat:@"%@", activityType];
                }
                brdict[@"completed"] = completed ? @"1" : @"0";
                [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeDefaultShareDismissed msg: [BRNativeTool NativeDictToStriOS:brdict]];
            };
            [GetAppController().rootViewController presentViewController:bractivityVC animated:YES completion:nil];
        }
    }];
}

#pragma mark -- private
- (void)messageComposeViewController:(MFMessageComposeViewController *)controller didFinishWithResult:(MessageComposeResult)result
{
    NSString *brcode = @"0";
    switch (result) {
        case MessageComposeResultCancelled:{
            brcode = @"100";
            break;
        }
        case MessageComposeResultSent:{
            brcode = @"200";
            break;
        }
        case MessageComposeResultFailed:{
            brcode = @"300";
            break;
        }
    }
    [controller dismissViewControllerAnimated:YES completion:^{
        [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeMessageShareDismissed msg: brcode];
    }];
}

@end
