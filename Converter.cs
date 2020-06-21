﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Notion2TistoryConsole
{
    class Converter
    {
        string comment = "<p>\n</p><p class=\"block-color-gray\">Uploaded by Notion2Tistory v1.0</p>";
        public Content GetContent(string c)
        {
            string header = ExtractHeader(c);

            string title = GetTitle(header);

            Content content = new Content(title);

            Dictionary<string, string> Table = ReadTable(header);

            content.SetVisbility(GetVisibilityType(Table));
            content.SetCategory(GetCategoryId(Table));
            content.SetTags(GetTags(Table));
            content.SetCommentAccept(GetAcceptComment(Table));
            content.SetPassword(GetPassword(Table));

            content.SetContent(ChangeHtml(c));

            return content;
        }

        public string ExtractHeader(string c)
        {
            string header = c.Split("<header>")[1].Split("</header>")[0];
            return header;
        }

        private string GetTitle(string header)
        {
            string title = "Notion Page";
            try
            {
                title = header.Split("<h1 class=\"page-title\">")[1].Split("</h1>")[0];
                Console.WriteLine("Page title : {0}", title);
            }
            catch
            {
                Console.WriteLine("Error : Can't find page title");
                Console.WriteLine("Default value is 'Notion Page'");
            }
            return title;
        }

        private int GetVisibilityType(Dictionary<string, string> table)
        {
            int type = 0;
            try
            {
                try
                {
                    string rowValue = table["select" + "Visibility"].Split("\">")[1].Split("</span>")[0];
                    try
                    {
                        if (rowValue == "Private")
                        {
                            type = 0;
                        }
                        else if (rowValue == "Protect")
                        {
                            type = 1;
                        }
                        else if (rowValue == "Public")
                        {
                            type = 2;
                        }
                        else
                        {
                            Exception e = new Exception();
                            throw e;
                        }
                        Console.WriteLine("Visbility type : {0}", rowValue);
                    }
                    catch
                    {
                        Console.WriteLine("Error : Can't read Visbility type");
                        Console.WriteLine("Default value is 'false'");
                    }
                }
                catch
                {
                    Console.WriteLine("Error : Can't find Visbility row");
                    Console.WriteLine("Default value is 'false'");
                }
            }
            catch
            {
                Console.WriteLine("Error : Can't extract CommentAccept");
                Console.WriteLine("Default value is 'false'");
            }
            return type;
        }

        private int GetCategoryId(Dictionary<string, string> table)
        {
            int category = 0;
            try
            {
                try
                {
                    string rowValue = table["relation" + "Category"];
                    try
                    {
                        // 시도
                    }
                    catch
                    {
                        Console.WriteLine("Error : Can't read Category-id");
                        Console.WriteLine("Default value is '0'");
                    }
                }
                catch
                {
                    Console.WriteLine("Error : Can't find Category row");
                    Console.WriteLine("Default value is '0'");
                }
            }
            catch
            {
                Console.WriteLine("Error : Can't extract Category-id");
                Console.WriteLine("Default value is '0'");
            }
            return category;
        }

        private List<string> GetTags(Dictionary<string, string> table)
        {
            List<string> tags = new List<string>();
            try
            {
                try
                {
                    string rowValue = table[ "multi_select" + "Tags" ];
                    try
                    {
                        tags = new List<string>(rowValue.Split("</span>"));
                        tags.Remove("");
                        tags = tags.Select(tag => tag.Split("\">")[1]).ToList();
                        Console.WriteLine("Tags : {0}", string.Join(",", tags));
                    }
                    catch
                    {
                        Console.WriteLine("Error : Can't read tags");
                    }
                }
                catch
                {
                    Console.WriteLine("Error : Can't find Tags row");
                }
            }
            catch
            {
                Console.WriteLine("Error : Can't extract tags");
            }
            return tags;
        }

        private bool GetAcceptComment(Dictionary<string, string> table)
        {
            bool accept = false;
            try
            {
                try
                {
                    string rowValue = table[ "checkbox" + "Comment" ];
                    try
                    {
                        if (rowValue == "<div class=\"checkbox checkbox-on\"></div>")
                        {
                            accept = true;
                        }
                        else if (rowValue == "<div class=\"checkbox checkbox-off\"></div>")
                        {
                            accept = false;
                        }
                        else
                        {
                            Exception e = new Exception();
                            throw e;
                        }
                        Console.WriteLine("Accept Comment : {0}", accept.ToString());
                    }
                    catch
                    {
                        Console.WriteLine("Error : Can't read CommentAccept");
                        Console.WriteLine("Default value is 'false'");
                    }
                }
                catch
                {
                    Console.WriteLine("Error : Can't find Comment row");
                    Console.WriteLine("Default value is 'false'");
                }
            }
            catch
            {
                Console.WriteLine("Error : Can't extract CommentAccept");
                Console.WriteLine("Default value is 'false'");
            }
            return accept;
        }

        private string GetPassword(Dictionary<string, string> table)
        {
            string password = "";
            try
            {
                try
                {
                    string rowValue = table["text" + "Password"];
                    try
                    {
                        // 시도
                    }
                    catch
                    {
                        Console.WriteLine("Error : Can't read Password");
                        Console.WriteLine("Default value is ''");
                    }
                }
                catch
                {
                    Console.WriteLine("Error : Can't find Password row");
                    Console.WriteLine("Default value is ''");
                }
            }
            catch
            {
                Console.WriteLine("Error : Can't extract Password");
                Console.WriteLine("Default value is ''");
            }
            return password;
        }

        private Dictionary<string, string> ReadTable(string table)
        {
            Dictionary<string, string> Table = new Dictionary<string, string>();
            // type : number, relation, select, multiselect, text, checkbox...
            static string DeleteIcon(string s)
            {
                if (s.Contains("<span class=\"icon property-icon\">"))
                {
                    string[] icon = { "<span class=\"icon property-icon\">", "</svg></span>" };
                    int iconStart = s.IndexOf(icon[0]);
                    int iconEnd = s.IndexOf(icon[1]) + icon[1].Length;
                    s = s.Substring(0, iconStart) + s.Substring(iconEnd, s.Length - iconEnd);
                }
                return s;
            }
            table = SubByString(table, "<tbody>", "</tbody>");
            List<string> rows = table.Split("</tr>").ToList();
            rows.Remove("");
            rows.Select(row => DeleteIcon(row));
            foreach(string row in rows)
            {
                string r = DeleteIcon(row);
                string type, name, content;
                type = SubByString(r, "<tr class=\"property-row property-row-", "\">");
                name = SubByString(r, "<th>", "</th>");
                Console.WriteLine("type : {0} | name : {1}", type, name);
                content = SubByString(r, "<td>", "</td>");
                Table.Add(type + name, content);
            }
            /*
            var lines = Table.Select(kvp => kvp.Key[0] + " & " + kvp.Key[1] + " : " + kvp.Value.ToString());
            Console.WriteLine(string.Join(Environment.NewLine, lines));
            */
            return Table;
        }


        private string ChangeHtml(string content)
        {
            // <body> 태그 안쪽만 남김 (<article> 태그)
            string changedContent = SubByString(content, "<body>", "</body>");
            
            // 헤더 삭제
            changedContent = changedContent.Split("<header>")[0] + changedContent.Split("</header>")[1];

            // Notion_P CSS 적용
            changedContent = changedContent.Replace("page sans\">", "Notion_P page sans\">");

            // 토글 전부 접기
            changedContent = changedContent.Replace("class=\"toggle\"><li><details open=\"\"><summary>", "class=\"toggle\"><li><details><summary>");

            // Embed 형식으로 변환
            changedContent = MakeEmbed(changedContent);

            // 끝에 주석 추가
            changedContent += comment;

            return changedContent;
        }

        private static string MakeEmbed(string content)
        {
            string url;
            string[] aTag = new string[] {
                "<div class=\"source\"><a href=\"",
                "\">",
                "</a></div></figure>"
            };
            string[] emebedaTag = new string[] {
                "<div class=\"embed_container\"><a href=\"",
                "\"><iframe src=\"",
                "\" class=\"embed_inner\"></iframe>"
            };

            int mark1, mark2, mark3;
            mark1 = 0;

            while (content.Substring(mark1).Contains(aTag[0]))
            {
                mark1 += content.Substring(mark1).IndexOf(aTag[0]) + 29;
                mark2 = content.Substring(mark1).IndexOf(aTag[1]);
                mark3 = mark1 + content.Substring(mark1).IndexOf(aTag[2]);
                url = content.Substring(mark1, mark2);
                Console.WriteLine("<this is url>");
                Console.WriteLine(url);
                url = GetEmbedUrl(url);
                Console.WriteLine("<this is converted url>");
                Console.WriteLine(url);
                content = content.Substring(0, mark1 - 29) + emebedaTag[0] + url + emebedaTag[1]
                    + url + emebedaTag[2] + content.Substring(mark3);
            }
            return content;
        }
        private static string GetEmbedUrl(string url)
        {
            string website;

            website = SubByString(url, "://", "#");
            website = SubByString(website , "#", "/");

            if (website == "www.youtube.com")
            {
                url = url.Replace("watch?v=", "embed/");
            }
            else if (website == "codepen.io")
            {
                url = url.Replace("/pen/", "/embed/") + "?theme-id=dark&default-tab=html,result";
            }
            else if (website == "whimsical.com")
            {
                url = url.Replace(".com/", ".com/embed/");
            }
            else if (website == "www.figma.com")
            {
                byte[] byteDataParams = UTF8Encoding.UTF8.GetBytes(url);
                string encodedString = System.Web.HttpUtility.UrlEncode(byteDataParams, 0, byteDataParams.Length);
                url = "https://www.figma.com/embed?embed_host=share&url=" + encodedString;
            }

            return url;
        }

        // 자주 사용하는 함수
        private static string SubByString(string s, string from, string to)
        {
            if (s.Contains(from))
            {
                s = s.Substring(s.IndexOf(from) + from.Length);
            }
            if (s.Contains(to))
            {
                s = s.Substring(0, s.IndexOf(to));
            }
            return s;
        }
    }
}
