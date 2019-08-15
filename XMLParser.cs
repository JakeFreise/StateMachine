using System.Collections.Generic;


namespace XMLParser
{
  
    public class Variable
    {
        private string name;
        private float _ourValue;
        private string equation;
        private Graphical function = null;
        private float _ourOldValue;
        private bool currentState;
        private bool inProgress;
        private bool _isStock;

        private List<string> _ourChildren;
        private int _ourNumChildren;      

        public Variable(string newName, float newValue, string newEquation, bool isStock)
        {
            name = newName;
            _ourValue = newValue;
            equation = newEquation;
            currentState = false;
            inProgress = false;
            _ourChildren = findChildren();
            _ourNumChildren = _ourChildren.Count;
            _isStock = isStock;
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public float Value
        {
            get
            {
                return _ourValue;
            }
            set
            {
                _ourValue = value;
            }
        }
        
        public string Equation
        {
            get
            {
                return equation;
            }
        }
        
        public Graphical Function
        {
            get
            {
                return function;
            }
            set
            {
                function = value;
            }
        }

        public float OldValue
        {
            get
            {
                return _ourOldValue;
            }
            set
            {
                _ourOldValue = value;
            }
        }

        public bool updated
        {
            get
            {
                return currentState;
            }
            set
            {
                currentState = value;
            }
        }

        public bool inUse
        {
            get
            {
                return inProgress;
            }
            set
            {
                inProgress = value;
            }
        }

        public bool isStock
        {
            get
            {
                return _isStock;
            }
        }

        public List<string> children
        {
            get
            {
                return _ourChildren;
            }
        }

        public int numChildren
        {
            get
            {
                return _ourNumChildren;
            }
        }

        public void SetFunction(Graphical value)
        {
            function = value;
        }

        private List<string> findChildren()
        {
            var children = new List<string>();
            var operators = new List<string> {" ", "+", "-", "*", "/" };

            //split equation into parts
            string[] words = equation.Split(new char[] {' ', '+', '-', '*', '/'}); 

            //if parts are not numbers, operaters, blanks or the parent they are children
            for (int counter = 0; counter < words.Length; counter++)
            {
                //that isnt a math operator and a number or empty
                if (!operators.Contains(words[counter]) && !float.TryParse(words[counter], out float n) && words[counter].Length > 0 && !words[counter].Equals(name))
                {
                    //Console.WriteLine(words[counter]);
                    children.Add(words[counter]);
                }
            }

            return children;
        }
    }

    public class Graphical
    {
        private float[] xBounds = new float [2];
        private float[] yBounds = new float [2];
        private float[] yTable;
        private float[] xTable;
        private float[,] points;
        
        public Graphical(string stringTable, string[] bounds)
        {
            xBounds[0] = float.Parse(bounds[0]);
            xBounds[1] = float.Parse(bounds[1]);
            yBounds[0] = float.Parse(bounds[2]);
            yBounds[1] = float.Parse(bounds[3]);

            yTable = parseStringTable(stringTable);
            xTable = createXTable(xBounds, yTable.Length);
            points = new float[yTable.Length,2];

            for (int counter = 0; counter < yTable.Length; counter++)
            {
                points[counter,0] = xTable[counter];
                points[counter,1] = yTable[counter];
                
                //Console.WriteLine(points[counter, 0] + ", " + points[counter, 1]);
            } 
        }

        private float[] parseStringTable(string inputTable)
        {
            string[] words = inputTable.Split(',');

            float[] outputTable = new float[words.Length];

            for (int counter = 0; counter < words.Length; counter++)
            {
                //Console.WriteLine(words[counter]);
                outputTable[counter] = float.Parse(words[counter]);
            }
            return outputTable;
        }

        private float[] createXTable(float[] bounds, int divisions)
        {
            float difference = bounds[1] - bounds[0];

            float increment = difference / (divisions-1);

            float[] xTable = new float[divisions];

            for (int counter = 0; counter < divisions; counter++)
            {
                xTable[counter] = bounds[0] + increment * (counter);
                //Console.WriteLine(xTable[counter]);
            }

            return xTable;
        }

        public float getOutput(float x)
        {
            //Console.WriteLine(x);
            int target = 0;
            
            //find which line segment our x fits into 
            for (int counter = 0; counter < points.Length; counter++)
            {
                if (x < points[counter, 0])
                {
                    target = counter;
                    break;
                }
                else if (x == points[counter, 0])
                {
                    return points[counter, 1];
                }
            }
            
            float a = (points[target, 1] - points[target - 1, 1]);  //y2-y1
            float b = (points[target, 0] - points[target - 1, 0]); //x2-x1
            float m = a/b; //slope
            float intercept = points[target, 1] - m * points[target, 0]; //b
            
            float output = m*x + intercept; 

            return output;
        }

        public float Xmin
        {
            get
            {
                return xBounds[0];
            }
        }

        public float Xmax
        {
            get
            {
                return xBounds[1];
            }
        }

        public float Ymin
        {
            get
            {
                return yBounds[0];
            }
        }

        public float Ymax
        {
            get
            {
                return yBounds[1];
            }
        }
    }
}
