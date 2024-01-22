#import <Foundation/Foundation.h>
#import <WebKit/WebKit.h>

/// weak delegate

NS_ASSUME_NONNULL_BEGIN

@interface BRWeakDelegate : NSObject<WKScriptMessageHandler>

@property (nonatomic, weak) id<WKScriptMessageHandler> weakdelegate;

- (instancetype)initWithDelegate:(id<WKScriptMessageHandler>)scriptDelegate;

@end

NS_ASSUME_NONNULL_END
