
#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface BRRiskifiedTool : NSObject

+ (instancetype)shared;

-(void)StartBeacon:(NSString*)account : (NSString*)token : (BOOL)debug;

-(void)UpdateSessionToken:(NSString*)token;

-(void)LogRequest:(NSURL*)url;

@end

NS_ASSUME_NONNULL_END