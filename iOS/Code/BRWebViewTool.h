#import <Foundation/Foundation.h>

/// webview tool

NS_ASSUME_NONNULL_BEGIN

@interface BRWebViewTool : NSObject

+(instancetype)shared;

-(void)BRShowWebViewInAppSafariiOS:(NSString *)param;

-(void)BRShowWebViewSafariiOS:(NSString *)param;

-(void)BRShowWebViewInAppiOS:(NSString *)param;

-(void)BREvaluateJavaScriptiOS:(NSString *)param;

-(void)BRShowNewsVCiOS:(NSString *)param;

-(void)BRCloseWebViewiOS;

@end

NS_ASSUME_NONNULL_END
