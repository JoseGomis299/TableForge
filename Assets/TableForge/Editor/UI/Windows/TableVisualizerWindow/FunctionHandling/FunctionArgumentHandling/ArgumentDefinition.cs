namespace TableForge.Editor.UI
{
    internal struct ArgumentDefinition
    {
        public ArgumentType type;
        public string name;
        public bool isOptional;
        public bool indefiniteArguments;

        public ArgumentDefinition(ArgumentType type, string name, bool isOptional = false, bool indefiniteArguments = false)
        {
            this.type = type;
            this.name = name;
            this.isOptional = isOptional;
            this.indefiniteArguments = indefiniteArguments;
        }

        public override string ToString()
        {
            string prefix = isOptional ? "[" : string.Empty; 
            string suffix = isOptional ? "]" : string.Empty;
            suffix = indefiniteArguments ? "; ..." + suffix : suffix;
            
            return $"{prefix}{name}{suffix}";
        }
    }
}