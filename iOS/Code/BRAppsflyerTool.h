#import <Foundation/Foundation.h>

/// af tool

NS_ASSUME_NONNULL_BEGIN

@interface BRAppsflyerTool : NSObject

-(void)BRStartAppsFlyeriOS;

+(instancetype)shared;

+(NSString *)BRGetAppsFlyerIdiOS;

+(void)BRSendAppsFlyerEventiOS:(NSString *)json;

+(void)BRSendAppsFlyerValueiOS:(NSString *)json;

@end

NS_ASSUME_NONNULL_END
