/*==========================================================================;
 *
 *  (c) Sowa Labs. All rights reserved.
 *
 *  File:    DocumentViewerController.js
 *  Desc:    Document viewer controller
 *  Created: Apr-2013
 *
 *  Author:  Miha Grcar
 *
 ***************************************************************************/

using System;
using System.IO;
using System.Text;
using System.Web.Mvc;
using System.IO.Compression;
using Latino;
using Latino.Workflows.TextMining;

using LUtils
    = Latino.Utils;

namespace DocumentViewer.Controllers
{
    /* .-----------------------------------------------------------------------
       |
       |  Class DocumentViewerController
       |
       '-----------------------------------------------------------------------
    */
    public class DocumentViewerController : Controller
    {
        private bool CheckRequest(string docId, out string fullFileName, bool htmlFile = false)
        {
            fullFileName = null;
            if (docId == null)
            {
                ViewBag.ErrorMessage = "Parameter missing.";
                ViewBag.Details = "Parameter name: docId";
                return false;
            }
            Guid docGuid;
            try
            {
                docGuid = new Guid(docId);
            }
            catch
            {
                ViewBag.ErrorMessage = "Invalid parameter value.";
                ViewBag.Details = "Parameter name: docId";
                return false;
            }
            string fileName = Utils.GetFileName(docGuid);
            if (fileName == null)
            {
                ViewBag.ErrorMessage = "Document not found.";
                ViewBag.Details = "Document ID: " + docId;
                return false;
            }
            if (htmlFile) { fileName = fileName.Replace(".xml.gz", ".html.gz"); }
            string[] dataRoots = LUtils.GetConfigValue<string>(htmlFile ? "HtmlRoot" : "DataRoot").Split(';');
            bool success = false;
            string fileNames = "";
            foreach (string dataRoot in dataRoots)
            {
                fullFileName = dataRoot.TrimEnd('\\') + "\\" + fileName;
                fileNames += fullFileName + ", ";
                if (LUtils.VerifyFileNameOpen(fullFileName))
                {
                    success = true;
                    break;
                }
            }
            if (!success) 
            {
                ViewBag.ErrorMessage = "Document file name invalid or file not found.";
                ViewBag.Details = "Assumed document file names: " + fileNames.Substring(0, fileNames.Length - 2);
                return false;
            }
            return true;
        }

        [ActionName("View")]
        public ActionResult ViewAction(string docId)
        {
            string fileName;
            if (!CheckRequest(docId, out fileName))
            {
                return View("Error");
            }
            Document doc = new Document("", "");
            doc.ReadXmlCompressed(fileName);
            ArrayList<object> treeItems, features, content;
            DocumentSerializer.SerializeDocument(doc, out treeItems, out features, out content);
            // fill ViewBag
            ViewBag.Title = doc.Name;
            ViewBag.TreeItemsParam = treeItems;
            ViewBag.FeaturesParam = features;
            ViewBag.ContentParam = content;
            // render
            return View("View");
        }

        public ActionResult Html(string docId)
        {
            string xmlFileName, htmlFileName;
            if (!CheckRequest(docId, out xmlFileName) || !CheckRequest(docId, out htmlFileName, /*htmlFile=*/true))
            {
                return View("Error");
            }
            Document doc = new Document("", "");
            doc.ReadXmlCompressed(xmlFileName);
            string charSet = doc.Features.GetFeatureValue("charSet");
            byte[] bytes;
            using (FileStream stream = new FileStream(htmlFileName, FileMode.Open))
            {
                using (GZipStream gzStream = new GZipStream(stream, CompressionMode.Decompress))
                {
                    bytes = gzStream.ReadAllBytes(/*sizeLimit=*/0);
                }
            }
            return File(bytes, "text/html; charset=" + charSet);
        }

        [ActionName("Redirect")]
        public ActionResult RedirectAction(string docId)
        {
            string fileName;
            if (!CheckRequest(docId, out fileName))
            {
                return View("Error");
            }
            Document doc = new Document("", "");
            doc.ReadXmlCompressed(fileName);
            string url = doc.Features.GetFeatureValue("responseUrl");
            if (url == null)
            {
                ViewBag.ErrorMessage = "Document URL not found.";
                ViewBag.Details = "Feature name: responseUrl";
                return View("Error");
            }
            return Redirect(url);
        }

        public ActionResult Text(string docId, bool? includeBoilerplate)
        {
            string fileName;
            if (!CheckRequest(docId, out fileName))
            {
                return View("Error");
            }
            if (!includeBoilerplate.HasValue)
            {
                includeBoilerplate = false;
            }
            Document doc = new Document("", "");
            doc.ReadXmlCompressed(fileName);
            StringBuilder txt = new StringBuilder();
            string selector = includeBoilerplate.Value ? "TextBlock" : "TextBlock/Content";
            foreach (TextBlock textBlock in doc.GetAnnotatedBlocks(selector))
            {
                txt.AppendLine(textBlock.Text);
            }
            return Content(txt.ToString(), "text/plain");
        }

        public ActionResult Xml(string docId)
        {
            string fileName;
            if (!CheckRequest(docId, out fileName))
            {
                return View("Error");
            }
            Document doc = new Document("", "");
            doc.ReadXmlCompressed(fileName);         
            return Content(doc.GetXml(), "application/xml");
        }
    }
}
