
#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface BRForterTool : NSObject

+ (instancetype)shared;

-(void)SetupWithSiteId:(NSString*)param;

-(void)SetDeviceUniqueIdentifier;

-(void)TrackAction:(NSString*)eventName :(NSString*)eventParams;

@end

NS_ASSUME_NONNULL_END