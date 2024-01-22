#import "BRNativeTool.h"

@implementation BRNativeTool

+ (UIColor *)NativeColorStriOS:(NSString *)stc {
    if (stc.length == 0) {
        return [UIColor blackColor];
    }
    if ([stc hasPrefix:@"#"]) {
        stc = [stc substringFromIndex:1];
    }
    if ([stc containsString:@"0x"]) {
        stc = [stc stringByReplacingOccurrencesOfString:@"0x" withString:@""];
    }
    if ([stc containsString:@"0X"]) {
        stc = [stc stringByReplacingOccurrencesOfString:@"0X" withString:@""];
    }
    if (stc.length == 3) {
        stc = [NSString stringWithFormat:@"%c%c%c%c%c%c",
               [stc characterAtIndex:0],
               [stc characterAtIndex:0],
               [stc characterAtIndex:1],
               [stc characterAtIndex:1],
               [stc characterAtIndex:2],
               [stc characterAtIndex:2]];
    }
    if (stc.length == 4) {
        stc = [NSString stringWithFormat:@"%c%c%c%c%c%c%c%c",
               [stc characterAtIndex:0],
               [stc characterAtIndex:0],
               [stc characterAtIndex:1],
               [stc characterAtIndex:1],
               [stc characterAtIndex:2],
               [stc characterAtIndex:2],
               [stc characterAtIndex:3],
               [stc characterAtIndex:3]];
    }
    if (stc.length == 8) {
        NSScanner *scanner = [NSScanner scannerWithString:stc];
        unsigned hexNum;
        if(![scanner scanHexInt:&hexNum]) {
            return nil;
        }
        return [self NativeColorRGBAHexiOS:hexNum];
    } else if (stc.length == 6) {
        NSScanner *scanner = [NSScanner scannerWithString:stc];
        unsigned hexNum;
        if(![scanner scanHexInt:&hexNum]) {
            return nil;
        }
        return [self NativeColorRGBHexiOS:hexNum];
    }else{
        return nil;
    }
}

+(NSString *)NativeDictToStriOS:(NSDictionary *)dic {
    NSError *err;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:dic options:NSJSONWritingPrettyPrinted error:&err];
    if (err) {
        NSLog(@"dict to str: %@", err.description);
    }
    return [[NSString alloc] initWithData:jsonData encoding: NSUTF8StringEncoding];
}

+(NSDictionary *)NativeStringTodictiOS:(NSString *)str {
    NSError *err;
    NSData *jsonData = [str dataUsingEncoding:NSUTF8StringEncoding];
    NSDictionary *dict = [NSJSONSerialization JSONObjectWithData:jsonData options:NSJSONReadingMutableContainers error:&err];
    if (err) {
        NSLog(@"str to dict: %@", err.description);
        return nil;
    }
    return dict;
}

+(void)NativeRunMainTheardiOS:(MainTheardBlock)block {
    dispatch_async(dispatch_get_main_queue(), ^{
        block();
    });
}

+(void)NativeRunSubTheardiOS:(SubTheardBlock)block {
    dispatch_async(dispatch_get_global_queue(0, 0), ^{
        block();
    });
}

+(void)NativeSendUnityMessageMethodiOS:(NSString *)method msg:(NSString *)msg {
    NSString *text = msg;
    if (!text || ![msg isKindOfClass:[NSString class]]) {
        text = @"";
    }
    const char *str = [text UTF8String];
    char* res = (char*)malloc(strlen(str)+1);
    strcpy(res, str);
    [self NativeRunMainTheardiOS:^{
        UnitySendMessage([@"YZController" UTF8String], [method UTF8String], res);
    }];
}

#pragma mark -- private
+ (UIColor *)NativeColorRGBHexiOS:(UInt32)hex {
    int r = (hex >> 16) & 0xFF;
    int g = (hex >> 8) & 0xFF;
    int b = (hex) & 0xFF;
    return [UIColor colorWithRed:r /255.0f green:g /255.0f blue:b /255.0f alpha:1.0f];
}

+ (UIColor *)NativeColorRGBAHexiOS:(UInt32)hex {
    int r = (hex >> 24) & 0xFF;
    int g = (hex >> 16) & 0xFF;
    int b = (hex>> 8) & 0xFF;
    int alpha = hex & 0xFF;
    return [UIColor colorWithRed:r /255.0f green:g /255.0f blue:b /255.0f alpha:alpha/255.0f];
}

@end
