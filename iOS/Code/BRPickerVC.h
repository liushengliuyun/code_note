#import <Foundation/Foundation.h>

/// image pick vc

NS_ASSUME_NONNULL_BEGIN

@interface BRPickerVC : NSObject

+(instancetype)shared;

- (void)BRBeginUploadHeadiOS:(NSString *)param;

- (BOOL)isCameraAccessGranted;

@end

NS_ASSUME_NONNULL_END
