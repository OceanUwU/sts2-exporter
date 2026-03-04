namespace STS2Export;

public static class Extensions {
    public static string ReplaceFirst(this string str, string oldValue, string newValue) {
        int pos = str.IndexOf(oldValue);
        if (pos < 0) return str;
        return str[..pos] + newValue + str[(pos + oldValue.Length)..];
    }
}