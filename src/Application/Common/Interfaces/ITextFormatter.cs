namespace JahnDigital.StudentBank.Application.Common.Interfaces;

/// <summary>
/// A contract that defines the conversion process between raw text and formatted text, such as Markdown to HTML.
/// </summary>
public interface ITextFormatter
{
    string Format(string input);
}
