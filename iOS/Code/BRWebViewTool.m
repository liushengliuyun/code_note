#import <SafariServices/SafariServices.h>

#import "BRiOSTool.h"
#import "BRNativeTool.h"
#import "BRWebViewTool.h"
#import "BRWebVC.h"
#import "BRNewsVC.h"
#import "BRUnityFounctionConst.h"

#import "UnityAppController.h"

@interface BRWebViewTool()<SFSafariViewControllerDelegate>

@property(strong, nonatomic)BRWebVC *brwvc;

@end

@implementation BRWebViewTool

+(instancetype)shared {
    static BRWebViewTool *shareInstance;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        shareInstance = [[BRWebViewTool alloc] init];
    });
    return shareInstance;
}

-(void)BRShowWebViewInAppSafariiOS:(NSString *)param
{
    [BRNativeTool NativeRunMainTheardiOS:^{
        NSDictionary *dict = [BRNativeTool NativeStringTodictiOS:param];
        if (dict) {
            SFSafariViewController *brvc = [[SFSafariViewController alloc] initWithURL:[NSURL URLWithString:dict[@"url"]]];
            brvc.preferredBarTintColor = [BRNativeTool NativeColorStriOS:dict[@"barTintColor"]];
            brvc.preferredControlTintColor =  [BRNativeTool NativeColorStriOS:dict[@"controlTintColor"]];
            brvc.delegate = self;
            [GetAppController().rootViewController presentViewController:brvc animated:YES completion:nil];
        }
    }];
}

-(void)BRShowWebViewSafariiOS:(NSString *)param
{
    [BRNativeTool NativeRunMainTheardiOS:^{
        [[UIApplication sharedApplication] openURL:[NSURL URLWithString:param] options:@{} completionHandler:nil];
    }];
}

-(void)BRShowWebViewInAppiOS:(NSString *)param
{
    [BRNativeTool NativeRunMainTheardiOS:^{
        UIStoryboard *raider = [UIStoryboard storyboardWithName:@"BRRaider" bundle:nil];
        self.brwvc = [raider instantiateViewControllerWithIdentifier:@"WebVC"];
        self.brwvc.modalPresentationStyle = UIModalPresentationFullScreen;
        [GetAppController().rootViewController presentViewController:self.brwvc animated:YES completion:nil];
        [self.brwvc BRShowWebViewInAppiOS:param];
    }];
}

-(void)BREvaluateJavaScriptiOS:(NSString *)param
{
    [BRNativeTool NativeRunMainTheardiOS:^{
        if (self.brwvc) {
            [self.brwvc BREvaluateJavaScriptiOS:param];
        }
    }];
}

-(void)BRShowNewsVCiOS:(NSString *)param
{
    [BRNativeTool NativeRunMainTheardiOS:^{
        UIStoryboard *raider = [UIStoryboard storyboardWithName:@"BRRaider" bundle:nil];
        BRNewsVC *brnvc = [raider instantiateViewControllerWithIdentifier:@"NewsVC"];
        brnvc.modalPresentationStyle = UIModalPresentationFullScreen;
        [GetAppController().rootViewController presentViewController:brnvc animated:YES completion:nil];
        [brnvc BRSetupUIiOS:param];
        [brnvc BRLoadUIiOS:param];
        [brnvc BRSetSelectiOS:1];
    }];
}

-(void)BRCloseWebViewiOS
{
    [BRNativeTool NativeRunMainTheardiOS:^{
        if (self.brwvc) {
            [self.brwvc BRCloseWebViewiOS];
            self.brwvc = nil;
        }
    }];
}

#pragma mark -- delegate
-(void)safariViewControllerDidFinish:(SFSafariViewController *)controller {
    [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeInAppSafariClosed msg: @""];
}

@end
