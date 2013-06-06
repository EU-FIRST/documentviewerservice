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
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Web;
using Latino;
using Latino.Workflows.TextMining;

namespace ViewDoc.Controllers
{
    /* .-----------------------------------------------------------------------
       |
       |  Class HomeController
       |
       '-----------------------------------------------------------------------
    */
    public class HomeController : Controller
    {
        /* .-----------------------------------------------------------------------
           |
           |  Class AnnotationInfo
           |
           '-----------------------------------------------------------------------
        */
        private class AnnotationInfo
        {
            public int mIdx;
            public int mAnnotationId;
            public bool mIsSpanStart;
            public Annotation mAnnotation;
            public bool mIsLeaf;

            public AnnotationInfo(Annotation a, int id, bool isSpanStart, bool isLeaf)
            {
                mAnnotationId = id;
                mIsSpanStart = isSpanStart;
                mIdx = isSpanStart ? a.SpanStart : a.SpanEnd;
                mAnnotation = a;
                mIsLeaf = isLeaf;
            }
        }

        static object EncodeState(Dictionary<int, Set<Annotation>> state)
        {
            ArrayList<object> stateEnc = new ArrayList<object>();
            foreach (KeyValuePair<int, Set<Annotation>> stateItem in state.OrderByDescending(x => x.Key))
            {
                if (stateItem.Value.Sum(x => x.Features.Count()) > 0)
                {
                    ArrayList<object> featEnc = new ArrayList<object>();
                    stateEnc.Add(featEnc);
                    featEnc.Add(stateItem.Key);
                    int i = 1;
                    foreach (Annotation annot in stateItem.Value)
                    {                        
                        foreach (KeyValuePair<string, string> featInfo in annot.Features)
                        {
                            if (stateItem.Value.Count > 1)
                            {
                                featEnc.Add("(" + i + ") " + featInfo.Key);
                            }
                            else
                            {
                                featEnc.Add(featInfo.Key);
                            }
                            featEnc.Add(featInfo.Value);                            
                        }
                        i++;
                    }
                } 
                else { stateEnc.Add(stateItem.Key); }
            }
            return stateEnc;
        }

        static string ProcessDocumentFeatureValue(string name, string val)
        {
            val = val.ToOneLine(/*compact=*/true);
            if (val.StartsWith("http://") || val.StartsWith("https://"))
            {
                return string.Format("<a target=\"_blank\" href=\"{0}\">{0}</a>", val, HttpUtility.HtmlEncode(val));
            }
            else if (val.Length > 400)
            {
                return HttpUtility.HtmlEncode(val.Truncate(400)) + "...";
            }
            return HttpUtility.HtmlEncode(val);
        }

        static string ProcessAnnotationFeatureValue(string name, string val)
        {
            val = val.ToOneLine(/*compact=*/true);
            if (val.Length > 100)
            {
                return HttpUtility.HtmlEncode(val.Truncate(400)) + "...";
            }
            return HttpUtility.HtmlEncode(val);
        }

