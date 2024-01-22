#import <UIKit/UIKit.h>

/// news vc

NS_ASSUME_NONNULL_BEGIN

@interface BRNewsVC : UIViewController

- (void)BRLoadUIiOS:(NSString *)json;

- (void)BRSetupUIiOS:(NSString *)json;

- (void)BRSetSelectiOS:(NSInteger)index;

@end

NS_ASSUME_NONNULL_END
