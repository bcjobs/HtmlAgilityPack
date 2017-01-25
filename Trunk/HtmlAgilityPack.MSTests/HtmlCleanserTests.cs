using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace HtmlAgilityPack.MSTests
{
    [TestClass]
    public class HtmlCleanserTests
    {
        [TestMethod]
        public void Removes_Span_Tags()
        {
            var cleanser = new HtmlCleanser("a", "strong", "b", "em", "i", "br", "p", "ul", "ol", "li", "div");
            var result = cleanser.Clean(@"<div><p><span lang=""\&quot; EN - GB\&quot; "">Flexibility</span></p></div>");

            Assert.AreEqual("<div><p>Flexibility</p></div>", result);
        }

        [TestMethod]
        public void Retains_Good_Html()
        {
            var html = @"<div><br></div><div><br></div>

<div>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Job Description</div><div><b>&nbsp;</b></div><div><b>&nbsp;</b></div><div><b>JOB TITLE:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; </b>Quality Engineer</div><div><b>DEPARTMENT:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; </b>Operations</div><div><b>REPORTS TO:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; </b>Quality Assurance and Manufacturing Services Manager</div><div><b>LOCATION:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; </b>Viola</div><div><b>&nbsp;</b></div><div><b>JOB SUMMARY:</b></div><div>To develop, install and maintain cost-effective methods of Quality control, monitoring, and improvement.</div><div><b>ESSENTIAL FUNCTIONS:</b></div><div>·&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Thoroughly understand and adhere to the department goals, objectives, and strategy </div><div>·&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Establish <b>and maintain </b>credibility <i>throughout </i>the organization as an effective developer of solutions </div><div>Present and maintain positive m</div><div><br></div><ol><li>line 1</li><li>line 2</li><li>line 3</li><li><br></li><li><br></li></ol><div><br></div><ul><li>line 1</li><li>line 2</li><li>line 3 test <a target=""_blank"" href=""http://test.com"">test</a> tes</li></ul><div><br></div>";

            var cleanser = new HtmlCleanser("a", "strong", "b", "em", "i", "br", "p", "ul", "ol", "li", "div");
            var result = cleanser.Clean(html);

            Assert.AreEqual(html, result);
        }

        [TestMethod]
        public void Retains_Good_Html_While_Tripping_Invalid()
        {
            var html = @"<div><br></div><div><br></div>
<p>foo</p>
<div>&nbsp;&nbsp; Job Description</div>
<div><b>&nbsp;</b></div><div><b>&nbsp;</b></div>
<div><b class=""color:red;"">JOB TITLE:&nbsp;&nbsp;&</b>Quality Engineer</div>
<script>alert('foo');</script>
<script type=""text/javascript"">alert('foo');</script>
<div>Present and maintain positive m</div><div><br></div>
<ol><li>line 1</li><li>line 2</li><li>line 3</li><li><br></li><li><br></li></ol>
<div><br></div>
<ul><li>line 1</li><li>line 2</li><li>line 3 test <a target=""_blank"" href=""http://test.com"">test</a> tes</li></ul>
<div><br></div>";

            var cleanser = new HtmlCleanser("a", "strong", "b", "em", "i", "br", "p", "ul", "ol", "li", "div");
            var result = cleanser.Clean(html);

            var expected = @"<div><br></div><div><br></div>
<p>foo</p>
<div>&nbsp;&nbsp; Job Description</div>
<div><b>&nbsp;</b></div><div><b>&nbsp;</b></div>
<div><b>JOB TITLE:&nbsp;&nbsp;&</b>Quality Engineer</div>
alert('foo');
alert('foo');
<div>Present and maintain positive m</div><div><br></div>
<ol><li>line 1</li><li>line 2</li><li>line 3</li><li><br></li><li><br></li></ol>
<div><br></div>
<ul><li>line 1</li><li>line 2</li><li>line 3 test <a target=""_blank"" href=""http://test.com"">test</a> tes</li></ul>
<div><br></div>";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Bad_Html_Gets_Cleansed()
        {
            var html = @"<div><br></div><div><br></div>
<p>foo</p>
<div>Present </p>and <a maintain positive m</div><div><br></div>
<ol><li>line 1</li><li>line 2</li><li>line 3</li><li><br></li><li><br></li></ol>
<div><br></div>";


            var cleanser = new HtmlCleanser("a", "strong", "b", "em", "i", "br", "p", "ul", "ol", "li", "div");
            var result = cleanser.Clean(html);

            var expected = @"<div><br></div><div><br></div>
<p>foo</p>
<div>Present <p>and <a></a></div><div><br></div>
<ol><li>line 1</li><li>line 2</li><li>line 3</li><li><br></li><li><br></li></ol>
<div><br></div>";

            Assert.AreEqual(expected, result);
        }
    }

    class HtmlCleanser
    {

        private readonly string[] _allowedTags;

        public HtmlCleanser(params string[] allowedTags)
        {
            _allowedTags = allowedTags.Select(x => x.ToLower()).ToArray();
        }

        public string Clean(string html)
        {

            var document = new HtmlDocument();
            document.LoadHtml(html);

            if (document.ParseErrors.Count() > 0)
                return "";

            for (int i = document.DocumentNode.Descendants().Count() - 1; i >= 0; i--)
            {

                var node = document.DocumentNode.Descendants().ElementAt(i);

                switch (node.NodeType)
                {
                    case HtmlNodeType.Comment:
                        node.ParentNode.RemoveChild(node, true);
                        break;
                    case HtmlNodeType.Document:
                        return "";
                    case HtmlNodeType.Element:
                        CleanElement(node);
                        break;
                    case HtmlNodeType.Text:
                        break;
                    default:
                        throw new NotSupportedException(String.Format("Node type not supported: {0}.", node.NodeType));
                }

            }

            return document.DocumentNode.OuterHtml;
        }

        private void CleanElement(HtmlNode node)
        {
            if (node.NodeType != HtmlNodeType.Element)
                throw new ArgumentException();

            if (!_allowedTags.Contains(node.Name.ToLower()))
            {
                node.ParentNode.RemoveChild(node, true);
                return;
            }

            if (node.HasAttributes)
            {
                for (int i = node.Attributes.Count - 1; i >= 0; i--)
                {

                    var attribute = node.Attributes[i];

                    switch (node.Name.ToLower())
                    {
                        case "a":
                            CleanAnchorAttribute(node, attribute);
                            break;
                        case "img":
                            CleanImageAttribute(node, attribute);
                            break;
                        default:
                            CleanAttribute(node, attribute);
                            break;
                    }
                }

            }
        }

        private void CleanAttribute(HtmlNode node, HtmlAttribute attribute)
        {
            node.Attributes.Remove(attribute);
        }

        private void CleanAnchorAttribute(HtmlNode node, HtmlAttribute attribute)
        {
            if (node.Name.ToLower() != "a")
                throw new ArgumentException();

            if (attribute.Name.ToLower() != "href" && attribute.Name.ToLower() != "target" && attribute.Name.ToLower() != "title")
                node.Attributes.Remove(attribute);
        }

        private void CleanImageAttribute(HtmlNode node, HtmlAttribute attribute)
        {
            if (node.Name.ToLower() != "img")
                throw new ArgumentException();

            // this is primarily for Jobg8, as they include a tracking code in each job description that looks like this:
            // <img src="http://www.jobg8.com/Tracking.aspx?zeq3XfyLuKawEkgG85pI%2bwu" width="0" height="0" />
            if (attribute.Name.ToLower() != "src" && attribute.Name.ToLower() != "width" && attribute.Name.ToLower() != "height")
                node.Attributes.Remove(attribute);
        }

    }
}
