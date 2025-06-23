namespace TableForge.Editor.UI
{
    internal struct ArgumentDefinition
    {
        public ArgumentType Type;
        public string Name;
        public bool IsOptional;
        public bool IndefiniteArguments;

        public ArgumentDefinition(ArgumentType type, string name, bool isOptional = false, bool indefiniteArguments = false)
        {
            Type = type;
            Name = name;
            IsOptional = isOptional;
            IndefiniteArguments = indefiniteArguments;
        }

        public override string ToString()
        {
            string prefix = IsOptional ? "[" : string.Empty; 
            string suffix = IsOptional ? "]" : string.Empty;
            suffix = IndefiniteArguments ? "; ..." + suffix : suffix;
            
            return $"{prefix}{Name}{suffix}";
        }
    }
}