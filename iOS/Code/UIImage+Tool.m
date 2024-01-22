//
//  UIImage+Tool.m
//  Unity-iPhone
//
//  Created by UnityDeveloper on 2022/9/13.
//

#import "UIImage+Tool.h"

@implementation UIImage (Tool)

- (UIImage *)BRImageAtRectiOS:(CGRect)rect {
    UIImage *fixedImage = [self BRFixOrientationiOS];
    CGImageRef imageRef = CGImageCreateWithImageInRect([fixedImage CGImage], rect);
    UIImage* subImage = [UIImage imageWithCGImage: imageRef];
    CGImageRelease(imageRef);
    return subImage;
}

- (UIImage *)BRFixOrientationiOS {
    if (self.imageOrientation == UIImageOrientationUp)
        return self;
    
    CGAffineTransform brtransform = CGAffineTransformIdentity;
    switch (self.imageOrientation) {
        case UIImageOrientationDown:
        case UIImageOrientationDownMirrored:
            brtransform = CGAffineTransformTranslate(brtransform, self.size.width, self.size.height);
            brtransform = CGAffineTransformRotate(brtransform, M_PI);
            break;
        case UIImageOrientationLeft:
        case UIImageOrientationLeftMirrored:
            brtransform = CGAffineTransformTranslate(brtransform, self.size.width, 0);
            brtransform = CGAffineTransformRotate(brtransform, M_PI_2);
            break;
        case UIImageOrientationRight:
        case UIImageOrientationRightMirrored:
            brtransform = CGAffineTransformTranslate(brtransform, 0, self.size.height);
            brtransform = CGAffineTransformRotate(brtransform, -M_PI_2);
            break;
        default:
            break;
    }
    switch (self.imageOrientation) {
        case UIImageOrientationUpMirrored:
        case UIImageOrientationDownMirrored:
            brtransform = CGAffineTransformTranslate(brtransform, self.size.width, 0);
            brtransform = CGAffineTransformScale(brtransform, -1, 1);
            break;
        case UIImageOrientationLeftMirrored:
        case UIImageOrientationRightMirrored:
            brtransform = CGAffineTransformTranslate(brtransform, self.size.height, 0);
            brtransform = CGAffineTransformScale(brtransform, -1, 1);
            break;
        default:
            break;
    }
    CGContextRef ctx = CGBitmapContextCreate(NULL, self.size.width, self.size.height,
                                             CGImageGetBitsPerComponent(self.CGImage), 0,
                                             CGImageGetColorSpace(self.CGImage),
                                             CGImageGetBitmapInfo(self.CGImage));
    CGContextConcatCTM(ctx, brtransform);
    switch (self.imageOrientation) {
        case UIImageOrientationLeft:
        case UIImageOrientationLeftMirrored:
        case UIImageOrientationRight:
        case UIImageOrientationRightMirrored:
            CGContextDrawImage(ctx, CGRectMake(0,0,self.size.height,self.size.width), self.CGImage);
            break;
            
        default:
            CGContextDrawImage(ctx, CGRectMake(0,0,self.size.width,self.size.height), self.CGImage);
            break;
    }
    CGImageRef cgimg = CGBitmapContextCreateImage(ctx);
    UIImage *img = [UIImage imageWithCGImage:cgimg];
    CGContextRelease(ctx);
    CGImageRelease(cgimg);
    return img;
}

- (UIImage *)BRResizeImageSizeiOS:(CGSize)size scale:(BOOL)scale
{
    UIGraphicsBeginImageContextWithOptions(size, NO, 0.0);
    CGRect rect = CGRectMake(0,0,size.width,size.height);
    if (!scale) {
        CGFloat imageWH = self.size.width/self.size.height;
        CGFloat SizeWH  = size.width/size.height;
        if (imageWH > SizeWH) {
            CGFloat SizeHimageH = size.height/self.size.height;
            CGFloat height = self.size.height*SizeHimageH;
            CGFloat width = height * imageWH;
            CGFloat x = -(width - size.width)/2;
            CGFloat y = 0;
            rect = CGRectMake(x,y,width,height);
        } else {
            CGFloat SizeWimageW = size.width/self.size.width;
            CGFloat width = self.size.width *SizeWimageW;
            CGFloat height = width / imageWH;
            CGFloat x = 0;
            CGFloat y = -(height - size.height)/2;
            rect = CGRectMake(x,y,width,height);
        }
    }
    [[UIColor clearColor] set];
    UIRectFill(rect);
    [self drawInRect:rect];
    UIImage *image = UIGraphicsGetImageFromCurrentImageContext();
    UIGraphicsEndImageContext();
    return image;
}

@end
