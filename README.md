# MyoArmControl
Research of mechanical control via myoelectrical signals using Myo Armband. 
Student work in Technical University of Liberec (TUL).

## Targets
- find optimal EMG signal filtering method (in accuracy and computational cost)
- create effective ML model for hand gesture recognition
- port this model on 
## Data preprocessing

For better results, the data needs preprocessing. Original EMG signal has fast large differences of values. For its approximation I chose Savitzkiy-Goley filter with 9 points of 
