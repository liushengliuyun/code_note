//
//  BREmailTool.m
//  Unity-iPhone
//
//  Created by UnityDeveloper on 2023/1/9.
//

#import "BREmailTool.h"
#import "sys/utsname.h"
#import "BRNativeTool.h"
#import "UnityAppController.h"
#import "BRUnityFounctionConst.h"

#import <MessageUI/MessageUI.h>

@interface BREmailTool()<MFMailComposeViewControllerDelegate>

@end

@implementation BREmailTool

+(instancetype)shared {
    static BREmailTool *shareInstance;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        shareInstance = [[BREmailTool alloc] init];
    });
    return shareInstance;
}

-(BOOL)BRSendEmailiOS:(NSString *)json {
    if (![MFMailComposeViewController canSendMail]) {
        return NO;
    }
    
    // 设备信息
    struct utsname system;
    uname(&system);
    NSString *machine = [NSString stringWithCString:system.machine encoding:NSUTF8StringEncoding];
    NSString *version = [[UIDevice currentDevice] systemVersion];
    NSString *model = [[UIDevice currentDevice] model];

    // 邮箱内容
    NSDictionary *brdict = [BRNativeTool NativeStringTodictiOS:json];
    NSMutableString *content = [NSMutableString string];
    [content appendString:@"-------------------------------->\n"];
    [content appendFormat:@"ID: %@\n", brdict[@"id"]];
    [content appendFormat:@"AppVersion: %@\n", brdict[@"version"]];
    [content appendFormat:@"Machine: %@\n", machine];
    [content appendFormat:@"System: %@\n", version];
    [content appendFormat:@"Model: %@\n", model];
    [content appendFormat:@"Country: %@\n", brdict[@"code"]];
    [content appendFormat:@"Pos: %@\n", brdict[@"pos"]];
    [content appendString:@"<--------------------------------\n"];
    [content appendString:@"Please tell us how can we help you:"];
    
    [BRNativeTool NativeRunMainTheardiOS:^{
        MFMailComposeViewController *mail = [[MFMailComposeViewController alloc] init];
        mail.modalPresentationStyle = UIModalPresentationFullScreen;
        [mail setMailComposeDelegate:self];
        [mail setSubject:brdict[@"subject"]];
        [mail setTitle:brdict[@"title"]];
        [mail setToRecipients:@[brdict[@"email"]]];
        [mail setMessageBody:content isHTML:NO];
        [GetAppController().rootViewController presentViewController:mail animated:YES completion:nil];
    }];

    return  YES;
}

- (void)mailComposeController:(MFMailComposeViewController *)controller didFinishWithResult:(MFMailComposeResult)result error:(nullable NSError *)error {
    switch (result)
    {
        case MFMailComposeResultCancelled:  // 用户取消编辑
            NSLog(@"Mail cancel");
            [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeSendEmailFinish msg:@"1"];
            break;
        case MFMailComposeResultSaved:      // 用户保存邮件
            NSLog(@"Mail saved");
            [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeSendEmailFinish msg:@"2"];
            break;
        case MFMailComposeResultSent:       // 用户点击发送
            NSLog(@"Mail sent");
            [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeSendEmailFinish msg:@"3"];
            break;
        case MFMailComposeResultFailed:     // 用户尝试保存或发送邮件失败
            if (error) {
                NSLog(@"Mail error: %@", [error localizedDescription]);
            } else {
                NSLog(@"Mail error");
            }
            [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeSendEmailFinish msg:@"4"];
            break;
    }
    // 关闭邮件发送视图
    [controller dismissViewControllerAnimated:YES completion:nil];
}

@end
