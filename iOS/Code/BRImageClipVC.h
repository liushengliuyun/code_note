#import <UIKit/UIKit.h>

NS_ASSUME_NONNULL_BEGIN

@protocol BRImageClipVCDelegate <NSObject>

- (void)BRImageClipVCFinish:(UIImage *)cropImage;

- (void)BRImageClipVCCancel;

@end

@interface BRImageClipVC : UIViewController

@property (nonatomic, assign)id<BRImageClipVCDelegate>delegate;

- (void)BRRefreshUIiOS:(UIImage *)image;

@end

NS_ASSUME_NONNULL_END
