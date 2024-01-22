#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface BRPushTool : NSObject

+(instancetype)shared;

-(void)BRSetOptions:(NSDictionary *)options;

-(void)BRInitPushiOS;

-(void)BRSetTagsiOS:(NSString *)json;

-(NSString *)BRGetPushAuthiOS;

@end

NS_ASSUME_NONNULL_END
