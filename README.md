# MyoArmControl
Research of mechanical control via myoelectrical signals using Myo Armband. 

Student work in Technical University of Liberec 

## Targets

- find optimal EMG signal filtering method (in accuracy and computational cost)
- create effective ML model for gesture recognition
- move this model on remote device (such Arduino board) or  coumuter native program
- exercise control

## Preview

![Preview](Images/gif.gif | width=120)

## Requirements 
Python: 3.6
- tensorflow==1.4.0
- keras==2.1.3

Unity: 2019.2.10f
- TensorFlow-Sharp ([Download here](https://s3.amazonaws.com/unity-ml-agents/0.5/TFSharpPlugin.unitypackage))

## ML models

Models were created for PC (tested in Unity) and for Arduino (Robot was created)

#### Model for Arduino

![Model for Arduino](Images/schemeheavymodern.png | width=120)

## Fitting results

The optimal result for the microcontroller was obtained with the model taking the last 8 measurements as input.

![Model for Arduino](Images/7gesturesLSTMNew.png)

