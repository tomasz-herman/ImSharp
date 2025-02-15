namespace ImSharpGenerator;

public class CSharpWriter : StringWriter
{
    public int IndentWidth { get; set; } = 4;
    public int CurrentIndent { get; set; } = 0;

    public Block WriteBlock()
    {
        return new Block(this);
    }
    
    public Indent WriteIndent()
    {
        return new Indent(this);
    }
    
    public void WriteIndented(string line)
    {
        Write($"{new string(' ', CurrentIndent)}{line}");
    }

    public void WriteLineIndented(string line)
    {
        WriteLine($"{new string(' ', CurrentIndent)}{line}");
    }

    public readonly ref struct Block : IDisposable
    {
        private readonly CSharpWriter _writer;
        private readonly int _indent;

        public Block(CSharpWriter writer)
        {
            _writer = writer;
            _indent = writer.IndentWidth;
            _writer.WriteLineIndented("{");
            _writer.CurrentIndent += _indent;
        }
        
        public void Dispose()
        {
            _writer.CurrentIndent -= _indent;
            _writer.WriteLineIndented("}");
        }
    }
    
    public readonly ref struct Indent : IDisposable
    {
        private readonly CSharpWriter _writer;
        private readonly int _indent;

        public Indent(CSharpWriter writer)
        {
            _writer = writer;
            _indent = writer.IndentWidth;
            _writer.CurrentIndent += _indent;
        }
        
        public void Dispose()
        {
            _writer.CurrentIndent -= _indent;
        }
    }
}