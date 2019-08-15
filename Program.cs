using System;
using StateMachine;

namespace Simulation
{
    internal class Program
    {
        static void Main()
        {
            string xml = "../../Fishmodel.stmx";

            Model testModel = new Model(xml);

            for (int counter = 0; counter < 30; counter++)
            {
                Console.WriteLine("Fish = " + testModel.getVariable("Fish"));
                //Console.WriteLine("Difference = " + testModel.getDifference("Fish"));
                testModel.Simulate();
            }
        }
    }
}