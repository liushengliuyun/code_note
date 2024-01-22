#import <WebKit/WebKit.h>

#import "BRiOSTool.h"
#import "BRNativeTool.h"
#import "BRUnityFounctionConst.h"
#import "BRWebVC.h"

@interface BRWebVC ()<WKUIDelegate, WKNavigationDelegate, WKScriptMessageHandler>

@property (weak, nonatomic) IBOutlet UIProgressView *brprogress;

@property (weak, nonatomic) IBOutlet UIView *brnavigation;

@property (weak, nonatomic) IBOutlet UILabel *brnavigationtxt;

@property (weak, nonatomic) IBOutlet UIButton *brback;

@property(nonatomic, strong)WKWebView *brwk;

@property(nonatomic, strong)WKPreferences *brpreferences;

@property(nonatomic, strong)WKWebViewConfiguration *brconfig;

@property(nonatomic, strong)NSString *brtips;

@property(nonatomic, strong)NSString *brok;

@end

@implementation BRWebVC

- (void)dealloc {
    if (self.brwk) {
        [self.brwk setUIDelegate:nil];
        [self.brwk setNavigationDelegate:nil];
    }
    if (self.brconfig) {
        [self.brconfig.userContentController removeAllUserScripts];
    }
}

-(BOOL)prefersStatusBarHidden {
    return  YES;
}

-(void)viewWillAppear:(BOOL)animated
{
    [super viewWillAppear:animated];
    if (self.brwk) {
        [self.brwk addObserver:self forKeyPath:NSStringFromSelector(@selector(estimatedProgress)) options:NSKeyValueObservingOptionNew context:nil];
        [self.brwk addObserver:self forKeyPath:@"URL" options:NSKeyValueObservingOptionNew context:nil];
        [self.brwk addObserver:self forKeyPath:@"loading" options:NSKeyValueObservingOptionNew context:nil];
    }
}

- (void)viewWillDisappear:(BOOL)animated
{
    [super viewWillDisappear:animated];
    if (self.brwk) {
        [self.brwk removeObserver:self forKeyPath:NSStringFromSelector(@selector(estimatedProgress))];
        [self.brwk removeObserver:self forKeyPath:@"URL"];
        [self.brwk removeObserver:self forKeyPath:@"loading"];
    }
}

-(void)BRCloseWebViewiOS
{
    [self dismissViewControllerAnimated:YES completion:nil];
}

-(void)BRShowWebViewInAppiOS:(NSString *)param
{
    NSDictionary *dict = [BRNativeTool NativeStringTodictiOS:param];
    NSURLRequest *request = [NSURLRequest requestWithURL: [NSURL URLWithString:dict[@"url"]]];
    [self BRSetupUIiOS:dict];
    [self.brwk loadRequest:request];
}

-(void)BREvaluateJavaScriptiOS:(NSString *)param
{
    [self.brwk evaluateJavaScript:param completionHandler:^(id _Nullable response, NSError * _Nullable error) {
        if (!error)
        {
            NSLog(@"evaluate succ = %@", response);
        }
        else
        {
            NSLog(@"evaluate fail = %@", error.localizedDescription);
        }
    }];
}

#pragma mark -- delegate
- (void)observeValueForKeyPath:(NSString *)keyPath ofObject:(id)object change:(NSDictionary<NSKeyValueChangeKey,id> *)change context:(void *)context {
    if (object == self.brwk && [keyPath isEqualToString:NSStringFromSelector(@selector(estimatedProgress))]) {
        [self.brprogress setAlpha:1.0f];
        [self.brprogress setProgress:self.brwk.estimatedProgress animated:YES];
        if (self.brwk.estimatedProgress >= 1.0f) {
            [self.brprogress setProgress:self.brwk.estimatedProgress animated:NO];
            [UIView animateWithDuration:0.3 delay:0.6 options:UIViewAnimationOptionCurveEaseOut animations:^{
                [self.brprogress setAlpha:0.0f];
            } completion:^(BOOL finished) {
                [self.brprogress setProgress:0.0f animated:NO];
            }];
        }
    } else if (object == self.brwk && [keyPath isEqualToString: @"URL"]) {
        [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeWKUrlDidChanged msg: self.brwk.URL.absoluteString];
    } else if (object == self.brwk && [keyPath isEqualToString: @"loading"]) {
        NSMutableDictionary *brdict = [NSMutableDictionary dictionary];
        brdict[@"loading"] = self.brwk.isLoading ? @"1" : @"0";
        brdict[@"url"] = self.brwk.URL.absoluteString;
        [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeWKLoadDidChanged msg: [BRNativeTool NativeDictToStriOS:brdict]];
    } else {
        [super observeValueForKeyPath:keyPath ofObject:object change:change context:context];
    }
}

- (void)userContentController:(WKUserContentController *)userContentController didReceiveScriptMessage:(WKScriptMessage *)message
{
    NSLog(@"js message: %@~~%@", message.name, message.body);
}

