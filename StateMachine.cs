using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using NCalc2;

using XMLParser;


namespace StateMachine
{
        public class Model
        {
            private List<Variable> variableList;

            private Hashtable H1 = new Hashtable();
            private Hashtable H2 = new Hashtable();

            public Model(string xmlFile)
            {
                XDocument xDoc = XDocument.Load(xmlFile);

                XNamespace ns = xDoc.Root.Attributes("xmlns").First().Value;

                variableList = ParseXML(xDoc, ns);

                //fill hashtables with inital values
                foreach (var variable in variableList)
                {
                    H1.Add(variable.Name, variable);
                }

                Simulate(); //prime the model with zeroth values
            }

            public float getDifference(string variable)
            {
                Variable target = (Variable)H1[variable];
                return target.Value - target.OldValue;
            }

            //returns current value
            public float getVariable(string variable)
            {
                return ((Variable)H1[variable]).Value;
            }

            //sets next value
            public int setVariable(string variable, float value)
            {
                if (H1[variable] != null)
                {
                    H1[variable] = value;
                    return 1;
                }

                return 0;
            }

            public void Simulate()
            {
                foreach (var variable in variableList)
                {
                    variable.updated = false;
                    variable.OldValue = variable.Value;
                }

                List<Variable> waitingParents = new List<Variable>();
                do
                {
                    waitingParents = new List<Variable>();
                    foreach (var variable in variableList)
                    {
                        if (variable.updated == false)
                        {
                            List<Variable> withDupes = waitingParents;
                            withDupes.AddRange(updateChildren(variable, H1));
                            waitingParents = withDupes.Distinct().ToList(); //no duplicates
                            //updateVariable(variable, H1);
                        }
                    }
                } while (waitingParents.Count > 0);
            }

            //takes a variable and updates all variables listed as children. 
            public List<Variable> updateChildren(Variable parent, Hashtable table)
            {
                parent.inUse = true;
                if (parent.isStock)
                {
                    updateVariable(parent, table);
                    parent.inUse = false;
                }

                bool readyToUpdate = true;
                List<Variable> waitingParents = new List<Variable>();
                string[] children = parent.children.ToArray();
                for (int index = 0; index < parent.numChildren; index++) //check to see if children are busy
                {
                    if (table[children[index]] != null)
                    {
                        Variable child = (Variable)table[children[index]];

                        if (child.updated == false && child.inUse) //parent who needs to wait for children 
                        {
                            waitingParents.Add(parent);
                            readyToUpdate = false;

                        }
                        else if (child.updated == false)
                        {
                            waitingParents.AddRange(updateChildren(child, table));
                        }
                    }
                }

                if (readyToUpdate && !parent.isStock)
                {
                    updateVariable(parent, table);
                }

                parent.inUse = false;
                return waitingParents;
            }

            //take a variable and a hashtable of current variable results and update the value of that variable
            public void updateVariable(Variable variable, Hashtable table)
            {
                //get
                float value = calculateEquation(variable.Equation, table);

                if (variable.Function == null)
                {
                    variable.Value = value;
                }
                else
                {
                    variable.Value = graphicalOutput(value, variable.Function);
                }
                variable.updated = true;
            }

            //takes equation and hashtable of current variable values returns the result of the equation as the float
            private float calculateEquation(string equation, Hashtable table)
            {
                string explicitEquation = MakeExplicitEquation(equation, table);
                Expression e = new Expression(explicitEquation);
                float value = float.Parse(e.Evaluate().ToString());

                return value;
            }

            private string MakeExplicitEquation(string equation, Hashtable table)
            {
                var operators = new List<string> { "+", "-", "*", "/" };
                equation = addSpacesToString(equation);
                //break equation into pieces
                string[] words = equation.Split(' ');

                //for each piece
                for (int counter = 0; counter < words.Length; counter++)
                {
                    //that isnt a math operator
                    if (!operators.Contains(words[counter]))
                    {
                        //convert it to the value in table
                        Variable target = (Variable)table[words[counter]];
                        if (target != null)
                        {
                            words[counter] = target.Value.ToString();
                        }
                    }
                }

                //remake the string and return it
                string concat = (String.Join(" ", words));
                return concat;
            }

