using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

public static class SqlQueryHelper
{
    private static Dictionary<string, string> queries;

    static SqlQueryHelper()
    {
        string filePath = HttpContext.Current.Server.MapPath("~/DBScripts/DBQuery.xml");
        XDocument doc = XDocument.Load(filePath);
        queries = doc.Descendants("query")
                     .ToDictionary(q => q.Attribute("key").Value, q => q.Value);
    }

    public static string GetQuery(string key)
    {
        return queries.ContainsKey(key) ? queries[key] : null;
    }
}
