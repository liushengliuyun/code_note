#import <Foundation/Foundation.h>

/// share tool

NS_ASSUME_NONNULL_BEGIN

@interface BRShareTool : NSObject

+(instancetype)shared;

-(void)BRShareToWhatsAppiOS:(NSString *)param;

-(void)BRShareToMessageiOS:(NSString *)param;

-(void)BRShareToDefaultiOS:(NSString *)param;

@end

NS_ASSUME_NONNULL_END