        public ActionResult Index()
        {
            Dictionary<string, int> idMapping = new Dictionary<string, int>();
            // GetId
            Func<string, int> GetId = delegate(string name) {
                int id;
                if (idMapping.TryGetValue(name, out id)) { return id; }
                idMapping.Add(name, id = idMapping.Count);
                return id;
            };
            string fn = @"C:\Work\SowaLabsSnippets\AnnotatedDocXmlToHtml\00_00_37_94e7f5ec8968de6aa3a6893c0dc36203.xml.gz";
            Document d = new Document("", "");
            d.ReadXmlCompressed(fn);
            ArrayList<AnnotationInfo> data = new ArrayList<AnnotationInfo>();
            Set<string> tabu = new Set<string>();
            ArrayList<Pair<ArrayList<int>, string>> treeItems = new ArrayList<Pair<ArrayList<int>, string>>();
            foreach (Annotation a in d.Annotations)
            {
                if (!tabu.Contains(a.Type))
                {
                    tabu.Add(a.Type);
                    string[] fullPath = a.Type.Split('/', '\\');
                    string path = "";
                    ArrayList<int> idList = new ArrayList<int>();
                    foreach (string pathItem in fullPath)
                    {
                        path += "/" + pathItem;
                        idList.Add(GetId(path));
                        if (!tabu.Contains(path))
                        {
                            tabu.Add(path);
                            treeItems.Add(new Pair<ArrayList<int>, string>(idList.Clone(), path));
                        }
                    }
                }
            }
            treeItems.Sort(delegate(Pair<ArrayList<int>, string> a, Pair<ArrayList<int>, string> b) {
                int n = Math.Min(a.First.Count, b.First.Count);
                for (int i = 0; i < n; i++)
                {
                    if (a.First[i] != b.First[i])
                    {
                        return a.First[i].CompareTo(b.First[i]);
                    }
                }
                if (a.First.Count > b.First.Count) { return 1; }
                else if (b.First.Count > a.First.Count) { return -1; }
                else { return 0; }
            });
            idMapping.Clear();
            ArrayList<object> treeItemsParam = new ArrayList<object>();
            foreach (Pair<ArrayList<int>, string> item in treeItems)
            {
                ArrayList<string> pathItems = new ArrayList<string>(item.Second.Split('/'));
                treeItemsParam.Add(new object[] { pathItems.Count - 1, pathItems.Last, GetId(item.Second) });
            }
            foreach (Annotation a in d.Annotations)
            {
                string path = "";
                string[] fullPath = a.Type.Split('/', '\\');
                for (int i = 0; i < fullPath.Length; i++) 
                {
                    path += "/" + fullPath[i];
                    int id = GetId(path);
                    bool isLeaf = i == fullPath.Length - 1;
                    data.Add(new AnnotationInfo(a, id, /*isSpanStart=*/true, isLeaf));
                    data.Add(new AnnotationInfo(a, id, /*isSpanStart=*/false, isLeaf));
                }
            }
            data.Sort(delegate(AnnotationInfo a, AnnotationInfo b)
            {
                int c = a.mIdx.CompareTo(b.mIdx);
                if (c != 0) { return c; }
                return -a.mIsSpanStart.CompareTo(b.mIsSpanStart);
            });
            string text = d.Text;
            Dictionary<int, Set<Annotation>> state = new Dictionary<int, Set<Annotation>>();
            // AddToState
            Action<AnnotationInfo> AddToState = delegate(AnnotationInfo annotInfo) {
                Set<Annotation> annots;
                if (!state.TryGetValue(annotInfo.mAnnotationId, out annots))
                {
                    state.Add(annotInfo.mAnnotationId, annots = new Set<Annotation>());
                }
                if (annotInfo.mIsLeaf) { annots.Add(annotInfo.mAnnotation); }
            };
            // RemoveFromState
            Action<AnnotationInfo> RemoveFromState = delegate(AnnotationInfo annotInfo) {
                Set<Annotation> annots;
                if (state.TryGetValue(annotInfo.mAnnotationId, out annots))
                {
                    if (annotInfo.mIsLeaf) { annots.Remove(annotInfo.mAnnotation); }
                    if (annots.Count == 0)
                    {
                        state.Remove(annotInfo.mAnnotationId);
                    }
                }                
            };
            ArrayList<object> featuresParam = new ArrayList<object>();
            foreach (KeyValuePair<string, string> f in d.Features)
            {
                string val = ProcessDocumentFeatureValue(f.Key, f.Value);
                if (val != null)
                {
                    featuresParam.Add(new string[] { f.Key, val });
                }
            }
            int cIdx = 0;
            ArrayList<object> contentParam = new ArrayList<object>();
            foreach (AnnotationInfo item in data)
            {
                if (item.mIsSpanStart)
                {
                    string part = text.Substring(cIdx, item.mIdx - cIdx);
                    if (part != "") { contentParam.Add(new object[] { part, EncodeState(state) }); }
                    cIdx = item.mIdx;
                    AddToState(item);
                }
                else
                {
                    string part = text.Substring(cIdx, item.mIdx - cIdx + 1);
                    if (part != "") { contentParam.Add(new object[] { part, EncodeState(state) }); }
                    cIdx = item.mIdx + 1;
                    RemoveFromState(item);
                }
            }
            if (text.Length - cIdx > 0) 
            { 
                contentParam.Add(new object[] { text.Substring(cIdx, text.Length - cIdx), EncodeState(state) }); 
            }
            // fill ViewBag
            ViewBag.Title = d.Name;
            ViewBag.TreeItemsParam = treeItemsParam;
            ViewBag.FeaturesParam = featuresParam;
            ViewBag.ContentParam = contentParam;
            // render
            return View();
        }
    }
}
