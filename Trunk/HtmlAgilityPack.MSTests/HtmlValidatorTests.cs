using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;

namespace HtmlAgilityPack.MSTests
{
    [TestClass]
    public class HtmlValidatorTests
    {
        [TestMethod]
        public void Validation_Is_True_For_Valid_HTML()
        {
            var validator = new HtmlValidator("a", "strong", "b", "em", "i", "br", "p", "ul", "ol", "li", "div");
            string[] errors;
            var result = validator.IsValid("Lorem<p>test</p><ul><li></li></ul><ol><li></li></ol><em></em><br /><br>Lorem<div><b>test</b><i>test</i></div><a href=\"http://www.bcjobs.ca\" target=\"_blank\">test</a>", out errors);

            Assert.IsTrue(result);
            Assert.AreEqual(0, errors.Count());
        }

        [TestMethod]
        public void Validation_Is_False_For_InValid_HTML()
        {
            var validator = new HtmlValidator("a", "strong", "b", "em", "i", "br", "p", "ul", "ol", "li", "div");
            string[] errors;
            var result = validator.IsValid("Lorem<p><ul>", out errors);

            Assert.IsFalse(result);
            Assert.AreEqual(1, errors.Count());
        }

        [TestMethod]
        public void Validation_Is_False_For_Disallowed_HTML()
        {
            var validator = new HtmlValidator("a", "strong", "b", "em", "i", "br", "p", "ul", "ol", "li", "div");
            string[] errors;
            var result = validator.IsValid("Lorem<script>alert('foo');</script>", out errors);

            Assert.IsFalse(result);
            Assert.AreEqual(1, errors.Count());
        }

        [TestMethod]
        public void Validation_Is_False_For_Disallowed_Attribute_Tag()
        {
            var validator = new HtmlValidator("a", "strong", "b", "em", "i", "br", "p", "ul", "ol", "li", "div");
            string[] errors;
            var result = validator.IsValid("Lorem<p class=\"red\">test</p>", out errors);

            Assert.IsFalse(result);
            Assert.AreEqual(1, errors.Count());
        }
    }

    class HtmlValidator
    {

        private readonly string[] _allowedTags;

        public HtmlValidator(params string[] allowedTags)
        {
            _allowedTags = allowedTags.Select(x => x.ToLower()).ToArray();
        }

        public bool IsValid(string html, out string[] errors)
        {

            var errorList = new List<string>();

            var document = new HtmlDocument();
            document.LoadHtml(html);

            if (document.ParseErrors.Count() > 0)
            {
                foreach (var error in document.ParseErrors)
                    errorList.Add(String.Format("HTML parse error: '{0}'", error.Reason));

                errors = errorList.ToArray();
                return false;
            }

            foreach (var node in document.DocumentNode.Descendants())
            {

                switch (node.NodeType)
                {
                    case HtmlNodeType.Comment:
                        errorList.Add(String.Format("HTML comment not allowed: '{0}'", node.InnerHtml));
                        break;
                    case HtmlNodeType.Document:
                        errorList.Add(String.Format("HTML document not allowed: '{0}'", node.InnerHtml));
                        break;
                    case HtmlNodeType.Element:
                        ValidateElement(node, errorList);
                        break;
                    case HtmlNodeType.Text:
                        break;
                    default:
                        throw new NotSupportedException(String.Format("Node type not supported: {0}.", node.NodeType));
                }

            }

            errors = errorList.ToArray();
            return errorList.Count < 1;
        }

        private void ValidateElement(HtmlNode node, List<string> errorList)
        {
            if (node.NodeType != HtmlNodeType.Element)
                throw new ArgumentException();

            if (!_allowedTags.Contains(node.Name.ToLower()))
            {
                errorList.Add(String.Format("Tag '{0}' not allowed.", node.Name));
                return;
            }

            if (node.HasAttributes)
            {
                foreach (var attribute in node.Attributes)
                {
                    if (node.Name.ToLower() == "a")
                    {
                        ValidateAnchorAttribute(node, attribute, errorList);
                    }
                    else
                    {
                        ValidateAttribute(node, attribute, errorList);
                    }
                }
            }
        }

        private void ValidateAttribute(HtmlNode node, HtmlAttribute attribute, List<string> errorList)
        {
            errorList.Add(String.Format("Attribute '{0}' not allowed on '{1}' tag.", attribute.Name, node.Name));
        }

        private void ValidateAnchorAttribute(HtmlNode node, HtmlAttribute attribute, List<string> errorList)
        {
            if (node.Name.ToLower() != "a")
                throw new ArgumentException();

            if (attribute.Name.ToLower() != "href" && attribute.Name.ToLower() != "target")
                errorList.Add(String.Format("Attribute '{0}' not allowed on '{1}' tag.", attribute.Name, node.Name));
        }
    }
}
