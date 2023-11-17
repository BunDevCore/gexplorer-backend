using System.Text;

namespace GdanskExplorer;

public static class Extensions
{
    public static Stream AsUtf8Stream(this string s)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(s ?? ""));
    }
}