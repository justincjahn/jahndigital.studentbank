using Ganss.Xss;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using Markdig;

namespace JahnDigital.StudentBank.Infrastructure.Markdown;

public class MarkdownToHtmlFormatter : ITextFormatter
{
    public string Format(string input)
    {
        var pipeline = new Markdig.MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var result = Markdig.Markdown.ToHtml(input, pipeline);
        var sanitizer = new HtmlSanitizer();
        return sanitizer.Sanitize(result);
    }
}
