namespace TableForge.Editor.UI
{
    internal struct FunctionInfo
    {
        public string Name { get; }
        public ArgumentDefinitionCollection ExpectedArguments { get; }
        public string Description { get; }
        public FunctionReturnType ReturnType { get; }

        public FunctionInfo(string name, string description,  FunctionReturnType returnType, ArgumentDefinitionCollection expectedArguments)
        {
            Name = name;
            ExpectedArguments = expectedArguments;
            Description = description;
            ReturnType = returnType;
        }

        public override string ToString()
        {
            return $"={Name}({ExpectedArguments})\n{Description}";
        }
    }
}