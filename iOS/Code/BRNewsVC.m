#import <WebKit/WebKit.h>

#import "BRiOSTool.h"
#import "BRNativeTool.h"
#import "BRUnityFounctionConst.h"
#import "BRNewsVC.h"
#import "BRWeakDelegate.h"

@interface BRNewsVC ()<WKUIDelegate, WKNavigationDelegate, WKScriptMessageHandler>

@property (weak, nonatomic) IBOutlet UIView *brnavigation;

@property (weak, nonatomic) IBOutlet UILabel *brnavigationtxt;

@property (weak, nonatomic) IBOutlet UIButton *brback;

@property (weak, nonatomic) IBOutlet UIButton *brbtn1;

@property (weak, nonatomic) IBOutlet UIButton *brbtn2;

@property (weak, nonatomic) IBOutlet UIButton *brbtn3;

@property (weak, nonatomic) IBOutlet UIButton *btselected1;

@property (weak, nonatomic) IBOutlet UIButton *brselected2;

@property (weak, nonatomic) IBOutlet UIButton *brselected3;

@property (weak, nonatomic) IBOutlet UILabel *brtxt1;

@property (weak, nonatomic) IBOutlet UILabel *brtxt2;

@property (weak, nonatomic) IBOutlet UILabel *brtxt3;

@property (weak, nonatomic) IBOutlet UIImageView *brline;

@property (strong, nonatomic)WKWebView *brwkweb1;

@property (strong, nonatomic)WKWebView *brwkweb2;

@property (strong, nonatomic)WKWebView *brwkweb3;

@property(nonatomic, strong)WKPreferences *brpreferences;

@property(nonatomic, strong)WKWebViewConfiguration *brconfig;

@end

@implementation BRNewsVC

- (void)dealloc {
    if (self.brwkweb1) {
        [self.brwkweb1 setUIDelegate:nil];
        [self.brwkweb1 setNavigationDelegate:nil];
    }
    if (self.brwkweb2) {
        [self.brwkweb2 setUIDelegate:nil];
        [self.brwkweb2 setNavigationDelegate:nil];
    }
    if (self.brwkweb3) {
        [self.brwkweb3 setUIDelegate:nil];
        [self.brwkweb3 setNavigationDelegate:nil];
    }
    if (self.brconfig) {
        if (@available(iOS 14.0, *)) {
            [self.brconfig.userContentController removeAllScriptMessageHandlers];
        }
        [self.brconfig.userContentController removeAllUserScripts];
        self.brconfig = nil;
    }
    NSLog(@"dealloc news vc");
}

-(BOOL)prefersStatusBarHidden {
    return  YES;
}

- (void)BRSetupUIiOS:(NSString *)json {
    NSDictionary *dict = [BRNativeTool NativeStringTodictiOS:json];
    self.brnavigationtxt.text = dict[@"top_txt"];
    self.brtxt1.text = dict[@"btn_txt_1"];
    self.brtxt2.text = dict[@"btn_txt_2"];
    self.brtxt3.text = dict[@"btn_txt_3"];
    
    [self.brback addTarget:self action:@selector(BROnClickBackiOS:) forControlEvents:UIControlEventTouchUpInside];
    [self.brbtn1 addTarget:self action:@selector(BROnClickBRTop1iOS:) forControlEvents:UIControlEventTouchUpInside];
    [self.brbtn2 addTarget:self action:@selector(BROnClickBRTop2iOS:) forControlEvents:UIControlEventTouchUpInside];
    [self.brbtn3 addTarget:self action:@selector(BROnClickBRTop3iOS:) forControlEvents:UIControlEventTouchUpInside];

    self.brpreferences = [WKPreferences new];
    self.brpreferences.javaScriptEnabled = YES;
    self.brpreferences.javaScriptCanOpenWindowsAutomatically = YES;
    if (@available(iOS 13.0, *)) {
        self.brpreferences.fraudulentWebsiteWarningEnabled = NO;
    }
    
    self.brconfig = [[WKWebViewConfiguration alloc] init];
    self.brconfig.userContentController = [WKUserContentController new];
    self.brconfig.preferences = self.brpreferences;
    [self.brconfig.userContentController addScriptMessageHandler:[[BRWeakDelegate alloc] initWithDelegate:self] name:@"SendEvents"];
    
    self.brwkweb1 = [[WKWebView alloc] initWithFrame:CGRectZero configuration:self.brconfig];
    self.brwkweb1.navigationDelegate = self;
    self.brwkweb1.UIDelegate = self;
    self.brwkweb1.scrollView.bounces = false;
    [self.brwkweb1 setOpaque:false];
    [self.view addSubview:self.brwkweb1];
    [self BRAddWkConstraint:self.brwkweb1];

    self.brwkweb2 = [[WKWebView alloc] initWithFrame:CGRectZero configuration:self.brconfig];
    self.brwkweb2.navigationDelegate = self;
    self.brwkweb2.UIDelegate = self;
    self.brwkweb2.scrollView.bounces = false;
    [self.brwkweb2 setOpaque:false];
    [self.view addSubview:self.brwkweb2];
    [self BRAddWkConstraint:self.brwkweb2];
    
    self.brwkweb3 = [[WKWebView alloc] initWithFrame:CGRectZero configuration:self.brconfig];
    self.brwkweb3.navigationDelegate = self;
    self.brwkweb3.UIDelegate = self;
    self.brwkweb3.scrollView.bounces = false;
    [self.brwkweb3 setOpaque:false];
    [self.view addSubview:self.brwkweb3];
    [self BRAddWkConstraint:self.brwkweb3];
}

