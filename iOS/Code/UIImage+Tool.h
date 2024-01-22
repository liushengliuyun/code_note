//
//  UIImage+Tool.h
//  Unity-iPhone
//
//  Created by UnityDeveloper on 2022/9/13.
//



NS_ASSUME_NONNULL_BEGIN

@interface UIImage (Tool)

- (UIImage *)BRFixOrientationiOS;

- (UIImage *)BRImageAtRectiOS:(CGRect)rect;

- (UIImage *)BRResizeImageSizeiOS:(CGSize)size scale:(BOOL)scale;

@end

NS_ASSUME_NONNULL_END
