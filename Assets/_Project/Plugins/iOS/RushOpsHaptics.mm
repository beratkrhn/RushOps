// Short haptic impacts for RushOps (issue #300): called from RushOps.Feel.HapticOutput.
#import <UIKit/UIKit.h>

extern "C" void RushOps_HapticImpact(int style)
{
    UIImpactFeedbackStyle impact = style >= 2 ? UIImpactFeedbackStyleHeavy : UIImpactFeedbackStyleMedium;
    UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:impact];
    [generator prepare];
    [generator impactOccurred];
}
