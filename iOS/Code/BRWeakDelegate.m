#import "BRWeakDelegate.h"

@implementation BRWeakDelegate

- (void)userContentController:(WKUserContentController *)userContentController didReceiveScriptMessage:(WKScriptMessage *)message {
    [self.weakdelegate userContentController:userContentController didReceiveScriptMessage:message];
}

- (instancetype)initWithDelegate:(id<WKScriptMessageHandler>)delegate {
    if (self = [super init]) {
        self.weakdelegate = delegate;
    }
    return self;
}

@end
