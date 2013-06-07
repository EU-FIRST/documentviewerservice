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
            fullFileName = fullFileName.Replace(".html.gz", ".xml.gz"); // bug workaround
            if (!LUtils.VerifyFileNameOpen(fullFileName))
            {
                ViewBag.ErrorMessage = "Document file name invalid or file not found.";
                ViewBag.Details = "Document file name: " + fullFileName;
                return false;
            }
            return true;
        }

        [ActionName("View")]
        public ActionResult _View(string docId)
        {
            string fullFileName;
            if (!CheckRequest(docId, out fullFileName))
            {
                return View("Error");
            }
            Document d = new Document("", "");
            d.ReadXmlCompressed(fullFileName);
            ArrayList<object> treeItems, features, content;
            DocumentSerializer.SerializeDocument(d, out treeItems, out features, out content);
            // fill ViewBag
            ViewBag.Title = d.Name;
            ViewBag.TreeItemsParam = treeItems;
            ViewBag.FeaturesParam = features;
            ViewBag.ContentParam = content;
            // render
            return View("View");
        }

        [ActionName("Redirect")]
        public ActionResult _Redirect(string docId)
        {
            string fullFileName;
            if (!CheckRequest(docId, out fullFileName))
            {
                return View("Error");
            }
            Document d = new Document("", "");
            d.ReadXmlCompressed(fullFileName);
            string url = d.Features.GetFeatureValue("responseUrl");
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
            string fullFileName;
            if (!CheckRequest(docId, out fullFileName))
            {
                return View("Error");
            }
            if (!includeBoilerplate.HasValue)
            {
                includeBoilerplate = false;
            }
            Document d = new Document("", "");
            d.ReadXmlCompressed(fullFileName);
            ViewBag.ErrorMessage = "Not implemented.";
            ViewBag.Details = "Action name: Text";
            return View("Error");
        }

        public ActionResult Xml(string docId)
        {
            string fullFileName;
            if (!CheckRequest(docId, out fullFileName))
            {
                return View("Error");
            }
            Document d = new Document("", "");
            d.ReadXmlCompressed(fullFileName);
            ViewBag.ErrorMessage = "Not implemented.";
            ViewBag.Details = "Action name: Xml";
            return View("Error");
        }
    }
}
