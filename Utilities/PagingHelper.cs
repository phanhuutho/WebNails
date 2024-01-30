using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace WebNails.Utilities
{
    public class PagingHelper
    {
        public static int CountSort
        {
            get
            {
                return 20;
            }
        }
        public static int PageIndex
        {
            get
            {
                var page = 1;
                if (HttpContext.Current.Request.QueryString["page"] != null)
                {
                    page = int.Parse(HttpContext.Current.Request.QueryString["page"]);
                }
                return page;
            }
        }
        public static int Skip
        {
            get
            {
                return (PageIndex - 1) * CountSort;
            }
        }
        public static int CurrentIndex(int countSort, int pageIndex)
        {
            return (countSort * (pageIndex - 1)) + 1;
        }
        public static int CurrentIndexCountDown(int totalRecord, int totalPage, int countSort, int pageIndex)
        {
            var index = totalRecord;
            if ((totalPage - pageIndex) * countSort < totalRecord)
            {
                index = (totalPage - pageIndex) * countSort;
                if (totalRecord % countSort == 0)
                {
                    index = totalPage * countSort;
                }
                else
                {
                    index = index + (totalRecord % countSort);
                }
            }
            return index;
        }
        public static int CountPage(int totalRecord, int countSort)
        {
            var result = (int)(totalRecord / countSort);
            if (totalRecord % countSort > 0)
                result = result + 1;
            return result;
        }
        public static string Page(int totalRecord, int countPageShow, int countRecordShow, string className)
        {
            var pageIndex = PageIndex;

            var myUri = new Uri(HttpContext.Current.Request.Url.AbsoluteUri); // Get Url
            var pathQuery = myUri.Query; // Get full querystring

            var charracter = "?";
            if (pathQuery != string.Empty)
            {
                var checkPathQuery = "?page=";
                if (!pathQuery.Contains(checkPathQuery))
                {
                    var words = Regex.Split(pathQuery, "&page=");
                    pathQuery = words[0];
                    charracter = "&";
                }
                else
                {
                    pathQuery = string.Empty;
                }
            }

            var rawHtml = string.Empty;

            //max count page
            var maxPage = totalRecord / countRecordShow;
            if (totalRecord % countRecordShow > 0)
                maxPage = maxPage + 1;

            if (maxPage > 1)
            {
                rawHtml = "<ul class='" + className + "'>";
                //page start
                var start = (pageIndex / countPageShow) * countPageShow;
                if (start == 0)
                    start = 1;

                //page end
                var end = start == 1 ? start + (countPageShow - 1) : start + countPageShow;

                //page next
                var next = pageIndex + 1;

                //page previous
                var previous = pageIndex - 1;

                //page fisrt
                //var first = 1;

                //page last
                //var last = maxPage;

                //show First page
                if (pageIndex > 1)
                {
                    rawHtml = rawHtml + string.Format("<li class='page-item'><a class='page-link' href='{0}{1}page={2}'>«</a></li>", pathQuery, charracter, 1);
                    rawHtml = rawHtml + string.Format("<li class='page-item'><a class='page-link' href='{0}{1}page={2}'>←</a></li>", pathQuery, charracter, previous);
                }


                //list page
                for (var i = start; i <= maxPage && i <= end; i++)
                {
                    if (i == pageIndex)
                        rawHtml = rawHtml + string.Format("<li class='page-item'><a class='page-link active' href='#'>{0}</a></li>", i);
                    else
                        rawHtml = rawHtml + string.Format("<li class='page-item'><a class='page-link' href='{0}{1}page={2}'>{2}</a></li>", pathQuery, charracter, i);
                }

                //show Last page
                if (pageIndex < maxPage)
                {
                    rawHtml = rawHtml + string.Format("<li class='page-item'><a class='page-link' href='{0}{1}page={2}'>→</a></li>", pathQuery, charracter, next);
                    rawHtml = rawHtml + string.Format("<li class='page-item'><a class='page-link' href='{0}{1}page={2}'>»</a></li>", pathQuery, charracter, maxPage);
                }

                rawHtml += "</ul>";
            }
            return rawHtml;
        }
    }
}