            private string addSpacesToString(string input)
            {
                string output = input;

                string[] words = input.Split('*');

                if (words.Length > 1)
                {
                    output = words[0];

                    for (int counter = 1; counter < words.Length; counter++)
                    {
                        output = output + " * " + words[counter];
                    }
                }

                words = input.Split('/');

                if (words.Length > 1)
                {
                    output = words[0];
                    for (int counter = 1; counter < words.Length; counter++)
                    {
                        output = output + " / " + words[counter];
                    }
                }

                words = input.Split('+');

                if (words.Length > 1)
                {
                    output = words[0];
                    for (int counter = 1; counter < words.Length; counter++)
                    {
                        output = output + " + " + words[counter];
                    }
                }

                words = input.Split('-');

                if (words.Length > 1)
                {
                    output = words[0];
                    for (int counter = 1; counter < words.Length; counter++)
                    {
                        output = output + " - " + words[counter];
                    }
                }
                return output;
            }

            private List<Variable> ParseXML(XDocument xDoc, XNamespace ns)
            {
                var result = xDoc.Root.Descendants(ns + "variables");

                List<Variable> variableList = new List<Variable>();

                foreach (XElement variable in result)
                {
                    //STOCKS
                    var stockList = from q in variable.Descendants(ns + "stock")
                                    select new
                                    {
                                        name = q.FirstAttribute.Value,
                                        initial = q.Element(ns + "eqn").Value,
                                        inflow = q.Elements(ns + "inflow").Select(el => el.Value).ToArray(),
                                        outflow = q.Elements(ns + "outflow").Select(el => el.Value).ToArray()
                                    };

                    foreach (var stock in stockList)
                    {

                        float initialValue = checkInitial(stock.initial);
                        string parsedName = stock.name.Replace(' ', '_');
                        string equation = makeStockEquation(parsedName, stock.inflow, stock.outflow);
                        variableList.Add(new Variable(parsedName, initialValue, equation, true));
                    }

                    //FLOWS
                    var flowList = from q in variable.Descendants(ns + "flow")
                                   select new
                                   {
                                       name = q.FirstAttribute.Value,
                                       equation = q.Element(ns + "eqn").Value,
                                   };
                    foreach (var flow in flowList)
                    {
                        float initialValue = checkInitial(flow.equation);
                        string parsedName = flow.name.Replace(' ', '_');
                        variableList.Add(new Variable(parsedName, 0, flow.equation, false));
                    }

                    //CONVERTERS
                    var auxList = from q in variable.Descendants(ns + "aux")
                                  select new
                                  {
                                      name = q.FirstAttribute.Value,
                                      equation = q.Element(ns + "eqn").Value,
                                      graph = CreateTable(q, ns),
                                  };
                    foreach (var aux in auxList)
                    {
                        float initialValue = checkInitial(aux.equation);
                        string parsedName = aux.name.Replace(' ', '_');
                        Variable converter = new Variable(parsedName, initialValue, aux.equation, false);
                        converter.SetFunction(aux.graph);
                        variableList.Add(converter);
                    }
                }

                return variableList;
            }

            private float checkInitial(string equation)
            {
                float n = 0;
                var isNumeric = float.TryParse(equation, out n);

                if (!isNumeric)
                {
                    n = 0;
                }

                return n;
            }

            private static Graphical CreateTable(XElement element, XNamespace ns)
            {
                var table = from q in element.Descendants(ns + "gf")
                            select new
                            {
                                list = q.Element(ns + "ypts").Value,
                                bounds = GetBounds(q)
                            };

                foreach (var value in table)
                {
                    Graphical newGraph = new Graphical(value.list, value.bounds);
                    return newGraph;
                }
                return null;
            }

            private static string[] GetBounds(XElement element)
            {
                string[] bounds = new string[4];
                var elements = element.Elements();

                var xLower = elements.First().Attribute("min").Value;
                var xUpper = elements.First().Attribute("max").Value;
                var yLower = elements.ElementAt(1).Attribute("min").Value;
                var yUpper = elements.ElementAt(1).Attribute("max").Value;

                bounds[0] = xLower;
                bounds[1] = xUpper;
                bounds[2] = yLower;
                bounds[3] = yUpper;

                return bounds;
            }

            private string makeStockEquation(string name, string[] inflow, string[] outflow)
            {
                string plus = " + ";
                string minus = " - ";
                string equation = name;

                for (int index = 0; index < inflow.Length; index++)
                {
                    equation = equation + plus + inflow[index];
                }

                for (int index = 0; index < outflow.Length; index++)
                {
                    equation = equation + minus + outflow[index];
                }

                return equation;
            }

            private float graphicalOutput(float input, Graphical function)
            {
                float output = input;
                if (input > function.Xmax)
                {
                    output = function.Xmax;
                }
                else if (input < function.Xmin)
                {
                    output = function.Xmin;
                }

                output = function.getOutput(output);
                return output;
            }
        }
}