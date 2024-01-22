//
//  BREmailTool.h
//  Unity-iPhone
//
//  Created by UnityDeveloper on 2023/1/9.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface BREmailTool : NSObject

+(instancetype)shared;

-(BOOL)BRSendEmailiOS:(NSString *)json;

@end

NS_ASSUME_NONNULL_END