- (void)BRLoadUIiOS:(NSString *)json
{
    NSDictionary *dict = [BRNativeTool NativeStringTodictiOS:json];
    NSURLRequest *request1 = [NSURLRequest requestWithURL: [NSURL URLWithString:dict[@"url1"]]];
    NSURLRequest *request2 = [NSURLRequest requestWithURL: [NSURL URLWithString:dict[@"url2"]]];
    NSURLRequest *request3 = [NSURLRequest requestWithURL: [NSURL URLWithString:dict[@"url3"]]];
    [self.brwkweb1 loadRequest:request1];
    [self.brwkweb2 loadRequest:request2];
    [self.brwkweb3 loadRequest:request3];
}

- (void)BRSetSelectiOS:(NSInteger)index
{
    [self.btselected1 setHidden:index != 1];
    [self.brselected2 setHidden:index != 2];
    [self.brselected3 setHidden:index != 3];
    [self.brwkweb1 setHidden:index != 1];
    [self.brwkweb2 setHidden:index != 2];
    [self.brwkweb3 setHidden:index != 3];
}

#pragma mark -- private
- (void)BRAddWkConstraint:(WKWebView *)web {
    web.translatesAutoresizingMaskIntoConstraints = NO;
    NSLayoutConstraint *top = [NSLayoutConstraint constraintWithItem:web attribute:NSLayoutAttributeTop relatedBy:NSLayoutRelationEqual toItem:self.brline attribute:NSLayoutAttributeBottom multiplier:1 constant:0];
    NSLayoutConstraint *lft = [NSLayoutConstraint constraintWithItem:web attribute:NSLayoutAttributeLeft relatedBy:NSLayoutRelationEqual toItem:self.view attribute:NSLayoutAttributeLeft multiplier:1 constant:0];
    NSLayoutConstraint *rit = [NSLayoutConstraint constraintWithItem:web attribute:NSLayoutAttributeRight relatedBy:NSLayoutRelationEqual toItem:self.view attribute:NSLayoutAttributeRight multiplier:1 constant:0];
    NSLayoutConstraint *btm = [NSLayoutConstraint constraintWithItem:web attribute:NSLayoutAttributeBottom relatedBy:NSLayoutRelationEqual toItem:self.view attribute:NSLayoutAttributeBottom multiplier:1 constant:0];
    [self.view addConstraints:@[top, lft, rit, btm]];
}

- (void)BROnClickBackiOS:(UIButton *)btn
{
    [self dismissViewControllerAnimated:true completion:nil];
}

- (void)BROnClickBRTop1iOS:(UIButton *)btn
{
    [self BRSetSelectiOS:1];
}

- (void)BROnClickBRTop2iOS:(UIButton *)btn
{
    [self BRSetSelectiOS:2];
}

- (void)BROnClickBRTop3iOS:(UIButton *)btn
{
    [self BRSetSelectiOS:3];
}

#pragma mark -- delegate
- (void)userContentController:(WKUserContentController *)userContentController didReceiveScriptMessage:(WKScriptMessage *)message
{
    if ([message.name isEqualToString:@"SendEvents"]) {
        if ([message.body isKindOfClass:[NSString class]]) {
            [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeNewsSendEvents msg:message.body];
        }
    }
    [self dismissViewControllerAnimated:false completion:nil];
}

-(void)webView:(WKWebView *)webView didFinishNavigation:(WKNavigation *)navigation {
    NSString *injectionJSString = @"var script = document.createElement('meta');"
    "script.name = 'viewport';"
    "script.content=\"width=device-width, initial-scale=1.0,maximum-scale=1.0, minimum-scale=1.0, user-scalable=no\";"
    "document.getElementsByTagName('head')[0].appendChild(script);";
    [webView evaluateJavaScript:injectionJSString completionHandler:nil];
}

- (void)webView:(WKWebView *)webView decidePolicyForNavigationAction:(WKNavigationAction *)navigationAction decisionHandler:(void (^)(WKNavigationActionPolicy))decisionHandler {
    if (![navigationAction.request.URL.absoluteString hasPrefix:@"https"] &&
        ![navigationAction.request.URL.absoluteString hasPrefix:@"http"] &&
        ![navigationAction.request.URL.absoluteString hasPrefix:@"about"]) {
        if ([[UIApplication sharedApplication] canOpenURL:navigationAction.request.URL]) {
            [[UIApplication sharedApplication] openURL:navigationAction.request.URL options:@{} completionHandler:nil];
        }
        decisionHandler(WKNavigationActionPolicyCancel);
    } else {
        decisionHandler(WKNavigationActionPolicyAllow);
    }
}

@end
