#import <Foundation/Foundation.h>

/// location tool

NS_ASSUME_NONNULL_BEGIN

@interface BRLocationManager : NSObject

+ (instancetype)shared;

- (NSString *)BRGetLocationAuthorizationiOS;

- (NSString *)BRGetLastLocationiOS;

- (void)BRSetupLoactioniOS:(NSString *)param;

- (void)BRRequestLocationAuthorizationiOS;

- (void)BRGetLocationiOS;

@end

NS_ASSUME_NONNULL_END
