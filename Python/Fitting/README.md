# MyoArmControl
Research of mechanical control via myoelectrical signals using Myo Armband. 

Student work in Technical University of Liberec 

## Targets
- find optimal EMG signal filtering method (in accuracy and computational cost)
- create effective ML model for gesture recognition
- move this model on remote device (such Arduino board) or  coumuter native program
- exercise control

## Review
- [Data mining](https://github.com/denisgriaznov/MyoArmControl#data-mining)
- [Data preprocessing](https://github.com/denisgriaznov/MyoArmControl#data-preprocessing)
- [Data analysis](https://github.com/denisgriaznov/MyoArmControl#data-analysis)
- [Create and fitting model](https://github.com/denisgriaznov/MyoArmControl#create-and-fitting-model)



## Requirements 
Python: 3.6
- tensorflow==1.4.0
- keras==2.1.3

Unity: 2019.2.10f
- TensorFlow-Sharp ([Download here](https://s3.amazonaws.com/unity-ml-agents/0.5/TFSharpPlugin.unitypackage))

## Data mining

Unity program was developed on base of the Unity SDK example from MyoArmband developers:

- added the ability to recieve EMG data and vizualize it in graph
- button for writing data to a file with current gesture chose (added as last element of a row)
- integrated neural network model using TensorFlowSharp and real-time gesture recognition with multithreading
- created class with Savitzkiy-Goley filter for processing input data

![Uniy](Images/unity.png)

Write to a file:
- already filtering data
- row is 720 EMG values (90 for each sensor) and last value of gesture code

[MyoArmband SDK](https://support.getmyo.com/hc/en-us/articles/360018409792-Myo-Connect-SDK-and-firmware-downloads)

## Data preprocessing

For better results, the data needs preprocessing. Original EMG signal has fast large differences of values. For its approximation I chose Savitzkiy-Goley filter with 9 points and 2nd polynom order.

[Useful link about such filtering](http://195.134.76.37/applets/AppletSmooth/Appl_Smooth2.html)

#### Comparison filters with different points count:
![Uniy](Images/filtering.png)

## Data analysis

For first example small dataset with 3 different gestures was recorded (about 60 elements for each class)

#### Classes:
- Fist
- Palm
- Relax

Correlation matrix was calculated in 2 variations:
- all features with all (720*720)
- all sensors curves with all(8*8)

![Fitting](Images/correlation.png)

No strong correlation between features so we shouldn't reduce dimension

Correlation effect increase near corners because those sensors are close to each other (it is cycle)

Next, see at distribution of values in each sensor for all classes

For example graphs of distribution of 4 and 8 sensors (other have similar ones)

![Fitting](Images/dist.png)
## Create and fitting model

Recurent neural network model was choised as more effective in time series and human action recognition.
Architecture includes few LSTM layers with dropout to avoid overfitting:

- Input layer 8*90
- LSTM layer with 50 units and 0,2 dropout
- LSTM layer with 50 units and 0,2 dropout
- LSTM layer with 50 units and 0,2 dropout
- LSTM layer with 50 units and 0,2 dropout
- Ordinary layer with 64 units
- Ordinary layer with 128 units
- Output layer with 3 options

[Wiki about LSTM](https://en.wikipedia.org/wiki/Long_short-term_memory)

#### Graphs of loss and accuracy on 25 epochs with 0.25 validation data from our dataset:

![Fitting](Images/fitting.png)

#### Validation loss: 0.1998 
#### Validation accuracy: 0.9070

It is a very good result for such small sample. It show us that we can recognize more gestures with more dataset and increase accuracy.
