#import <Foundation/Foundation.h>

/// native use tool

NS_ASSUME_NONNULL_BEGIN

typedef void(^SubTheardBlock) (void);
typedef void(^MainTheardBlock) (void);

@interface BRNativeTool : NSObject

+(UIColor *)NativeColorStriOS:(NSString *)str;

+(NSString *)NativeDictToStriOS:(NSDictionary *)dic;

+(NSDictionary *)NativeStringTodictiOS:(NSString *)str;

+(void)NativeRunSubTheardiOS:(SubTheardBlock)block;

+(void)NativeRunMainTheardiOS:(MainTheardBlock)block;

+(void)NativeSendUnityMessageMethodiOS:(NSString *)method msg:(NSString *)msg;

@end

NS_ASSUME_NONNULL_END
