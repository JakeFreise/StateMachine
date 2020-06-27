# StateMachine

This is an API that allows System Dynamics Models to be accessed by C#. It simulates a provided model within C# and provides functionality to allow a programmer to push and pull data to/from the model during execution. 

To use in unity simple download the plugins folder and import it into your project. 

--More details in the PDF--

Example of implementation using Craig Reynolds Boid behaviors to control fish AI.
In this instance, green fish chase blue fish.

AI is controlled by C# program, population is controlled by the model.

https://www.youtube.com/watch?v=H8fWJYYAyzc

Fish AI can be found in "Scripts" folder

Model provided to the simulator is located at Scripts/Fishmodel.stmx

![Model](https://i.imgur.com/TC56pBG.png)
-5.1 in PDF


This graphic shows the relationships that drive fish population dynamics. In this instance the fish capacity variable is hardcoded for simplicity as this is supposed to be a simple demonstration. In practice this variable could be controled be a multitude of factors such as food availibility. 

