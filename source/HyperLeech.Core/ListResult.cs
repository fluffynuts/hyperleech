using System.Collections.Generic;
using HtmlAgilityPack;
using PeanutButter.Utils;

namespace HyperLeech
{
    public interface IListResultItem
    {
        string Name { get; }
        string Url { get; }   
    }

    public interface IListResult
    {
        IListResultItem[] Folders { get; }
        IListResultItem[] Files { get; }
    }

    public class ListResultItem: IListResultItem
    {
        public string Name { get; }
        public string Url { get; }
        public ListResultItem(string name, string url)
        {
            Name = name;
            Url = url;
        }
    }

    public class ListResult : IListResult
    {
        public IListResultItem[] Folders { get; }
        public IListResultItem[] Files { get; }
        public ListResult(string url, string html)
        {
            if (!url.EndsWith("/"))
                url += "/";
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var folders = new List<IListResultItem>();
            var files = new List<IListResultItem>();
            doc.DocumentNode.SelectNodes("//a[@href]").ForEach(node =>
            {
                var href = node.GetAttributeValue("href", "");
                if (href == "../" || href == "./")
                    return;
                var itemUrl = url + href;
                if (itemUrl.EndsWith("/"))
                    itemUrl = itemUrl.Substring(0, itemUrl.Length - 1);
                var item = new ListResultItem(node.InnerText, itemUrl);
                var addTo = href.EndsWith("/") ? folders: files;
                addTo.Add(item);
            });
            Folders = folders.ToArray();
            Files = files.ToArray();
        }
    }
}