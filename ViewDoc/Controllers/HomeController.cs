/*==========================================================================;
 *
 *  (c) Sowa Labs. All rights reserved.
 *
 *  File:    HomeController.js
 *  Desc:    Document viewer controller
 *  Created: Apr-2013
 *
 *  Author:  Miha Grcar
 *
 ***************************************************************************/

using System;
using System.Web.Mvc;
using System.Data.SqlClient;
using Latino;
using Latino.Workflows.TextMining;

using LUtils
    = Latino.Utils;

namespace DocumentViewer.Controllers
{
    /* .-----------------------------------------------------------------------
       |
       |  Class HomeController
       |
       '-----------------------------------------------------------------------
    */
    public class HomeController : Controller
    {
        public ActionResult Index(string docId)
        {
            if (docId == null)
            {
                ViewBag.ErrorMessage = "Parameter missing.";
                ViewBag.Details = "Parameter name: docId";
                return View("Error");
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
                return View("Error");                
            }
            string fileName = Utils.GetFileName(docGuid);
            if (fileName == null)
            {
                ViewBag.ErrorMessage = "Document not found.";
                ViewBag.Details = "Document ID: " + docId;
                return View("Error");
            }
            string fullFileName = LUtils.GetConfigValue<string>("DataRoot").TrimEnd('\\') + "\\" + fileName;
            if (!LUtils.VerifyFileNameOpen(fileName))
            {
                ViewBag.ErrorMessage = "Document file name invalid or file not found.";
                ViewBag.Details = "Document file name: " + fullFileName;
                return View("Error");            
            }
            // read document
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
            return View("Viewer");
        }
    }
}
