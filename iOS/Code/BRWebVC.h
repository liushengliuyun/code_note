#import <UIKit/UIKit.h>

/// webview vc

NS_ASSUME_NONNULL_BEGIN

@interface BRWebVC : UIViewController

-(void)BRCloseWebViewiOS;

-(void)BRShowWebViewInAppiOS:(NSString *)param;

-(void)BREvaluateJavaScriptiOS:(NSString *)param;

@end

NS_ASSUME_NONNULL_END
