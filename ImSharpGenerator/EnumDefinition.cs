using System.Collections;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ImSharpGenerator;

public class EnumDefinition : ISourceGenerator, IEnumerable<EnumDefinition.EnumValue>
{
    public bool Flags { get; }
    public string Name { get; }
    public string FriendlyName { get; }
    public SortedSet<EnumValue> Values { get; } = [];

    public EnumDefinition(JProperty enumJson)
    {
        Name = enumJson.Name;
        Flags = Name.Contains("Flags");
        FriendlyName = Name.Replace("Flags", "").Replace("_", "");
        foreach (var field in enumJson.Values())
        {
            var name = field["name"]?.ToString();
            var value = field["value"]?.ToString();
            var calcValue = field["calc_value"]?.ToString();
            if (name != null && value != null && calcValue != null)
            {
                Values.Add(new EnumValue(Name, name, value, calcValue));
            }
        }

        foreach (var value in this)
        {
            value.InitFriendlyValue(this);
        }
    }

    public string GenerateSource()
    {
        using var writer = new CSharpWriter();

        writer.WriteLineIndented("namespace ImSharp");
        using (writer.WriteBlock())
        {
            if (Flags)
            {
                writer.WriteLineIndented("[Flags]");
            }
            
            writer.WriteLineIndented($"public enum {FriendlyName}");
            using (writer.WriteBlock())
            {
                foreach (var value in Values)
                {
                    writer.WriteLineIndented($"{value},");
                }
            }
        }

        return writer.ToString();
    }

    public IEnumerator<EnumValue> GetEnumerator()
    {
        return Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public class EnumValue : IComparable<EnumValue>
    {
        public string Name { get; }
        public string FriendlyName { get; }
        public string Value { get; }
        public int CalculatedValue { get; }
        public string? FriendlyValue { get; private set; }

        public EnumValue(string enumName, string name, string value, string calculatedValue)
        {
            Name = name;
            Value = value;
            CalculatedValue = int.Parse(calculatedValue);
            FriendlyName = Name.Replace($"{enumName}_", "")
                .Replace(enumName, "");
            if (char.IsDigit(FriendlyName[0])) FriendlyName = $"_{FriendlyName}";
        }

        public void InitFriendlyValue(EnumDefinition definition)
        {
            StringBuilder builder = new StringBuilder();
            if (!definition.Flags) return;
            if (CalculatedValue == 0) return;
            if ((CalculatedValue & (CalculatedValue - 1)) == 0)
            {
                int power = (int) Math.Log(CalculatedValue, 2);
                FriendlyValue = $"1 << {power}";
            }
            else
            {
                int rem = CalculatedValue;
                foreach (var value in definition.Reverse())
                {
                    if (rem == 0) break;

                    if (value.CalculatedValue != CalculatedValue && value.CalculatedValue <= rem)
                    {
                        rem -= value.CalculatedValue;
                        builder.Append(builder.Length == 0 ? value.FriendlyName : $" | {value.FriendlyName}");
                    }
                }

                if (rem == 0)
                {
                    FriendlyValue = builder.ToString();
                }
            }
        }

        public int CompareTo(EnumValue other)
        {
            return CalculatedValue.CompareTo(other.CalculatedValue);
        }

        public override bool Equals(object? obj)
        {
            if (obj is EnumValue otherEnumValue)
            {
                return StringComparer.InvariantCulture.Compare(otherEnumValue.Name, Name) == 0;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return $"{FriendlyName} = {FriendlyValue ?? CalculatedValue.ToString()}";
        }
    }
}