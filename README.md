# MyoArmControl
Research of mechanical control via myoelectrical signals using Myo Armband. 
Student work in Technical University of Liberec (TUL).

## Targets
- find optimal EMG signal filtering method (in accuracy and computational cost)
- create effective ML model for gesture recognition
- move this model on remote device (such Arduino board) or  coumuter native program
- exercise control

## Requirements (for now)
Python: 3.6
- tensorflow==1.4.0
- keras==2.1.3

Unity: 2019.2.10f



## Data preprocessing

For better results, the data needs preprocessing. Original EMG signal has fast large differences of values. For its approximation I chose Savitzkiy-Goley filter with 9 points of 
