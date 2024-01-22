//
//  BRKycTool.h
//  Unity-iPhone
//
//  Created by UnityDeveloper on 2023/3/23.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface BRKycTool : NSObject

+(instancetype)shared;

-(void)BRInitKyciOS:(NSString *)json;

-(void)BRUpdateKycTokeniOS:(NSString *)token;

@end

NS_ASSUME_NONNULL_END
