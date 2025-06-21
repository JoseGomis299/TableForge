namespace TableForge.UI
{
    internal struct ArgumentDefinition
    {
        public ArgumentType Type;
        public bool IsOptional;
        public bool IndefiniteArguments;

        public ArgumentDefinition(ArgumentType type, bool isOptional = false, bool indefiniteArguments = false)
        {
            Type = type;
            IsOptional = isOptional;
            IndefiniteArguments = indefiniteArguments;
        }
    }
}