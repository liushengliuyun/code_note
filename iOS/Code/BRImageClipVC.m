#import "BRImageClipVC.h"
#import "UIImage+Tool.h"

@interface CropAreaView : UIView

@property(nonatomic, assign)CGRect Rect;

- (void)BRDrawBorderLayerPathiOS;

- (CGRect)BRGetInerFrame;

@end

@implementation CropAreaView

- (void)BRDrawBorderLayerPathiOS {
    CGFloat top = (self.bounds.size.height - self.bounds.size.width - 8) / 2;
    self.Rect = CGRectMake(8, top, self.bounds.size.width - 16, self.bounds.size.width - 16);
    UIBezierPath *path = [UIBezierPath bezierPathWithRoundedRect:self.bounds cornerRadius:0];
    UIBezierPath *inerPath = [UIBezierPath bezierPathWithRoundedRect:self.Rect cornerRadius:0];
    [path appendPath:inerPath];
    [path setUsesEvenOddFillRule:YES];
    
    CAShapeLayer *fillLayer = [CAShapeLayer layer];
    fillLayer.path = path.CGPath;
    fillLayer.fillRule = kCAFillRuleEvenOdd;
    fillLayer.fillColor = [UIColor blackColor].CGColor;
    fillLayer.opacity = 0.5;
    [self.layer addSublayer:fillLayer];
}

- (CGRect)BRGetInerFrame {
    return self.Rect;
}

@end

@interface BRImageClipVC ()

@property (strong, nonatomic) UIImageView *imageView;
@property (strong, nonatomic) CropAreaView *cropAreaView;

@property (nonatomic, strong) UIButton  *btnOK;
@property (nonatomic, strong) UIButton  *btnCancel;

@end

@implementation BRImageClipVC

- (void)BRRefreshUIiOS:(UIImage *)image {
    _imageView = [[UIImageView alloc]initWithFrame: self.view.bounds];
    _imageView.contentMode = UIViewContentModeScaleAspectFit;
    _imageView.image = image;
    [self.view addSubview: _imageView];
    
    _cropAreaView = [[CropAreaView alloc] initWithFrame:_imageView.bounds];
    [_imageView addSubview: _cropAreaView];
    [_cropAreaView BRDrawBorderLayerPathiOS];
    
    _btnOK = [UIButton buttonWithType:UIButtonTypeCustom];
    _btnOK.backgroundColor = [UIColor whiteColor];
    _btnOK.titleLabel.font = [UIFont systemFontOfSize:20];
    _btnOK.frame = CGRectMake(15, _imageView.bounds.size.height - 60, 120, 40);
    [_btnOK.layer setMasksToBounds:YES];
    [_btnOK.layer setCornerRadius:6.0];
    [_btnOK setTitle:@"OK" forState:UIControlStateNormal];
    [_btnOK setTitleColor:[UIColor blackColor] forState:UIControlStateNormal];
    [_btnOK addTarget:self action:@selector(BRClickOkiOS:) forControlEvents:UIControlEventTouchUpInside];
    [self.view addSubview:_btnOK];
    
    _btnCancel = [UIButton buttonWithType:UIButtonTypeCustom];
    _btnCancel.backgroundColor = [UIColor whiteColor];
    _btnCancel.titleLabel.font = [UIFont systemFontOfSize:20];
    _btnCancel.frame = CGRectMake(_imageView.bounds.size.width - 120 - 15, _imageView.bounds.size.height - 60, 120, 40);
    [_btnCancel.layer setMasksToBounds:YES];
    [_btnCancel.layer setCornerRadius:6.0];
    [_btnCancel setTitle:@"Cancel" forState:UIControlStateNormal];
    [_btnCancel setTitleColor:[UIColor blackColor] forState:UIControlStateNormal];
    [_btnCancel addTarget:self action:@selector(BRClickCanceliOS:) forControlEvents:UIControlEventTouchUpInside];
    [self.view addSubview:_btnCancel];
}

#pragma mark - btn click
- (void)BRClickOkiOS:(UIButton *)btn {
    if (self.delegate && [self.delegate respondsToSelector:@selector(BRImageClipVCFinish:)]) {
        [self.delegate BRImageClipVCFinish:[self BRGetClipImage]];
    }
    [self.presentingViewController dismissViewControllerAnimated:true completion:nil];
}
- (void)BRClickCanceliOS:(UIButton *)btn {
    if (self.delegate && [self.delegate respondsToSelector:@selector(BRImageClipVCCancel)]) {
        [self.delegate BRImageClipVCCancel];
    }
    [self.presentingViewController dismissViewControllerAnimated:true completion:nil];
}

- (UIImage *)BRGetClipImage {
    CGRect iner = [_cropAreaView BRGetInerFrame];
    CGFloat factorw = _imageView.bounds.size.width / _imageView.image.size.width;
    CGFloat factorh = _imageView.bounds.size.height / _imageView.image.size.height;
    CGRect Rect = CGRectMake(iner.origin.x / factorw, iner.origin.y / factorh, iner.size.width / factorw, iner.size.height / factorh);
    return [_imageView.image BRImageAtRectiOS:Rect];
}

@end
