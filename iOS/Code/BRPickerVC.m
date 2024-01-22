#import "UnityAppController.h"

#import "BRPickerVC.h"
#import "BRUnityFounctionConst.h"
#import "BRiOSTool.h"
#import "BRNativeTool.h"
#import "BRImageClipVC.h"

#import "UIImage+Tool.h"
#import <AVFoundation/AVFoundation.h>

@interface BRPickerVC()<UINavigationControllerDelegate, UIImagePickerControllerDelegate, BRImageClipVCDelegate>

@end

@implementation BRPickerVC

- (BOOL)isCameraAccessGranted {
    AVAuthorizationStatus status = [AVCaptureDevice authorizationStatusForMediaType:AVMediaTypeVideo];
    
    if (status == AVAuthorizationStatusAuthorized) {
        // 用户已授予相机权限
        return YES;
    } else if (status == AVAuthorizationStatusDenied || status == AVAuthorizationStatusRestricted) {
        // 用户已拒绝相机权限或受到限制
        return NO;
    } else if (status == AVAuthorizationStatusNotDetermined) {
        // 尚未询问用户相机权限，可以进行权限请求
        __block BOOL accessGranted = NO;
        dispatch_semaphore_t semaphore = dispatch_semaphore_create(0);
        
        [AVCaptureDevice requestAccessForMediaType:AVMediaTypeVideo completionHandler:^(BOOL granted) {
            accessGranted = granted;
            dispatch_semaphore_signal(semaphore);
        }];
        
        dispatch_semaphore_wait(semaphore, DISPATCH_TIME_FOREVER);
        return accessGranted;
    }
    
    return NO;
}

+(instancetype)shared {
    static BRPickerVC *shareInstance;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        shareInstance = [[BRPickerVC alloc] init];
    });
    return shareInstance;
}

- (void)BRBeginUploadHeadiOS:(NSString *)param {
    NSDictionary *brdict = [BRNativeTool NativeStringTodictiOS:param];
    NSString *camera = brdict[@"choosecamare"];
    NSString *photos = brdict[@"choosephotos"];
    NSString *cancel = brdict[@"cancel"];
    [self BRBeginUploadHeadCamera:camera photos:photos cancel:cancel];
}

- (void)BRBeginUploadHeadCamera:(NSString *)camera photos:(NSString *)photos cancel:(NSString *)cancel{
    UIAlertController *brac = [UIAlertController alertControllerWithTitle:nil message:nil preferredStyle:UIAlertControllerStyleActionSheet];
    UIAlertAction *brbtn2 = [UIAlertAction actionWithTitle:photos style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action) {
        UIImagePickerController *brimgPicker = [[UIImagePickerController alloc] init];
        brimgPicker.modalPresentationStyle = UIModalPresentationFullScreen;
        brimgPicker.sourceType = UIImagePickerControllerSourceTypeSavedPhotosAlbum;
        brimgPicker.delegate = self;
        [GetAppController().rootViewController presentViewController:brimgPicker animated:YES completion:nil];
    }];
    UIAlertAction *brbtn1 = [UIAlertAction actionWithTitle:camera style:UIAlertActionStyleDefault handler:^(UIAlertAction * _Nonnull action) {
        //判断是否有权限,没有权限就返回
            if (![self isCameraAccessGranted]) {
                // 如果相机权限未授予，直接返回
                [BRNativeTool NativeSendUnityMessageMethodiOS:iOSClickCamera msg:@""];
                NSLog(@"相机权限未授予，无法执行操作");
                return;
            }
           
        UIImagePickerController *brimgPicker = [[UIImagePickerController alloc] init];
        brimgPicker.modalPresentationStyle = UIModalPresentationFullScreen;
        brimgPicker.sourceType = UIImagePickerControllerSourceTypeCamera;
        brimgPicker.delegate = self;
        [GetAppController().rootViewController presentViewController:brimgPicker animated:YES completion:nil];
    }];
    UIAlertAction *brbtn3 = [UIAlertAction actionWithTitle:cancel style:UIAlertActionStyleCancel handler:^(UIAlertAction * _Nonnull action) {}];
    [brac addAction:brbtn1];
    [brac addAction:brbtn2];
    [brac addAction:brbtn3];
    [GetAppController().rootViewController presentViewController:brac animated:true completion:nil];
}

-(void)imagePickerController:(UIImagePickerController *)picker didFinishPickingMediaWithInfo:(NSDictionary<UIImagePickerControllerInfoKey,id> *)info {
    UIImage *brimage = info[UIImagePickerControllerOriginalImage];
    BRImageClipVC *vc = [[BRImageClipVC alloc] init];
    vc.delegate = self;
    [picker pushViewController:vc animated:true];
    [vc BRRefreshUIiOS:brimage];
}

-(void)imagePickerControllerDidCancel:(UIImagePickerController *)picker {
    [picker dismissViewControllerAnimated:true completion:nil];
}

- (void)BRImageClipVCFinish:(UIImage *)cropImage {
    UIImage * resize = [cropImage BRResizeImageSizeiOS:CGSizeMake(60, 60) scale: NO];
    NSData *brdataimages = UIImagePNGRepresentation(resize);
    NSString *brimgString = [brdataimages base64EncodedStringWithOptions:0];
    [BRNativeTool NativeSendUnityMessageMethodiOS:CShapePickerImageFinish msg:brimgString];
}

- (void)BRImageClipVCCancel {
    
}

@end
