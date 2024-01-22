#import <CoreLocation/CoreLocation.h>

#import "BRLocationManager.h"
#import "BRUnityFounctionConst.h"
#import "BRiOSTool.h"
#import "BRNativeTool.h"

@interface BRLocationManager()<CLLocationManagerDelegate>

@property(nonatomic, strong)CLLocationManager *brlocationmanager;

@property(nonatomic, strong)NSString *braddresscache;

@end

@implementation BRLocationManager

+ (instancetype)shared
{
    static BRLocationManager *manager = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        manager = [[BRLocationManager alloc]init];
    });
    return manager;
}

- (instancetype)init
{
    self = [super init];
    if (self) {
        self.brlocationmanager = [[CLLocationManager alloc] init];
        self.brlocationmanager.delegate = self;
    }
    return self;
}

- (void)BRRequestLocationAuthorizationiOS
{
    [self.brlocationmanager requestAlwaysAuthorization];
}

- (void)BRSetupLoactioniOS:(NSString *)param {
    NSDictionary *dict = [BRNativeTool NativeStringTodictiOS:param];
    if (dict) {
        NSInteger accuracy = [dict[@"accuracy"] intValue];
        CGFloat distance = [dict[@"distance"] floatValue];
        switch (accuracy) {
            case 1:
                self.brlocationmanager.desiredAccuracy = kCLLocationAccuracyBestForNavigation;
                break;
            case 2:
                self.brlocationmanager.desiredAccuracy = kCLLocationAccuracyBest;
                break;
            case 3:
                self.brlocationmanager.desiredAccuracy = kCLLocationAccuracyNearestTenMeters;
                break;
            case 4:
                self.brlocationmanager.desiredAccuracy = kCLLocationAccuracyHundredMeters;
                break;
            case 5:
                self.brlocationmanager.desiredAccuracy = kCLLocationAccuracyKilometer;
                break;
            case 6:
                self.brlocationmanager.desiredAccuracy = kCLLocationAccuracyThreeKilometers;
                break;
            default:
                self.brlocationmanager.desiredAccuracy = kCLLocationAccuracyBest;
                break;
        }
        self.brlocationmanager.distanceFilter = distance;
    }
}
//获取定位
- (void)BRGetLocationiOS {
    if (self.braddresscache) {
        [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeLocationReverseGeocodeSuccess msg:self.braddresscache];
    } else if (self.brlocationmanager.location) {
        [self BRReverseGeocodeLocationiOS:self.brlocationmanager.location];
    } else {
        [self.brlocationmanager requestLocation];
        [self performSelector:@selector(BRLocationTimeoutiOS) withObject:nil afterDelay:20];
    }
    if ([self isMockLocationEnabled]) {
        // 如果相机权限未授予，直接返回
        NSLog(@"虚拟定位开启");
        [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeLocationMock msg:@""];
    }
}

//获取上一次的定位
- (NSString *)BRGetLastLocationiOS {
    if (self.brlocationmanager.location) {
        return [self BRLocationToStr:self.brlocationmanager.location];
    }
    return @"";
}

- (NSString *)BRGetLocationAuthorizationiOS {
    CLAuthorizationStatus status = [CLLocationManager authorizationStatus];
    switch (status) {
        case kCLAuthorizationStatusNotDetermined:
            return @"1";
        case kCLAuthorizationStatusRestricted:
            return @"2";
        case kCLAuthorizationStatusDenied:
            return @"3";
        case kCLAuthorizationStatusAuthorizedAlways:
            return @"4";
        case kCLAuthorizationStatusAuthorizedWhenInUse:
            return @"5";
    }
    return @"0";
}

#pragma mark -- delegate
// ios 14以前
- (void)locationManager:(CLLocationManager *)manager didChangeAuthorizationStatus:(CLAuthorizationStatus)status {
    switch (status) {
        case kCLAuthorizationStatusNotDetermined:
            [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeLocationDidChangeAuthorization msg:@"1"];
            break;
        case kCLAuthorizationStatusRestricted:
            [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeLocationDidChangeAuthorization msg:@"2"];
            break;
        case kCLAuthorizationStatusDenied:
            [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeLocationDidChangeAuthorization msg:@"3"];
            break;
        case kCLAuthorizationStatusAuthorizedAlways:
            [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeLocationDidChangeAuthorization msg:@"4"];
            break;
        case kCLAuthorizationStatusAuthorizedWhenInUse:
            [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeLocationDidChangeAuthorization msg:@"5"];
            break;
    }
}

// ios 14以后
- (void)locationManagerDidChangeAuthorization:(CLLocationManager *)manager {
    if (@available(iOS 14.0, *)) {
        switch (manager.authorizationStatus) {
            case kCLAuthorizationStatusNotDetermined:
                [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeLocationDidChangeAuthorization msg:@"1"];
                break;
            case kCLAuthorizationStatusRestricted:
                [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeLocationDidChangeAuthorization msg:@"2"];
                break;
            case kCLAuthorizationStatusDenied:
                [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeLocationDidChangeAuthorization msg:@"3"];
                break;
            case kCLAuthorizationStatusAuthorizedAlways:
                [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeLocationDidChangeAuthorization msg:@"4"];
                break;
            case kCLAuthorizationStatusAuthorizedWhenInUse:
                [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeLocationDidChangeAuthorization msg:@"5"];
                break;
        }
    }
}

