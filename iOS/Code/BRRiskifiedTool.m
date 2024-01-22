#import "BRRiskifiedTool.h"
#import <RiskifiedBeacon/RiskifiedBeacon.h>

@implementation BRRiskifiedTool

+(instancetype)shared {
    static BRRiskifiedTool *shareInstance;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        shareInstance = [[BRRiskifiedTool alloc] init];
    });
    return shareInstance;
}

-(void)StartBeacon:(NSString*)account : (NSString*)token : (BOOL)debug{
    [RiskifiedBeacon startBeacon:account sessionToken:token debugInfo:debug];
}

-(void)UpdateSessionToken:(NSString*)token{
    [RiskifiedBeacon updateSessionToken:token];
}

-(void)LogRequest:(NSURL*)url{
    [RiskifiedBeacon logRequest:url];
}


@end