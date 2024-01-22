#import <Foundation/Foundation.h>

/// thinking tool

NS_ASSUME_NONNULL_BEGIN

@interface BRShuShuTool : NSObject

+(instancetype)shared;

-(NSString *)BRGetDistinctIDiOS;

-(void)BRFlushDatasiOS;

-(void)BRCalibrateTimeiOS:(NSString *)time;

-(void)BRInitThinkiOS:(NSString *)debug;

-(void)BRLoginThinkiOS:(NSString *)userid;

-(void)BRStartAutoEventiOS:(NSString *)time;

-(void)BRSetGameRoundsiOS:(NSString *)value;

-(void)BRThinkUserSetiOS:(NSString *)json;

-(void)BRThinkUserSetOnceiOS:(NSString *)json;

-(void)BRThinkTrackiOS:(NSString *)json;

@end

NS_ASSUME_NONNULL_END
