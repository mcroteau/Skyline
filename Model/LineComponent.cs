using System;

namespace Tiger.Model {
    
    public class LineComponent {

        bool iterated;
        string activeField;
        string objectField;
        string lineElement;

        public bool isIterated()
        {
            return this.iterated;
        }

        public void setIterated(bool iterated)
        {
            this.iterated = iterated;
        }

        public string getActiveField()
        {
            return this.activeField;
        }

        public void setActiveField(string activeField)
        {
            this.activeField = activeField;
        }

        public string getObjectField()
        {
            return this.objectField;
        }

        public void setObjectField(string objectField)
        {
            this.objectField = objectField;
        }

        public string getLineElement()
        {
            return this.lineElement;
        }

        public void setLineElement(string lineElement)
        {
            this.lineElement = lineElement;
        }

        public string getCompleteLineFunction(){
            return OPEN + this.lineElement + END_FUNCTION;
        }

        public string getCompleteLineElement(){
            return OPEN + this.lineElement.Replace(".", "\\.") + END;
        }

       string OPEN = "\\$\\{";
        string END = "\\}";
        string END_FUNCTION = "\\(\\a-zA-Z+)}";
    }
}