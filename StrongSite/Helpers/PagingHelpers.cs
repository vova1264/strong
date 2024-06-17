using System;
using System.Text;
using System.Web.Mvc;

using StrongSite.Operations;

namespace StrongSite.Helpers
{
    public static class PagingHelpers
    {
        public static MvcHtmlString PageLinks(this HtmlHelper html, PageViewModel pageModel, Func<int, string> pageUrl)
        {
            TagBuilder ul = new TagBuilder("ul");
            ul.AddCssClass("pagination");

            int startPage = Math.Max(1, pageModel.PageNumber - 2);
            int endPage = Math.Min(startPage + 3, pageModel.TotalPages);

            if (startPage > 1)
            {
                TagBuilder li = new TagBuilder("li");
                TagBuilder a = new TagBuilder("a");
                a.AddCssClass("btn btn-default text-coclor");
                a.MergeAttribute("href", pageUrl(1));
                a.InnerHtml = "1";
                li.InnerHtml = a.ToString();
                ul.InnerHtml += li.ToString();
            }

            if (startPage > 2)
            {
                TagBuilder li = new TagBuilder("li");
                li.InnerHtml = "<span>...</span>";
                ul.InnerHtml += li.ToString();
            }

            for (int i = startPage; i <= endPage; i++)
            {
                TagBuilder li = new TagBuilder("li");

                if (i == pageModel.PageNumber)
                {
                    li.AddCssClass("selected btn-primary btn btn-default text-coclor");
                    li.InnerHtml = $"<span>{i}</span>";
                }
                else
                {
                    TagBuilder a = new TagBuilder("a");
                    a.MergeAttribute("href", pageUrl(i));
                    a.InnerHtml = i.ToString();
                    a.AddCssClass("btn btn-default text-coclor");
                    li.InnerHtml = a.ToString();
                }

                ul.InnerHtml += li.ToString();
            }

            if (endPage < pageModel.TotalPages - 1)
            {
                TagBuilder li = new TagBuilder("li");
                li.InnerHtml = "<span>...</span>";
                ul.InnerHtml += li.ToString();
            }

            if (endPage < pageModel.TotalPages)
            {
                TagBuilder li = new TagBuilder("li");
                TagBuilder a = new TagBuilder("a");
                a.AddCssClass("btn btn-default text-coclor");
                a.MergeAttribute("href", pageUrl(pageModel.TotalPages));
                a.InnerHtml = pageModel.TotalPages.ToString();
                li.InnerHtml = a.ToString();
                ul.InnerHtml += li.ToString();
            }

            return MvcHtmlString.Create(ul.ToString());
        }

    }
}