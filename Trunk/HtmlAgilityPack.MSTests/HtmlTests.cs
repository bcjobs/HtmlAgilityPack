using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.IO;

namespace HtmlAgilityPack.MSTests
{
    [TestClass]
    public class HtmlTests
    {
        [TestMethod]
        public void Can_Strip_HTML()
        {
            var document = new HtmlDocument();
            document.LoadHtml("<div>Foo bar<ul><li>Line 1</li><li>Line 2</li></ul></div>");
            var result = document.DocumentNode.InnerText;

            Assert.AreEqual("Foo barLine 1Line 2", result);
        }

        [TestMethod]
        public void Can_Stripe_Word_HTML()
        {
            string wordHtml = Directory.GetCurrentDirectory().Replace(@"\bin\Debug", @"\files\resume1.html");
            var document = new HtmlDocument();
            document.Load(wordHtml);
            var result = document.DocumentNode.InnerText;

            string expected = File.ReadAllText(Directory.GetCurrentDirectory().Replace(@"\bin\Debug", @"\files\resume1_result.html"));
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Set_Anchor_Target_Blank()
        {
            var document = new HtmlDocument();
            document.LoadHtml("<div><span>test</span><a href=\"foo.html\">link</a><br /><br /><a href=\"bar.html\">link</a></div>");

            var anchors = document.DocumentNode.SelectNodes("//a[@href]");
            foreach (var node in anchors)
                node.SetAttributeValue("target", "_blank");

            var result = document.DocumentNode.OuterHtml;

            Assert.AreEqual("<div><span>test</span><a href=\"foo.html\" target=\"_blank\">link</a><br><br><a href=\"bar.html\" target=\"_blank\">link</a></div>", result);
        }

        [TestMethod]
        public void RemoveChild_Retains_Ordering()
        {
            // https://htmlagilitypack.codeplex.com/workitem/43552

            var document = new HtmlDocument();
            document.LoadHtml("<div><span>a<em>b</em>c</span></div>");
            var span = document.DocumentNode.Descendants().First(n => n.Name == "span");
            span.ParentNode.RemoveChild(span, true);
            var result = document.DocumentNode.OuterHtml;

            Assert.AreEqual("<div>a<em>b</em>c</div>", result);
        }
    }
}
