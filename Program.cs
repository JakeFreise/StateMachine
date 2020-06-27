using System;
using StateMachine;
//TEST PROGRAM SHOWING SIMPLE IMPLEMENTATION OF API
namespace Simulation
{
    internal class Program
    {
        static void Main()
        {
            //string location of the System Dynamics Model file
            string xml = "../../Fishmodel.stmx";

            //Model object using location
            Model testModel = new Model(xml);

            //run 30 timesteps and print the number of fish at each time step.
            for (int counter = 0; counter < 30; counter++)
            {
                //Programmers need to know key terms from the model such as "Fish" to access the stock variable 'Fish' in the model.
                //These can be found in the .stmx model file
                Console.WriteLine("Fish = " + testModel.getVariable("Fish"));
                //Console.WriteLine("Difference = " + testModel.getDifference("Fish"));
                testModel.Simulate();
            }
        }
    }
}