- (void)locationManager:(CLLocationManager *)manager didUpdateLocations:(NSArray<CLLocation *> *)locations {
    [NSObject cancelPreviousPerformRequestsWithTarget:self selector:@selector(locationTimeout) object:nil];
    CLLocation *location = locations.lastObject;
    if (location) {
        [self BRReverseGeocodeLocationiOS:location];
        [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeLocationDidSuccess msg:[self BRLocationToStr:location]];
    } else if (self.brlocationmanager.location) {
        [self BRReverseGeocodeLocationiOS:location];
        [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeLocationDidSuccess msg:[self BRLocationToStr:self.brlocationmanager.location]];
    } else {
        [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeLocationDidFailWithError msg:@"location error"];
    }
}

- (void)locationManager:(CLLocationManager *)manager didFailWithError:(NSError *)error {
    NSString *msg = [NSString stringWithFormat:@"%@", error];
    [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeLocationDidFailWithError msg:msg];
}

#pragma mark -- private
- (void)BRLocationTimeoutiOS {
    [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeLocationTimeOut msg:@""];
}

- (NSString *)BRLocationToStr:(CLLocation *)location {
    NSMutableDictionary *dict = [NSMutableDictionary dictionary];
    dict[@"latitude"] = [NSString stringWithFormat:@"%f", location.coordinate.latitude];
    dict[@"longitude"] = [NSString stringWithFormat:@"%f", location.coordinate.longitude];
    return [BRNativeTool NativeDictToStriOS:dict];
}

- (void)BRReverseGeocodeLocationiOS:(CLLocation *)location {
    CLGeocoder *geo = [[CLGeocoder alloc] init];
    [geo reverseGeocodeLocation:location completionHandler:^(NSArray<CLPlacemark *> * _Nullable placemarks, NSError * _Nullable error) {
        if (error) {
            NSString *msg = [NSString stringWithFormat:@"%@", error];
            [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeLocationReverseGeocodeError msg:msg];
        } else {
            CLPlacemark *brmark = placemarks.firstObject;
            NSMutableDictionary *brdict = [NSMutableDictionary dictionary];
            if (brmark.subAdministrativeArea) {
                brdict[@"subAdministrativeArea"] = brmark.subAdministrativeArea;
            }
            if (brmark.postalCode) {
                brdict[@"postalCode"] = brmark.postalCode;
            }
            if (brmark.ISOcountryCode) {
                brdict[@"ISOcountryCode"] = brmark.ISOcountryCode;
            }
            if (brmark.country) {
                brdict[@"country"] = brmark.country;
            }
            if (brmark.inlandWater) {
                brdict[@"inlandWater"] = brmark.inlandWater;
            }
            if (brmark.ocean) {
                brdict[@"ocean"] = brmark.ocean;
            }
            if (brmark.name) {
                brdict[@"name"] = brmark.name;
            }
            if (brmark.thoroughfare) {
                brdict[@"thoroughfare"] = brmark.thoroughfare;
            }
            if (brmark.subThoroughfare) {
                brdict[@"subThoroughfare"] = brmark.subThoroughfare;
            }
            if (brmark.locality) {
                brdict[@"locality"] = brmark.locality;
            }
            if (brmark.subLocality) {
                brdict[@"subLocality"] = brmark.subLocality;
            }
            if (brmark.administrativeArea) {
                brdict[@"administrativeArea"] = brmark.administrativeArea;
            }
            self.braddresscache = [BRNativeTool NativeDictToStriOS:brdict];
            NSLog(@"location: %@", self.braddresscache);
            [BRNativeTool NativeSendUnityMessageMethodiOS:CShapeLocationReverseGeocodeSuccess msg:self.braddresscache];
        }
    }];
}

- (BOOL)isMockLocationEnabled {
    if (![CLLocationManager locationServicesEnabled]) {
            NSLog(@"定位服务被禁用");
        return NO;
    }
    
    if ([self isRunningOnSimulator]) {
         NSLog(@"在模拟器上运行，假设为虚拟定位");
        return YES;
    }
    
    CLLocationManager *locationManager = [[CLLocationManager alloc] init];
    locationManager.delegate = nil;
    locationManager.desiredAccuracy = kCLLocationAccuracyBest;
    
    if ([CLLocationManager locationServicesEnabled]) {
      NSLog(@"定位服务开启");
        NSDictionary *info = [[NSBundle mainBundle] infoDictionary];
        NSString *bundleIdentifier = info[@"CFBundleIdentifier"];
        
        if ([CLLocationManager locationServicesEnabled] && [CLLocationManager isMonitoringAvailableForClass:[CLCircularRegion class]]) {
            // 尝试创建一个监测区域，如果无法创建，则说明虚拟定位被启用
            CLCircularRegion *region = [[CLCircularRegion alloc] initWithCenter:CLLocationCoordinate2DMake(0, 0) radius:1 identifier:bundleIdentifier];
            [locationManager startMonitoringForRegion:region];
            [locationManager stopMonitoringForRegion:region];
            return NO;
        }
    }
    
    return YES;
}

- (BOOL)isRunningOnSimulator {
#if TARGET_IPHONE_SIMULATOR
    return YES;
#else
    return NO;
#endif
}

@end
