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
using System.Text;
using System.Web.Mvc;
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
        private bool CheckRequest(string docId, out string fullFileName)
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
            fullFileName = LUtils.GetConfigValue<string>("DataRoot").TrimEnd('\\') + "\\" + fileName;
            if (!LUtils.VerifyFileNameOpen(fullFileName))
            {
                ViewBag.ErrorMessage = "Document file name invalid or file not found.";
                ViewBag.Details = "Document file name: " + fullFileName;
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