-(void)webView:(WKWebView *)webView didFinishNavigation:(WKNavigation *)navigation {
    NSString *injectionJSString = @"var script = document.createElement('meta');"
    "script.name = 'viewport';"
    "script.content=\"width=device-width, initial-scale=1.0,maximum-scale=1.0, minimum-scale=1.0, user-scalable=no\";"
    "document.getElementsByTagName('head')[0].appendChild(script);";
    [webView evaluateJavaScript:injectionJSString completionHandler:nil];
}

- (void)webView:(WKWebView *)webView decidePolicyForNavigationAction:(WKNavigationAction *)navigationAction decisionHandler:(void (^)(WKNavigationActionPolicy))decisionHandler {
    [BRNativeTool NativeRunMainTheardiOS:^{
        if (![navigationAction.request.URL.absoluteString hasPrefix:@"https"] &&
            ![navigationAction.request.URL.absoluteString hasPrefix:@"http"] &&
            ![navigationAction.request.URL.absoluteString hasPrefix:@"about"]) {
            if ([[UIApplication sharedApplication] canOpenURL:navigationAction.request.URL]) {
                [[UIApplication sharedApplication] openURL:navigationAction.request.URL options:@{} completionHandler:nil];
            } else {
                UIAlertController *brac = [UIAlertController alertControllerWithTitle:nil message:self.brtips preferredStyle:UIAlertControllerStyleAlert];
                UIAlertAction *braction = [UIAlertAction actionWithTitle:self.brok style:UIAlertActionStyleCancel handler:^(UIAlertAction * _Nonnull action) {
                    [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeWKOpenAppFailed msg: navigationAction.request.URL.absoluteString];
                }];
                [brac addAction:braction];
                [self presentViewController:brac animated:YES completion:nil];
            }
            decisionHandler(WKNavigationActionPolicyCancel);
        } else {
            decisionHandler(WKNavigationActionPolicyAllow);
        }
    }];
}

#pragma mark -- private
- (void)BRSetupUIiOS:(NSDictionary *)dict {
    // ui
    self.brpreferences = [WKPreferences new];
    self.brpreferences.javaScriptEnabled = YES;
    self.brpreferences.javaScriptCanOpenWindowsAutomatically = YES;
    if (@available(iOS 13.0, *)) {
        self.brpreferences.fraudulentWebsiteWarningEnabled = NO;
    }
    
    self.brconfig = [[WKWebViewConfiguration alloc] init];
    self.brconfig.userContentController = [WKUserContentController new];
    self.brconfig.preferences = self.brpreferences;
    
    self.brwk = [[WKWebView alloc] initWithFrame:CGRectZero configuration:self.brconfig];
    self.brwk.navigationDelegate = self;
    self.brwk.UIDelegate = self;
    self.brwk.scrollView.bounces = false;
    [self.brwk setOpaque:false];
    [self.view addSubview:self.brwk];
    [self BRAddWkConstraint:self.brwk];
    
    [self.brback addTarget:self action:@selector(BRBackButtonClickiOS:) forControlEvents:UIControlEventTouchUpInside];
    
    // data
    self.view.backgroundColor = UIColor.whiteColor;
    if (dict) {
        NSString *navi_color = dict[@"topBarColor"];
        self.brnavigation.backgroundColor = [BRNativeTool NativeColorStriOS: navi_color];

        self.brnavigationtxt.text = dict[@"title"];
        self.brnavigationtxt.font = [UIFont systemFontOfSize: [dict[@"titleSize"] integerValue]];

        self.brprogress.tintColor = [BRNativeTool NativeColorStriOS: dict[@"progressColor"]];
        self.brprogress.trackTintColor = [BRNativeTool NativeColorStriOS: dict[@"progressBgColor"]];

        self.brtips = dict[@"tips"];
        self.brok = dict[@"ok"];
    }
}

- (void)BRBackButtonClickiOS:(UIButton *)button
{
    [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeWKUrlDidClosed msg: self.brwk.URL.absoluteString];
    [self BRCloseWebViewiOS];
}

- (void)BRAddWkConstraint:(WKWebView *)web {
    web.translatesAutoresizingMaskIntoConstraints = NO;
    NSLayoutConstraint *top = [NSLayoutConstraint constraintWithItem:web attribute:NSLayoutAttributeTop relatedBy:NSLayoutRelationEqual toItem:self.brprogress attribute:NSLayoutAttributeBottom multiplier:1 constant:0];
    NSLayoutConstraint *lft = [NSLayoutConstraint constraintWithItem:web attribute:NSLayoutAttributeLeft relatedBy:NSLayoutRelationEqual toItem:self.view attribute:NSLayoutAttributeLeft multiplier:1 constant:0];
    NSLayoutConstraint *rit = [NSLayoutConstraint constraintWithItem:web attribute:NSLayoutAttributeRight relatedBy:NSLayoutRelationEqual toItem:self.view attribute:NSLayoutAttributeRight multiplier:1 constant:0];
    NSLayoutConstraint *btm = [NSLayoutConstraint constraintWithItem:web attribute:NSLayoutAttributeBottom relatedBy:NSLayoutRelationEqual toItem:self.view attribute:NSLayoutAttributeBottom multiplier:1 constant:0];
    [self.view addConstraints:@[top, lft, rit, btm]];
}

@end
