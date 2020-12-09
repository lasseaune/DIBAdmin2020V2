using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Net;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using DIBAdminAPI.Helpers.Extentions;
using System.IO;
using HtmlAgilityPack;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Diagnostics.CodeAnalysis;

namespace DIBAdminAPI.Data.Entities
{
    public static class JsonElementActions
    {
        public static void SetOtherProps(this KeyValuePair<string, JsonElement> e, string key, dynamic value)
        {
            if (e.Value.otherprops== null)
            {
                e.Value.otherprops = new Dictionary<string, dynamic>();
            }
            if (e.Value.otherprops.ContainsKey(key))
            {
                e.Value.otherprops[key] = value;
                return;
            }
            e.Value.otherprops.Add(key, value);   
        }
    }
    public class JsonElementGroupNorn
    {
        public KeyValuePair<string, JsonElement> element { get; set; }
        public JsonChild child { get; set; }
        public KeyValuePair<string, JsonElement> childElement { get; set; }
    }
    public class JsonElementGroup
    {
        public JsonElementHierarcy element { get; set; }
        public JsonChild child { get; set; }
        public KeyValuePair<string, JsonElement>  childElement { get; set; }
    }
    public class JsonElementHierarcy
    {
        public Dictionary<string, string> attributes = new Dictionary<string, string>();
        public List<JsonChild> children { get; set; } = new List<JsonChild>();
        public string name { get; set; }

        public XNode GetChildren(JsonChild jsonChild)
        {
            if (jsonChild.build != null)
                return jsonChild.build.GetXML();
            else if (jsonChild.text != null)
                return new XText(jsonChild.text);
            return null;
        }
        public XElement GetXML()
        {
            return new XElement(name,
                attributes.Select(p => new XAttribute(p.Key, p.Value)),
                children.Select(p => GetChildren(p))
             );
        }
    }
    public class JsonHierarcy
    {
        public string id { get; set; }
        public string name { get; set; }
        public JsonChild text { get; set; }
        public JsonElement element { get; set; }
        public List<JsonHierarcy> elements { get; set; }

        private XNode GetC(JsonHierarcy c)
        {

            if (c.text != null)
                return new XText(c.text.text);
            else if (c.element != null)
                return c.GetXML();
            return null;
        }
        public XElement GetXML()
        {
            return new XElement(element.name,
                element.attributes.Select(p => new XAttribute(p.Key, p.Value)),
                elements.Select(p => GetC(p))
             );
        }
    }

    public class JsonXMLBuildElement
    {
        public string id { get; set; }
        public JsonElement element { get; set; }
        public List<JsonXMLBuildElement> elements { get; set; }
        private XNode GetC(JsonChild c)
        {
            if (c.id != null)
                return elements.Where(p => p.id == c.id).Select(p => p.GetXML()).FirstOrDefault();
            else if (c.text != null)
                return new XText(c.text);
            return null;
        }
        public XElement GetXML()
        {
            return new XElement(element.name,
                element.attributes.Select(p=>new XAttribute(p.Key, p.Value)),
                element.children.Select(p=>GetC(p))
             );
        }
    }
    
    public class BuildJson
    {
        public SubJsonElement element { get; set; }
        public List<SubJsonElement> children { get; set; }
    }
    public class SubJsonElement
    {
        public string id { get; set; }
        public JsonElement element { get; set; }
    }
    public class TocUpdate : Object
    {
        public KeyValuePair<string, JsonToc> oldToc { get; set; }
        public KeyValuePair<string, JsonToc> newToc { get; set; }
    }
    public class Heading
    {
        public XElement Element { get; set; }
        public int Level { get; set; }
        public Heading(XElement h)
        {
            Element = h;
            Level = Convert.ToInt32(Regex.Match(h.Name.LocalName, @"h(?<n>(\d+))").Groups["n"].Value);
        }
    }
    public class RowSpan
    {
        public string colId { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public int colspan { get; set; }
        public int colNo { get; set; }
    }
    public class TableEditObject
    {
        public TableActions actions { get; set; }
        public string start { get; set; }
        public string end { get; set; }

        public string startRowId { get; set; }
        public int startColNo { get; set; }
        public int startColSpan { get; set; }
        public string startColId { get; set; }

        public string endRowId { get; set; }
        public int endColNo { get; set; }
        public int endColSpan { get; set; }
        public string endColId { get; set; }
    }
    public class TableProduction
    {
        public XElement Table { get; set; }
        public XElement thead { get; set; }
        public XElement tbody { get; set; }
        public XElement tfoot { get; set; }
        private TableRows Rows { get; set; }
        private TableEditObject EditObject { get; set; }
        private class TableCell
        {
            public bool ActionStart { get; set; }
            public string Id { get; set; }
            public XElement Cell { get; set; }
            public int ColNo { get; set; }
            public int rowspan { get; set; }
            public int colspan { get; set; }
            public TableCell(XElement cell, int colNo)
            {
                Cell = cell;
                Id = (string)cell.Attributes("id").FirstOrDefault();
                ColNo = colNo;
                rowspan = ((string)cell.Attributes("rowspan").FirstOrDefault() ?? "1").Trim().IsNumeric();
                colspan = ((string)cell.Attributes("colspan").FirstOrDefault() ?? "1").Trim().IsNumeric();

            }
        }
        private class TableRowSpan
        {
            public XElement Cell { get; set; }
            public string StartRowId { get; set; }
            //public string EndRowId { get; set; }
            public string CellId { get; set; }
            public int ColNo { get; set; }
            public int Colspan { get; set; }
            public int RowSpan { get; set; }
            public int counter { get; set; }
            //public bool InSpan { get; set; }
            //public int spanCount { get; set; }
        }
        private class TableRow
        {
            public XElement Row { get; set; }
            public string Id { get; set; }
            public string ParentName { get; set; }
            public List<TableCell> Cells = new List<TableCell>();
            public List<TableRowSpan> RowSpans = new List<TableRowSpan>();
            public TableRow(TableProduction tp, XElement row, List<TableRowSpan> rowSpans)
            {
                Row = row;
                Id = (string)row.Attributes("id").FirstOrDefault();
                ParentName = row.Parent.Name.LocalName;
                int colNo = 1;
                foreach (XElement cell in row.Elements().Where(p => "th;td".Split(';').Contains(p.Name.LocalName)))
                {
                    if (rowSpans != null)
                    {
                        foreach (TableRowSpan rowspan in rowSpans)
                        {
                            if (colNo.Between(rowspan.ColNo, rowspan.ColNo + (rowspan.Colspan - 1), true))
                            {
                                colNo = rowspan.ColNo + (rowspan.Colspan - 1);
                                rowspan.counter++;
                                colNo++;
                            }
                        }
                    }
                    
                    TableCell tc = new TableCell(cell, colNo);
                    if (tp.EditObject!=null)
                    {
                        if (tp.EditObject.start == tc.Id)
                        {
                            if (tp.EditObject.startRowId == null)
                            {
                                tp.EditObject.startRowId = Id;
                                tp.EditObject.startColId = tc.Id;
                                tp.EditObject.startColNo = tc.ColNo;
                                tp.EditObject.startColSpan = tc.colspan;

                            }
                            else
                            {
                                tp.EditObject.endRowId = Id;
                                tp.EditObject.endColId = tc.Id;
                                tp.EditObject.endColNo = tc.ColNo;
                                tp.EditObject.endColSpan = tc.colspan;
                            }
                        }
                        else if (tp.EditObject.end == tc.Id)
                        {
                            if (tp.EditObject.startRowId == null)
                            {
                                tp.EditObject.startRowId = Id;
                                tp.EditObject.startColId = tc.Id;
                                tp.EditObject.startColNo = tc.ColNo;
                                tp.EditObject.startColSpan = tc.colspan;

                            }
                            else
                            {
                                tp.EditObject.endRowId = Id;
                                tp.EditObject.endColId = tc.Id;
                                tp.EditObject.endColNo = tc.ColNo;
                                tp.EditObject.endColSpan = tc.colspan;
                            }
                        }

                    }

                    if (tc.rowspan>1)
                    {
                        RowSpans.Add(new TableRowSpan
                        {
                            Cell = tc.Cell,
                            StartRowId = Id,
                            CellId = tc.Id,
                            Colspan = tc.colspan,
                            RowSpan = tc.rowspan,
                            ColNo = tc.ColNo,
                            counter = 1
                        });
                    }
                    Cells.Add(tc);
                    colNo = tc.ColNo + (tc.colspan == 0 ? 0 : tc.colspan - 1);
                    colNo++;
                }
            }
        }
        private class TableRows
        {
            public List<TableRow> Rows = new List<TableRow>();

            public TableRows(TableProduction tp)
            {
                if (tp.thead!=null)
                {
                    foreach (XElement tr in tp.thead.Elements("tr"))
                    {
                        List<TableRowSpan> RowSpans = Rows.SelectMany(p => p.RowSpans).Where(p => p.counter < p.RowSpan).Select(p => p).ToList();
                        TableRow tableRow = new TableRow(tp, tr, RowSpans);
                        Rows.Add(tableRow);
                    }
                }
                if (tp.tbody != null)
                {
                    foreach (XElement tr in tp.tbody.Elements("tr"))
                    {
                        List<TableRowSpan> RowSpans = Rows.SelectMany(p => p.RowSpans).Where(p => p.counter < p.RowSpan).Select(p => p).ToList();
                        TableRow tableRow = new TableRow(tp, tr, RowSpans);
                        Rows.Add(tableRow);
                    }
                }
                if (tp.tfoot != null)
                {
                    foreach (XElement tr in tp.tfoot.Elements("tr"))
                    {
                        List<TableRowSpan> RowSpans = Rows.SelectMany(p => p.RowSpans).Where(p => p.counter < p.RowSpan).Select(p => p).ToList();
                        TableRow tableRow = new TableRow(tp, tr, RowSpans);
                        Rows.Add(tableRow);
                    }
                }

                int MaxCol = Rows.SelectMany(p => p.Cells).Max(p => p.ColNo + (p.colspan - 1));

                switch (tp.EditObject.actions)
                {
                    case TableActions.InsertColumnBefore:
                    case TableActions.InsertColumnAfter:
                        TableInsertColumn(tp);
                        //TableDeleteRow(tp);
                        break;
                    case TableActions.InsertRowBefore:
                    case TableActions.InsertRowAfter:
                        TableInsertRow(tp);
                        break;
                    case TableActions.Span:
                        TableSpan(tp);
                        break;
                    case TableActions.Unspan:
                        TableSpanCollapse(tp);
                        break;
                    case TableActions.DeleteRow:
                        TableDeleteRow(tp);
                        break;
                    case TableActions.DeleteColumn:
                        TableDeleteColumn(tp);
                        break;
                        
                }
            }
            public class FormatedSpan
            {
                public List<XElement> NewCells = null;
                public List<TableRowSpan> NewRowSpans = new List<TableRowSpan>();
                public int ColNo = 0;
            }
            public FormatedSpan FormatCell(TableCell tc, TableRow tr, int colNo, int startCol, int endCol)
            {
                List<XElement> newCells = new List<XElement>();
                List<TableRowSpan> newRowSpans = new List<TableRowSpan>();
                if (tc.ColNo.Between(startCol,endCol ,true) || (tc.ColNo + (tc.colspan - 1)).Between(startCol, endCol,true))
                {
                    int beforeStart = 0;
                    int insideStart = 0;
                    int afterStart = 0;
                    int before = 0;
                    int inside = 0;
                    int after = 0;
                    for (int i = tc.ColNo; i <= tc.ColNo + (tc.colspan - 1); i++)
                    {
                        if (i < startCol)
                        {
                            if (beforeStart == 0) beforeStart = i;
                            before = before + 1;
                        }
                        else if (i >= startCol && i <= endCol)
                        {
                            if (insideStart == 0) insideStart = i;
                            inside = inside + 1;
                        }
                        else if (i > endCol)
                        {
                            if (afterStart == 0) afterStart = i;
                            after = after + 1;
                        }
                    }
                    if (before > 0)
                    {
                        newCells.Add(new XElement("td",
                                new XAttribute("id", Guid.NewGuid().ToString()),
                                new XAttribute("colNo", beforeStart.ToString()),
                                before > 1 ? new XAttribute("colspan", before.ToString()) : null,
                                tc.rowspan>1 ? new XAttribute("rowspan", tc.rowspan.ToString()) : null,
                                tc.Cell.Nodes()
                            )
                        );
                    }
                    if (inside > 0)
                    {
                        newCells.Add(new XElement("td",
                                new XAttribute("id", Guid.NewGuid().ToString()),
                                new XAttribute("currentspan", "true"),
                                new XAttribute("colNo", insideStart.ToString()),
                                inside > 1 ? new XAttribute("colspan", inside.ToString()) : null,
                                tc.rowspan > 1 ? new XAttribute("rowspan", tc.rowspan.ToString()) : null,
                                before == 0 ? tc.Cell.Nodes() : new XElement("td", new XText("")).Nodes()
                            )
                        );
                    }

                    if (after > 0)
                    {
                        newCells.Add(
                            new XElement("td",
                                new XAttribute("id", Guid.NewGuid().ToString()),
                                new XAttribute("colNo", afterStart.ToString()),
                                after > 1 ? new XAttribute("colspan", after.ToString()) : null,
                                tc.rowspan > 1 ? new XAttribute("rowspan", tc.rowspan.ToString()) : null,
                                before == 0 && inside == 0 ? tc.Cell.Nodes() : new XElement("td", new XText("")).Nodes()
                            )
                        );
                    }
                    if (newCells.Count() > 0 && tc.rowspan > 1)
                    {
                        foreach (XElement td in newCells)
                        {
                            newRowSpans.Add(new TableRowSpan
                            {
                                Cell = td,
                                StartRowId = tr.Id,
                                CellId = (string)td.Attributes("id").FirstOrDefault(),
                                ColNo = Convert.ToInt32((string)td.Attributes("colNo").FirstOrDefault()),
                                Colspan = (string)td.Attributes("colspan").FirstOrDefault() == null ? 1 : Convert.ToInt32((string)td.Attributes("colspan").FirstOrDefault()),
                                RowSpan = tc.rowspan,
                                counter = 1
                            });
                        }

                    }
                    colNo = tc.ColNo + (tc.colspan - 1);
                    colNo++;
                    return new FormatedSpan
                    {
                        NewCells = newCells,
                        ColNo = colNo,
                        NewRowSpans = newRowSpans

                    };
                }
                else
                {
                    newCells.Add(tc.Cell);
                    colNo = tc.ColNo + (tc.colspan - 1);
                    colNo++;
                    return new FormatedSpan
                    {
                        NewCells = newCells,
                        ColNo = colNo,
                        NewRowSpans = newRowSpans
                    };
                }
                
            }
            public FormatedSpan FormatSpan(ref List<TableRowSpan> rsBefore, TableRow tr, int colNo, int startCol, int endCol, bool BreakRowspan, TableProduction tp)
            {
                List<TableRowSpan> newRowSpans = new List<TableRowSpan>();
                List<XElement> newCells = new List<XElement>();
                TableRowSpan trs = rsBefore.Where(p => colNo.Between(p.ColNo, p.ColNo + (p.Colspan - 1), true)).FirstOrDefault();
                while (trs != null)
                {
                    if (BreakRowspan)
                    {
                        int rowSpan = 0;
                        if (trs.ColNo.Between(startCol, endCol,true) || (trs.ColNo + (trs.Colspan - 1)).Between(startCol, endCol, true))
                        {
                            int beforeStart = 0;
                            int insideStart = 0;
                            int afterStart = 0;

                            int before = 0;
                            int inside = 0;
                            int after = 0;
                            for (int i = colNo; i <= trs.ColNo + (trs.Colspan - 1); i++)
                            {
                                if (i < startCol)
                                {
                                    if (beforeStart == 0) beforeStart = i;
                                    before = before + 1;
                                }
                                else if (i >= startCol && i <= endCol)
                                {
                                    if (insideStart == 0) insideStart = i;
                                    inside = inside + 1;
                                }
                                else if (i > endCol)
                                {
                                    if (afterStart == 0) afterStart = i;
                                    after = after + 1;
                                }
                            }
                            rowSpan = trs.RowSpan - trs.counter;
                            if (trs.counter > 1)
                            {
                                tp.Table
                                    .Descendants()
                                    .Where(p => (string)p.Attributes("id").FirstOrDefault() == trs.CellId)
                                    .ToList()
                                    .ForEach(p => p.SetAttributeValue("rowspan", trs.counter));
                            }
                            else
                            {
                                tp.Table.Descendants().Where(p => (string)p.Attributes("id").FirstOrDefault() == trs.CellId).Attributes("rowspan").Remove();
                            }
                            
                            trs.RowSpan = trs.counter;

                            if (before > 0)
                            {
                                newCells.Add(new XElement("td",
                                        new XAttribute("id", Guid.NewGuid().ToString()),
                                        new XAttribute("colNo", beforeStart.ToString()),
                                        before > 1 ? new XAttribute("colspan", before.ToString()) : null,
                                        rowSpan > 1 ? new XAttribute("rowspan", rowSpan.ToString()) : null,
                                        new XText("")
                                    )
                                );
                            }
                            if (inside > 0)
                            {
                                newCells.Add(new XElement("td",
                                        new XAttribute("id", Guid.NewGuid().ToString()),
                                        new XAttribute("colNo", insideStart.ToString()),
                                        inside > 1 ? new XAttribute("colspan", inside.ToString()) : null,
                                        rowSpan > 1 ? new XAttribute("rowspan", rowSpan.ToString()) : null,
                                        new XAttribute("currentspan", "true"),
                                        new XText("")
                                    )
                                );
                            }

                            if (after > 0)
                                newCells.Add(
                                    new XElement("td",
                                        new XAttribute("id", Guid.NewGuid().ToString()),
                                        new XAttribute("colNo", afterStart.ToString()),
                                        after > 1 ? new XAttribute("colspan", after.ToString()) : null,
                                        rowSpan > 1 ? new XAttribute("rowspan", rowSpan.ToString()) : null,
                                        new XText(""))
                                );
                        }
                        if (newCells.Count()>0 && rowSpan > 1)
                        {
                            foreach (XElement td in newCells)
                            {
                                newRowSpans.Add(new TableRowSpan
                                {
                                    Cell = td,
                                    StartRowId = tr.Id,
                                    CellId = (string)td.Attributes("id").FirstOrDefault(),
                                    ColNo = Convert.ToInt32((string)td.Attributes("colNo").FirstOrDefault()),
                                    Colspan = (string)td.Attributes("colspan").FirstOrDefault() == null ? 1 : Convert.ToInt32((string)td.Attributes("colspan").FirstOrDefault()),
                                    RowSpan = Convert.ToInt32(rowSpan),
                                    counter = 1
                                });
                            }

                        }
                    }
                    colNo = trs.ColNo + (trs.Colspan - 1);
                    colNo++;
                    trs = rsBefore.Where(p => colNo.Between(p.ColNo, p.ColNo + (p.Colspan - 1), true)).FirstOrDefault();
                }
                return new FormatedSpan
                {
                    NewCells = newCells,
                    ColNo = colNo,
                    NewRowSpans = newRowSpans

                };
            }
            public void TableSpan(TableProduction tp)
            {
                TableCell start = null;// Rows.SelectMany(p => p.Cells).Select(p => p).Where(p => p.Id == tp.EditObject.startColId).FirstOrDefault();
                TableCell end = null;// Rows.SelectMany(p => p.Cells).Select(p => p).Where(p => p.Id == tp.EditObject.endColId).FirstOrDefault();
                int rowCount = 1;
                //tell opp rows i span
                List<int> lRowCount = new List<int>();

                for (int i = 0; i < Rows.Count; i++)
                {
                    TableRow tr = Rows.ElementAt(i);
                    foreach (TableCell tc in tr.Cells)
                    {
                        if (start == null && (tp.EditObject.startColId == tc.Id || tp.EditObject.endColId == tc.Id))
                        {
                            start = tc;
                            lRowCount.Add(rowCount);
                            lRowCount.Add(rowCount + (tc.rowspan - 1));
                        }
                        else if (start != null && (tp.EditObject.startColId == tc.Id || tp.EditObject.endColId == tc.Id))
                        {
                            end = tc;
                            lRowCount.Add(rowCount);
                            lRowCount.Add(rowCount + (tc.rowspan - 1));
                        }
                        if (start != null && end != null)
                            break;
                    }
                    if (start != null) rowCount++;

                    if (start != null && end != null)
                        break;

                }
                rowCount = lRowCount.Max();


                if (start == null) return;
                
                if (end == null) return;
                if (start == end) return;

                List<int> spanCol = new List<int>();
                spanCol.Add(start.ColNo);
                spanCol.Add(start.ColNo + (start.colspan - 1));
                spanCol.Add(end.ColNo);
                spanCol.Add(end.ColNo + (end.colspan - 1));

                int startCol = spanCol.Min();
                int endCol = spanCol.Max();
                //if (start.ColNo > end.ColNo)
                //{
                //    startCol = end.ColNo;
                //    endCol = start.ColNo;
                //}
                //else
                //{
                //    startCol = start.ColNo;
                //    endCol = end.ColNo;
                //}
                string startId = (string)start.Cell.Parent.Attributes("id").FirstOrDefault();
                string endId = (string)end.Cell.Parent.Attributes("id").FirstOrDefault();

                if (start.Cell.Parent.Parent != end.Cell.Parent.Parent) return;

                bool take = false;
                bool stop = false;
                bool bstart = false;
                
                TableRow next = null;
                TableRow previouse = null;
                string spanId = null;
                XElement newRow = null;
                bool dropCell;
                List<XElement> spanContent = new List<XElement>();

                //behandle rader
                int iTaken = 0;
                List<TableRowSpan> rs = new List<TableRowSpan>();
                for (int i = 0; i < Rows.Count; i++)
                {
                    newRow = new XElement("newrow");
                    TableRow tr = Rows.ElementAt(i);
                    next = null;
                    //bLastBefore = false;
                    List<TableRowSpan> rsBefore = rs.Where(p => p.counter < p.RowSpan).ToList();
                    bool BreakRowspan = false;

                    if ((i + 1) < Rows.Count) next = Rows.ElementAt(i + 1);
                    
                    //if (next == null ? false : next.Id == startId) bLastBefore = true;

                    if (tr.Id == startId)
                    {
                        bstart = true;
                        take = true;
                        BreakRowspan = true;
                        
                    }
                    if (take) iTaken++;

                    //if (previouse==null ? false : previouse.Id==endId)
                    if (iTaken > rowCount)
                    {
                        stop = true;
                        BreakRowspan = true;
                    }
                    
                    if (take)
                    {
                        List<TableRowSpan> newRowSpan = new List<TableRowSpan>();
                        int colNo = 1;
                        FormatedSpan fs = FormatSpan(ref rsBefore, tr, colNo, startCol, endCol, BreakRowspan, tp);
                        if (fs.NewCells.Count() > 0)
                        {
                            if (fs.NewCells.Where(p => (string)p.Attributes("currentspan").FirstOrDefault() == "true").Count() == 0)
                            {
                                colNo = fs.ColNo;
                            }
                            else
                            { 
                                foreach (XElement td in fs.NewCells.Select(p => p))
                                {
                                    dropCell = false;
                                    if (!stop)
                                    {
                                        if ((string)td.Attributes("currentspan").FirstOrDefault() == "true")
                                        {
                                            if (bstart && spanId == null)
                                            {
                                                spanId = (string)td.Attributes("id").FirstOrDefault();
                                                if ((endCol - startCol) >= 1)
                                                {
                                                    td.SetAttributeValueEx("colspan", ((endCol - startCol) + 1).ToString());
                                                }

                                                if (rowCount < 1)
                                                {
                                                    td.Attributes("rowspan").Remove();
                                                }
                                                else
                                                {
                                                    td.SetAttributeValueEx("rowspan", rowCount.ToString());
                                                }
                                                spanContent.AddRange(td.GetCellNodes());
                                            }
                                            else
                                            {
                                                dropCell = true;
                                                spanContent.AddRange(td.GetCellNodes());
                                            };
                                        }
                                    }
                                    if (!dropCell)
                                    {
                                        newRow.Add(
                                            new XElement(td.Name.LocalName,
                                                td.Attributes("id"),
                                                td.Attributes("rowspan"),
                                                td.Attributes("colspan"),
                                                td.Nodes()
                                            )
                                        );
                                    }
                                }
                                colNo = fs.ColNo;
                                if (fs.NewRowSpans.Count() > 0)
                                {
                                    newRowSpan = fs.NewRowSpans;
                                }
                            }
                        }
                        foreach (TableCell tc in tr.Cells)
                        {
                            fs = FormatCell(tc, tr, colNo, startCol, endCol);
                            if (fs.NewCells.Count() > 0)
                            {
                                foreach (XElement td in fs.NewCells.Select(p => p))
                                {
                                    dropCell = false;
                                    if (!stop)
                                    {
                                        if ((string)td.Attributes("currentspan").FirstOrDefault() == "true")
                                        {
                                            if (bstart && spanId == null)
                                            {
                                                spanId = (string)td.Attributes("id").FirstOrDefault();
                                                if ((endCol - startCol) >= 1)
                                                {
                                                    td.SetAttributeValueEx("colspan", ((endCol - startCol) + 1).ToString());
                                                }
                                                if (rowCount <= 1)
                                                {
                                                    td.Attributes("rowspan").Remove();
                                                }
                                                else
                                                {
                                                    td.SetAttributeValueEx("rowspan", rowCount.ToString());
                                                }
                                                spanContent.AddRange(td.GetCellNodes());
                                            }
                                            else
                                            {
                                                dropCell = true;
                                                spanContent.AddRange(td.GetCellNodes());

                                            }
                                        }
                                    }
                                    
                                    if (!dropCell)
                                    {
                                        newRow.Add(
                                            new XElement(td.Name.LocalName,
                                                td.Attributes("id"),
                                                td.Attributes("rowspan"),
                                                td.Attributes("colspan"),
                                                td.Nodes()
                                            )
                                        );

                                    }
                                    colNo = fs.ColNo;
                                }
                                if (fs.NewRowSpans.Count()>0)
                                {
                                    tr.RowSpans = tr.RowSpans.Where(p => p.CellId != tc.Id).ToList();
                                    tr.RowSpans.AddRange(fs.NewRowSpans);
                                }
                            }
                            fs = FormatSpan(ref rsBefore, tr, colNo, startCol, endCol, BreakRowspan, tp);
                            if (fs.NewCells.Count() > 0)
                            {
                                if (fs.NewCells.Where(p => (string)p.Attributes("currentspan").FirstOrDefault() == "true").Count() == 0)
                                {
                                    colNo = fs.ColNo;
                                }
                                else
                                {
                                    foreach (XElement td in fs.NewCells.Select(p => p))
                                    {
                                        dropCell = false;
                                        if (!stop)
                                        {
                                            if ((string)td.Attributes("currentspan").FirstOrDefault() == "true")
                                            {
                                                if (bstart && spanId == null)
                                                {
                                                    spanId = (string)td.Attributes("id").FirstOrDefault();
                                                    if ((endCol - startCol) >= 1)
                                                    {
                                                        td.SetAttributeValueEx("colspan", ((endCol - startCol) + 1).ToString());
                                                    }
                                                    if (rowCount < 1)
                                                    {
                                                        td.Attributes("rowspan").Remove();
                                                    }
                                                    else
                                                    {
                                                        td.SetAttributeValueEx("rowspan", rowCount.ToString());
                                                    }
                                                }
                                                else
                                                {
                                                    dropCell = true;
                                                    spanContent.AddRange(td.GetCellNodes());
                                                };
                                            }
                                        }

                                        if (!dropCell)
                                        {
                                            newRow.Add(
                                                new XElement(td.Name.LocalName,
                                                    td.Attributes("id"),
                                                    td.Attributes("rowspan"),
                                                    td.Attributes("colspan"),
                                                    td.Nodes()
                                                )
                                            );
                                        }
                                    }
                                    colNo = fs.ColNo;
                                    if (fs.NewRowSpans.Count() > 0)
                                    {
                                        newRowSpan = fs.NewRowSpans;
                                    }
                                }
                            }
                        }
                        tr.Row.ReplaceWith(new XElement(tr.Row.Name.LocalName, tr.Row.Attributes(), newRow.Nodes()));
                        rsBefore.ForEach(p => p.counter++);
                        rs.AddRange(rsBefore);
                        rs.AddRange(newRowSpan);
                        if (stop)
                        {
                            XElement spancell = tp.Table.Descendants().Where(p => (string)p.Attributes("id").FirstOrDefault() == spanId).FirstOrDefault();
                            if (spancell!=null)
                            {
                                spancell.Nodes().ToList().ForEach(p => p.Remove());
                                spancell.Add(spanContent);
                            }
                            return;
                        }
                    }
                    else
                    {
                        rsBefore.ForEach(p => p.counter++);
                        List<TableRowSpan> temp = tr.RowSpans.Select(p => p).ToList();
                        temp.ForEach(p => p.counter = 1);
                        rs.AddRange(temp);
                    }
                    
                    previouse = tr;
                }
            }
            public List<XElement> GetCells(int start, int end)
            {
                List<XElement> cells = new List<XElement>();
                for (int i = start; i <= end; i++)
                {
                    cells.Add(new XElement("td", new XAttribute("id", Guid.NewGuid().ToString()), new XText("")));
                }
                return cells;
            }
            public void TableSpanCollapse(TableProduction tp)
            {
                List<TableRowSpan> rs = new List<TableRowSpan>();
                TableCell start = Rows.SelectMany(p => p.Cells).Select(p => p).Where(p => p.Id == tp.EditObject.startColId).FirstOrDefault();
                if (start == null)
                {
                    return;
                }
                string RowId = (string)start.Cell.Parent.Attributes("id").FirstOrDefault();
                
                int rowCount = 0;
                int colspan = 1;
                int StartCol = 1;
                int EndCol = 1;

                foreach (TableRow tr in Rows)
                {
                    if (tr.Id == RowId)
                    {
                        StartCol = start.ColNo;
                        EndCol = start.ColNo + (start.colspan - 1);
                        colspan = start.colspan;
                        rowCount = start.rowspan;
                    }
                    if (rowCount>0)
                    {
                        bool bAddBefore = false;
                        
                        bool bReplace = false;
                       

                       
                        foreach (TableCell tc in tr.Cells)
                        {
                            
                            if (tc.Id == start.Id)
                            {
                                bReplace = true;
                                tc.Cell.ReplaceWith(GetCells(StartCol, EndCol));
                                break;
                            }
                            else if (tc.ColNo > EndCol)
                            {
                                bAddBefore = true;
                                tc.Cell.AddBeforeSelf(GetCells(StartCol, EndCol));
                                break;
                            }
                            
                            
                        }
                        if (!(bAddBefore || bReplace))
                        {
                            tr.Row.Add(GetCells(StartCol, EndCol));
                        }
                            


                        rowCount = rowCount - 1;
                        if (rowCount == 0)
                            break;


                    }
                }
            }
            public void TableDeleteRow(TableProduction tp)
            {
                List<TableRowSpan> rs = new List<TableRowSpan>();
                TableCell start = Rows.SelectMany(p => p.Cells).Select(p => p).Where(p => p.Id == tp.EditObject.startColId).FirstOrDefault();
                if (start == null)
                {
                    return;
                }
                string RowId = (string)start.Cell.Parent.Attributes("id").FirstOrDefault();
                TableRow DeleteRow = null;
                foreach (TableRow tr in Rows)
                {
                    List<TableRowSpan> rsBefore = rs.Where(p => p.counter < p.RowSpan).ToList();
                    if (DeleteRow != null)
                    {
                        int colNo = 1;

                        
                        TableRowSpan trs = rsBefore.Where(p => colNo.Between(p.ColNo, p.ColNo + (p.Colspan - 1), true)).FirstOrDefault();
                        List<XElement> newCell = new List<XElement>();
                        while (trs != null)
                        {
                            if (trs.counter == 1)
                            {
                                if (trs.RowSpan == 2)
                                {
                                    newCell.Add(new XElement(trs.Cell.Name, 
                                        trs.Cell.Attributes().Where(p => !"id;rowspan".Split(';').Contains(p.Name.LocalName)),
                                        new XAttribute("id", Guid.NewGuid().ToString()),
                                        trs.Cell.Nodes()));
                                }
                                else if (trs.RowSpan > 2)
                                {
                                    newCell.Add(new XElement(trs.Cell.Name,
                                        trs.Cell.Attributes().Where(p => !"id;rowspan".Split(';').Contains(p.Name.LocalName)),
                                        new XAttribute("id", Guid.NewGuid().ToString()),
                                        new XAttribute("rowspan", (trs.RowSpan - 1).ToString()),
                                        trs.Cell.Nodes()));
                                }
                            }
                            
                            colNo = trs.ColNo + (trs.Colspan - 1);
                            colNo++;
                            trs = rsBefore.Where(p => colNo.Between(p.ColNo, p.ColNo + (p.Colspan - 1), true)).FirstOrDefault();
                        }
                        foreach (TableCell tc in tr.Cells)
                        {
                            newCell.Add(tc.Cell);
                            colNo = tc.ColNo + (tc.colspan - 1);
                            colNo++;
                            trs = rsBefore.Where(p => colNo.Between(p.ColNo, p.ColNo + (p.Colspan - 1), true)).FirstOrDefault();
                            while (trs != null)
                            {
                                if (trs.counter == 1)
                                {
                                    if (trs.RowSpan == 2)
                                    {
                                        newCell.Add(new XElement(trs.Cell.Name,
                                            trs.Cell.Attributes().Where(p => !"id;rowspan".Split(';').Contains(p.Name.LocalName)),
                                            new XAttribute("id", Guid.NewGuid().ToString()),
                                            trs.Cell.Nodes()));
                                    }
                                    else if (trs.RowSpan > 2)
                                    {
                                        newCell.Add(new XElement(trs.Cell.Name,
                                            trs.Cell.Attributes().Where(p => !"id;rowspan".Split(';').Contains(p.Name.LocalName)),
                                            new XAttribute("id", Guid.NewGuid().ToString()),
                                            new XAttribute("rowspan", (trs.RowSpan - 1).ToString()),
                                            trs.Cell.Nodes()));
                                    }
                                }
                                colNo = trs.ColNo + (trs.Colspan - 1);
                                colNo++;
                                trs = rsBefore.Where(p => colNo.Between(p.ColNo, p.ColNo + (p.Colspan - 1), true)).FirstOrDefault();
                            }
                        }
                        if (newCell.Count()>0)
                        {
                            tp.Table.Descendants().Where(p => (string)p.Attributes("id").FirstOrDefault() == DeleteRow.Id).ToList().ForEach(p => p.Remove());
                            tr.Row.Nodes().Remove();
                            tr.Row.Add(newCell);
                            return;
                        }
                    }
                    if (tr.Id == RowId)
                    {
                        foreach (TableRowSpan trs in rsBefore)
                        {
                            if (trs.RowSpan == 2)
                            {
                                trs.Cell.Attributes("rowspan").Remove();
                            }
                            else if (trs.RowSpan > 2)
                            {
                                trs.Cell.SetAttributeValueEx("rowspan", (trs.RowSpan - 1).ToString());
                            }
                        }
                        DeleteRow = tr;
                    }
                    rs.ForEach(p => p.counter++);
                    List<TableRowSpan> temp = tr.RowSpans.Select(p => p).ToList();
                    temp.ForEach(p => p.counter = 1);
                    rs.AddRange(temp);
                }
                if (DeleteRow.Row != null)
                {
                    tp.Table.Descendants().Where(p => (string)p.Attributes("id").FirstOrDefault() == DeleteRow.Id).ToList().ForEach(p => p.Remove());
                    return;
                }
            }
            public void TableInsertRow(TableProduction tp)
            {
                List<TableRowSpan> rs = new List<TableRowSpan>();
                TableCell start = Rows.SelectMany(p => p.Cells).Select(p => p).Where(p => p.Id == tp.EditObject.startColId).FirstOrDefault();
                if (start==null)
                {
                    return; 
                }
                string RowId = (string)start.Cell.Parent.Attributes("id").FirstOrDefault();
                XElement newRow = null;
                foreach (TableRow tr in Rows)
                {
                    if (tr.Id == RowId)
                    {
                        newRow = new XElement(tr.Row.Name.LocalName, new XAttribute("id", Guid.NewGuid().ToString()));
                    }
                    List<TableRowSpan> rsBefore = rs.Where(p => p.counter < (p.RowSpan - 1)).ToList();
                    if (newRow != null)
                    {
                        int colNo = 1;
                        TableRowSpan trs = rsBefore.Where(p => colNo.Between(p.ColNo, p.ColNo + (p.Colspan - 1), true)).FirstOrDefault();
                        while (trs != null)
                        {
                            if (tp.EditObject.actions == TableActions.InsertRowBefore)
                            {

                                trs.Cell.SetAttributeValueEx("rowspan", (trs.RowSpan + 1).ToString());
                                colNo = trs.ColNo + (trs.Colspan - 1);
                                colNo++;
                            }
                            else if (tp.EditObject.actions == TableActions.InsertRowAfter)
                            {
                                if (trs.counter == trs.RowSpan)
                                {
                                    for (int i = 1; i <= trs.Colspan; i++)
                                    {
                                        newRow.Add(new XElement(trs.Cell.Name.LocalName, new XAttribute("id", Guid.NewGuid().ToString()), new XText("")));
                                    }
                                }
                                else
                                {
                                    trs.Cell.SetAttributeValueEx("rowspan", (trs.RowSpan + 1).ToString());
                                }
                                colNo = trs.ColNo + (trs.Colspan - 1);
                                colNo++;
                            }
                            trs = rsBefore.Where(p => colNo.Between(p.ColNo, p.ColNo + (p.Colspan - 1), true)).FirstOrDefault();
                        }
                        foreach (TableCell tc in tr.Cells)
                        {
                            if (tp.EditObject.actions == TableActions.InsertRowBefore)
                            {
                                for (int i = 1; i <= tc.colspan; i++)
                                {
                                    newRow.Add(new XElement(tc.Cell.Name.LocalName, new XAttribute("id", Guid.NewGuid().ToString()), new XText("")));
                                }
                            }
                            else if (tp.EditObject.actions == TableActions.InsertRowAfter)
                            {
                                if (tc.rowspan >1)
                                {
                                    tc.Cell.SetAttributeValue("rowspan", (tc.rowspan + 1).ToString());
                                }
                                else
                                {
                                    for (int i = 1; i <= tc.colspan; i++)
                                    {
                                        newRow.Add(new XElement(tc.Cell.Name.LocalName, new XAttribute("id", Guid.NewGuid().ToString()), new XText("")));
                                    }
                                }
                                    
                            }
                            colNo = colNo + (tc.colspan - 1);
                            colNo++;

                            trs = rsBefore.Where(p => colNo.Between(p.ColNo, p.ColNo + (p.Colspan - 1), true)).FirstOrDefault();
                            while (trs != null)
                            {
                                if (tp.EditObject.actions == TableActions.InsertRowBefore)
                                {
                                    trs.Cell.SetAttributeValueEx("rowspan", (trs.RowSpan + 1).ToString());
                                    colNo = trs.ColNo + (trs.Colspan - 1);
                                    colNo++;
                                }
                                else if (tp.EditObject.actions == TableActions.InsertRowAfter)
                                {
                                    if (trs.counter == trs.RowSpan)
                                    {
                                        for (int i = 1; i <= trs.Colspan; i++)
                                        {
                                            newRow.Add(new XElement(trs.Cell.Name.LocalName, new XAttribute("id", Guid.NewGuid().ToString()), new XText("")));
                                        }
                                    }
                                    else
                                    {
                                        trs.Cell.SetAttributeValueEx("rowspan", (trs.RowSpan + 1).ToString());
                                    }
                                    colNo = trs.ColNo + (trs.Colspan - 1);
                                    colNo++;
                                }
                                trs = rsBefore.Where(p => colNo.Between(p.ColNo, p.ColNo + (p.Colspan - 1), true)).FirstOrDefault();
                            }
                        }
                        if (newRow != null)
                        {
                            if (tp.EditObject.actions == TableActions.InsertRowBefore)
                            {
                                tr.Row.AddBeforeSelf(newRow);
                                return;
                            }
                            else if (tp.EditObject.actions == TableActions.InsertRowAfter)
                            {
                                tr.Row.AddAfterSelf(newRow);
                                return;
                            }
                            else
                            {
                                return;
                            }
                        }

                    }
                    
                    rsBefore.ForEach(p => p.counter++);
                    List<TableRowSpan> temp = tr.RowSpans.Select(p => p).ToList();
                    temp.ForEach(p => p.counter = 1);
                    rs.AddRange(temp);
                }
            }
            public void TableInsertColumn(TableProduction tp)
            {
                List<TableRowSpan> rs = new List<TableRowSpan>();
                TableCell start = Rows.SelectMany(p=>p.Cells).Select(p=>p).Where(p => p.Id == tp.EditObject.startColId).FirstOrDefault();
                int InsertColNo = 1;
                switch (tp.EditObject.actions)
                {
                    case TableActions.InsertColumnBefore:
                        InsertColNo = start.ColNo;
                        break;
                    case TableActions.InsertColumnAfter:
                        InsertColNo = start.ColNo + (start.colspan - 1);
                        break;
                    default: return;
                }
                foreach (TableRow tr in Rows)
                {
                    int colNo = 1;
                    foreach (TableCell tc in tr.Cells)
                    {
                        colNo = tc.ColNo;
                        if (colNo > InsertColNo)
                        {
                            break;
                        }
                        if (InsertColNo.Between(tc.ColNo, tc.ColNo + (tc.colspan - 1), true))
                        {
                            if (colNo == InsertColNo && tp.EditObject.actions == TableActions.InsertColumnBefore)
                            {
                                tc.Cell.AddBeforeSelf(new XElement(tc.Cell.Name.LocalName, new XAttribute("id", Guid.NewGuid().ToString()), new XText("")));
                                break;
                            }
                            else if (colNo + (tc.colspan - 1) == InsertColNo && tp.EditObject.actions == TableActions.InsertColumnAfter)
                            {
                                tc.Cell.AddAfterSelf(new XElement(tc.Cell.Name.LocalName, new XAttribute("id", Guid.NewGuid().ToString()), new XText("")));
                                break;
                            }
                            else
                            {
                                tc.Cell.SetAttributeValueEx("colspan", (tc.colspan + 1).ToString());
                                break;
                            }
                        }
                        colNo = colNo + (tc.colspan - 1);
                        colNo++;
                    }
                }
            }
            public void TableDeleteColumn(TableProduction tp)
            {
                List<TableRowSpan> rs = new List<TableRowSpan>();
                TableCell start = Rows.SelectMany(p => p.Cells).Select(p => p).Where(p => p.Id == tp.EditObject.startColId).FirstOrDefault();
                if (start == null) return;
                int iStart = start.ColNo;
                int iEnd = start.ColNo + (start.colspan - 1);
                int iCols = (iEnd - iStart) + 1;


                foreach (TableRow tr in Rows)
                {
                    foreach (TableCell tc in tr.Cells)
                    {
                        int beforeStart = 0;
                        int insideStart = 0;
                        int afterStart = 0;
                        int before = 0;
                        int inside = 0;
                        int after = 0;
                        for (int i = tc.ColNo; i <= tc.ColNo + (tc.colspan - 1); i++)
                        {
                            if (i < iStart)
                            {
                                if (beforeStart == 0) beforeStart = i;
                                before = before + 1;
                            }
                            else if (i >= iStart && i <= iEnd)
                            {
                                if (insideStart == 0) insideStart = i;
                                inside = inside + 1;
                            }
                            else if (i > iEnd)
                            {
                                if (afterStart == 0) afterStart = i;
                                after = after + 1;
                            }
                        }
                        
                        if (inside > 0)
                        {
                            if (inside - (before + after) == (iEnd-iStart)+ 1)
                            {
                                tc.Cell.Remove();
                            }
                            else if ((before + after) ==1)
                            {
                                tc.Cell.Attributes("colspan").Remove();
                            }
                            else if ((before + after) > 1)
                            {
                                tc.Cell.SetAttributeValueEx("colspan", (before + after).ToString());
                            }
                        }
                        if ((tc.ColNo + (tc.colspan - 1)) > iEnd)
                            break;
                        
                    }
                }
            }
        }
        
        public TableProduction(XElement table, TableEditObject editObject)
        {
            EditObject = editObject;
            Table = table;
            thead = table.Elements("thead").FirstOrDefault();
            tbody = table.Elements("tbody").FirstOrDefault();
            tfoot = table.Elements("tfoot").FirstOrDefault();
            if (thead == null && tbody == null && tfoot == null && table.Elements("tr").Count() > 0)
            {
                table.Add(new XElement("tbody", table.Elements("tr")));
                table.Elements("tr").ToList().ForEach(p => p.Remove());
                tbody = table.Elements("tbody").FirstOrDefault();
            }
            TableRows tableRows = new TableRows(this);


        }
    }

    public enum TableActions
    {
        None = -1,
        InsertRowAfter = 0,
        InsertRowBefore = 1,
        InsertColumnAfter = 2,
        InsertColumnBefore = 3,
        Span = 4,
        Unspan = 5,
        DeleteRow = 6,
        DeleteColumn = 7
    }


    public class EditDocumentContainerResult
    {
        public JsonObject json { get; set; }
        public DocumentContainer documentContainer { get; set; }
    }
    public class EditResult
    {
        public JsonObject json { get; set; }
        public DocumentContainer documentContainer { get; set; }
    }
    public class RowInfo
    {
        public int rowNumber { get; set; }
        public List<CellSpan> cellSpans = new List<CellSpan>();
        public int cols = 1;
    }
    public class CellInfo
    {
        public int cellNumber {get;set;}
    }
    public class CellSpan
    {
        public int rowNumber { get; set; }
        public int colNumber { get; set; }
        public int colStart { get; set; }
        public int colEnd { get; set; }
        public int rowStart { get; set; }
        public int rowEnd { get; set; }
    }
    
    public class ElementConstructor
    {
        public XElement CreateCheckItem()
        {
            XElement checklistitem = new XElement("section",
                new XAttribute("id", Guid.NewGuid().ToString()),
                new XAttribute("data-type", "1"),
                new XAttribute("class", "check-item"),
                (
                     new XElement("h2",
                            new XAttribute("id", Guid.NewGuid().ToString()),
                            new XAttribute("class", "check-title"),
                            new XText("Tittel")
                      )
                ),
                new XElement("section",
                    new XAttribute("class", "check-ingress"),
                    new XAttribute("id", Guid.NewGuid().ToString())
                ),
                new XElement("section",
                    new XAttribute("class", "check-description"),
                    new XAttribute("id", Guid.NewGuid().ToString())
                ),
                new XElement("section",
                    new XAttribute("class", "check-law"),
                    new XAttribute("id", Guid.NewGuid().ToString())
                ),
                new XElement("section",
                    new XAttribute("id", Guid.NewGuid().ToString()),
                    new XAttribute("class", "check-children")
                )
            );
            return checklistitem;
        }
    }
    public static class Extentions
    {
        public static void SetHrefValue(this IEnumerable<XAttribute> xAttributes)
        {
            foreach (XAttribute a in xAttributes)
            {
                if (a.Parent.Attributes("href").FirstOrDefault()==null)
                {
                    a.Parent.Add(new XAttribute("href", a.Value));
                }
                else
                {
                    a.Parent.Attributes("href").First().SetValue(a.Value);
                }
            }
        }
        public static void SetDataClassValue(this IEnumerable<XAttribute> xAttributes)
        {
            foreach (XAttribute a in xAttributes)
            {
                a.Parent.Add(new XAttribute("class", a.Value));
            }
        }
        public static void SetClassValue(this IEnumerable<XAttribute> xAttributes)
        {
            foreach (XAttribute a in xAttributes)
            {
                string dataClassName = (string)a.Parent.Attributes("data-class-name").FirstOrDefault() ?? "";
                if (dataClassName != "")
                {
                    a.SetValue(dataClassName);
                }
                else
                {
                    string value = "";
                    List<string> sValues = a.Value.Split(' ').ToList();
                    foreach (string s in sValues)
                    {
                        if (s.StartsWith("table-wrapper") || s.StartsWith("diblink-"))
                        {
                            value = (value == "" ? "" : " ") + s;
                        }
                    }
                    if (value == "")
                    {
                        a.Remove();
                    }
                    else
                    {
                        a.SetValue(value);
                    }
                }
            }
        }
        public static IEnumerable<XElement> GetTocSegment(this IEnumerable<XElement> map, string segment_id, XElement document)
        {
            List<XElement> result = new List<XElement>();
            if (map == null ? true : map.Count() == 0) return result;
            foreach (XElement m in map)
            {
                if (((string)m.Attributes("id").FirstOrDefault()??"").Trim().ToLower() == segment_id.Trim().ToLower())
                {
                    result.AddRange(document.Elements().Where(p => p.Descendants().Where(d => d.IsHeaderName()).Count() > 0).HeaderToToc());
                }
                else
                {
                    int isSegment = ("1;true".Split(';').Contains((string)m.Attributes().Where(a => "segment;s".Split(';').Contains(a.Name.LocalName)).FirstOrDefault() ?? "") ? 1 : 0);
                    if ((string)m.Attributes().Where(a => "id;key".Split(';').Contains(a.Name.LocalName)).FirstOrDefault()!=null
                        && (string)m.Attributes().Where(a => "title;text".Split(';').Contains(a.Name.LocalName)).FirstOrDefault() != null)
                    { 
                        result.Add(
                            new XElement("item",
                                new XAttribute("id", (string)m.Attributes().Where(a => "id;key".Split(';').Contains(a.Name.LocalName)).FirstOrDefault()),
                                new XAttribute("seg", isSegment.ToString()),
                                new XAttribute("title", (string)m.Attributes().Where(a => "title;text".Split(';').Contains(a.Name.LocalName)).FirstOrDefault()),
                                isSegment == 0 ? null : m.Elements().GetTocSegment(segment_id, document)
                            
                            )
                        );
                    }
                }
            }
            return result;
        }
        public static string GetHeaderText(this XElement e)
        {
            return Regex.Replace(e.DescendantNodes().OfType<XText>().Where(p => p.Ancestors("sup").Count() == 0).Select(p => p.Value).StringConcatenate(" "), @"\s+", " ").Trim();
        }
        public static List<XElement> HeaderToToc(this IEnumerable<XElement> elements)
        {
            
            List<XElement> result = new List<XElement>();
            if (elements == null ? true : elements.Count() == 0) return result; 
            IEnumerable<XElement> headers = elements.Where(p => p.DescendantsAndSelf().Where(d => d.IsHeaderName()).Count() > 0);
            foreach (XElement e in headers)
            {
                if (e.IsHeaderName() && e.Parent.Name.LocalName == "section" && e.Parent.Elements().Where(p => p.IsHeaderName()).FirstOrDefault() == e)
                {
                    result.Add(new XElement("item",
                        e.Parent.Attributes("id"),
                        new XAttribute("seg", "1;true".Split(';').Contains((string)e.Parent.Attributes().Where(a => "segment;s".Split(';').Contains(a.Name.LocalName)).FirstOrDefault() ?? "") ? "1" : "0"),
                        new XAttribute("title", e.GetHeaderText()),
                        e.NodesAfterSelf().OfType<XElement>()
                        .TakeWhile(p=> !p.IsHeaderName())
                        .Where(p=> p.Descendants().Where(d=>d.IsHeaderName()).Count()>0)
                        .HeaderToToc()
                        )
                    );
                    break;
                }
                else if (e.IsHeaderName())
                {
                    result.Add(new XElement("item",
                        e.Attributes("id"),
                        new XAttribute("title", e.GetHeaderText()),
                         e.NodesAfterSelf().OfType<XElement>()
                        .TakeWhile(p => !p.IsHeaderName())
                        .Where(p=> p.Descendants().Where(d => d.IsHeaderName()).Count() > 0)
                        .HeaderToToc()
                        )
                    );
                    break;
                }
                else
                {
                    result.AddRange(e.Elements().HeaderToToc());
                }

            }

            return result;
        }
        public static List<JsonChild> GetDocumentToc(this List<JsonChild> children, Dictionary<string, JsonElement> elements)
        {
            
            var x = from c in children.Where(p => p.id != null)
                    join e in elements.Where(p => p.Value.name == "section") 
                    on c.id equals e.Key
                    
                    select e.Value;
            return null;
        }
        public static XElement GetXMLFromJsonElements(this string id, Dictionary<string, JsonElement> elements)
        {
            return elements
                .Where(p => p.Key == id)
                .Select(p =>
                    new XElement(p.Value.name,
                        p.Value.attributes == null ? null : p.Value.attributes.Select(a => new XAttribute(a.Key, a.Value)),
                        p.Value.children.Select(c => c.GetJsonChildren(elements))
                    )
                )
                .FirstOrDefault();
        }
        public static IEnumerable<XNode>ChildrenToXml(this JsonChild jc, List<BuildJson> elements)
        {
            List<XNode> result = new List<XNode>();
            if (jc.text!=null)
            {
                result.Add(new XText(jc.text));
            }
            else if (jc.id!=null)
            {
                result.AddRange(elements.Where(p => p.element.id == jc.id).SelectMany(p => p.JsonToXml(elements)));
            }
            return result;
        }
        public static IEnumerable<XNode>JsonToXml(this BuildJson buildJson, List<BuildJson> elements )
        {
            List<XNode> result = new List<XNode>();
            foreach (JsonChild jc in buildJson.element.element.children)
            {
                if (jc.text!=null)
                {
                    result.Add(new XText(jc.text));
                }
                else if (jc.id != null)
                {
                    result.AddRange(buildJson
                        .children
                        .Where(p => p.id == jc.id)
                        .Select(p => new XElement(
                                p.element.name,
                                p.element.attributes.Select(a => new XAttribute(a.Key, a.Value)),
                                p.element.children.Select(c=>c.ChildrenToXml(elements))
                            )
                        )
                    );
                        
                }

            }
            if (buildJson.element.element.name != null)
            {
                return new List<XNode> {
                        new XElement(
                            buildJson.element.element.name,
                            buildJson.element.element.attributes.Select(a => new XAttribute(a.Key, a.Value)),
                            result
                        )
                };
            }
            return result;    


        }

        public static XNode GetJsonChild(KeyValuePair<string, JsonElement> e, JsonChild c)
        {
            if (e.Key == null)
                return new XText(c.text);
            else
                return new XElement(e.Value.name);
         
        }
        public static void FillEH(this JsonHierarcy e, List<JsonHierarcy> l)
        {
            e.elements = l;
        }
        public static void FillE(this JsonXMLBuildElement e, List<JsonXMLBuildElement> l)
        {
            e.elements = l;
        }
        public static void FillBuildH(this IEnumerable<JsonHierarcy> hierarcies, Dictionary<string, JsonElement> elements)
        {
            

            var result = (
              from b in hierarcies
              from c in b.element.children
              join e in elements.DefaultIfEmpty()
              on c.id equals e.Key
              group new{ b, e, c } by b into g
              select new { a = g.Key, b = g.Select(p =>GetObject(p.e, p.c)).ToList() }
             ).ToList();
            if (result.Count() == 0) return;

            result.ForEach(p => p.a.FillEH(p.b));


            result.SelectMany(p => p.a.elements).FillBuildH(elements);

        }
        public static void FillBuild(this IEnumerable<JsonXMLBuildElement> lb, Dictionary<string, JsonElement> elements)
        {
            var result = (
              from b in lb
              from c in b.element.children
              join e in elements
              on c.id equals e.Key
              group new { b, e } by b into g
              select new { a = g.Key, b = g.Select(p => new JsonXMLBuildElement { id = p.e.Key, element = p.e.Value }).ToList() }
             ).ToList();
            if (result.Count() == 0) return;

            result.ForEach(p => p.a.FillE(p.b));

           
            result.SelectMany(p => p.a.elements).FillBuild(elements);
          
        }
        
        public static JsonHierarcy GetObject(KeyValuePair<string ,JsonElement> e, JsonChild c)
        {
            if (e.Key != null) return new JsonHierarcy { id = e.Key, element = e.Value };
            else if (c != null) return new JsonHierarcy{ text = c};
            return null;
        }
       
        public static void GetChildElements(this IEnumerable<JsonElementHierarcy> childeElements, Dictionary<string, JsonElement> elements)
        {
            //if (childeElements == null) return;
            //if (childeElements.Where(p => p == null).Count() != 0) return;
            List<IGrouping< JsonElementHierarcy, JsonElementGroup>> result =
            (from e in childeElements
             from c in e.children
             join ee in elements
             on c.id equals ee.Key
             group new JsonElementGroup { element = e, child = c, childElement = ee} by e into g
             select g
            ).ToList();

            result.ForEach(p=>p.SetJsonChild());


            if (result.Count() == 0) return;

            
            result.SelectMany(p => p.Key.children.Where(c=>c.build!=null).Select(c => c.build)).GetChildElements(elements);
        }
        public static List<JsonChild> SetJsonChild(this IGrouping<KeyValuePair<string,JsonElement>, JsonElementGroupNorn> g)
        {
            g.ToList().ForEach(p => p.child.build = new JsonElementHierarcy
            {
                name = p.childElement.Value.name,
                attributes = p.childElement.Value.attributes,
                children = p.childElement.Value.children ?? new List<JsonChild>()

            });
            return g.Key.Value.children;
        }
        public static List<JsonChild> SetJsonChild(this IGrouping<JsonElementHierarcy, JsonElementGroup> g)
        {
            g.ToList().ForEach(p => p.child.build = new JsonElementHierarcy
            {
                name = p.childElement.Value.name,
                attributes = p.childElement.Value.attributes,
                children = p.childElement.Value.children ?? new List<JsonChild>()
                
            });
            return g.Key.children;
        }
        public static XElement GetDocumentContainerXML(this DocumentContainer documentContainer)
        {
            JsonElement root = documentContainer
                .elements
                .Where(p => p.Key == documentContainer.root.Select(r => r.id).FirstOrDefault())
                .Select(p => p.Value)
                .FirstOrDefault();

            List<JsonElementHierarcy> rootjson = (
                 from c in root.children
                 join e in documentContainer.elements
                 on c.id equals e.Key
                 from cc in e.Value.children
                 join ee in documentContainer.elements
                 on cc.id equals ee.Key
                 group new JsonElementGroupNorn
                 {
                     element =  e,
                     child = cc,
                     childElement = ee
                 } by e into g
                 select new JsonElementHierarcy
                 {
                     name = g.Key.Value.name,
                     attributes = g.Key.Value.attributes,
                     children = g.SetJsonChild()

                 }
            ).ToList();
            rootjson.SelectMany(p => p.children.Select(c => c.build)).GetChildElements(documentContainer.elements);
            XElement result1 = new XElement("document",
                from be in rootjson
                select be.GetXML()
            );
            return result1;


        }
        //public static XElement GetDocumentXML(this DocumentContainer documentContainer)
        //{
        //    XElement ee = documentContainer.GetDocumentContainerXML();
        //    return ee;
        //}    
        public static XElement GetXml(this Dictionary<string, JsonElement> elements, string elementId)
        {
            List<XElement> result = new List<XElement>();

            KeyValuePair<string, JsonElement> e = elements.Where(p => p.Key.Trim().ToLower() == elementId.Trim().ToLower()).FirstOrDefault();
            XElement element = new XElement(e.Value.name,
                e.Value.attributes.Select(p => new XAttribute(p.Key, p.Value)),
                e.Value.children.Select(p => p.GetJsonChildren(elements))
            );
            return element;
        }
        public static IEnumerable<XElement> GetCellNodes(this XElement e)
        {

            List<XElement> result = new List<XElement>();
            if (e.DescendantNodes().OfType<XText>().Select(p => p.Value).StringConcatenate().Trim() == "")
            {
                return result;
            }
            else if (e.Nodes().First().NodeType == XmlNodeType.Text && e.Nodes().OfType<XElement>().Count() == 0)
            {
                result.Add(new XElement("p", new XAttribute("id", Guid.NewGuid().ToString()), e.Nodes()));
            }
            else if (e.Nodes().First().NodeType == XmlNodeType.Element && e.Nodes().OfType<XText>().Count() == 0)
            {
                result.AddRange(e.Nodes().OfType<XElement>());
            }
            return result;
        }
        public static string GetElementId(this XElement e)
        {
            return (string)e.Attributes("id").FirstOrDefault();
        }
        public static int GetAttributeInt(this XElement e, string name)
        {
            return Convert.ToInt32((string)e.Attributes().Where(p => p.Name.LocalName.ToLower() == name && Regex.IsMatch(p.Value.Trim(), @"^\d$")).FirstOrDefault() ?? "-1");
        }
        public static bool DeleteSubElement(this JsonElement e, Dictionary<string, JsonElement> elements)
        {
            List<string> children = e.children.Select(p => p.id).ToList();
            foreach (string c in children)
            {
                KeyValuePair<string, JsonElement> kvp = elements.Where(p => p.Key == c).FirstOrDefault();
                if (kvp.Key != null)
                {
                    kvp.Value.DeleteSubElement(elements);
                    elements.Remove(kvp.Key);
                }
                
            }
            return true;
        }
        //public static bool DeleteJsonElement(this Dictionary<string, JsonElement> elements, JsonDelete jsonDelete)
        //{
        //    if (elements == null) return false;
        //    if (jsonDelete.action != "delete") return false;
        //    foreach (string id in jsonDelete.id)
        //    {
        //        if (elements.ContainsKey(id))
        //        {
        //            JsonElement e = elements.Values.Where(p => p.children.Where(c => c.id == id).Count() > 0).FirstOrDefault();
        //            if (e != null)
        //            {
        //                e.RemoveChild(id);
        //            }
        //        }
        //    }

        //    return true;
        //}
        public static EditDocumentContainerResult DeleteJsonElement(this DocumentContainer documentContainer, JsonDelete jsonDelete, string resourceId, string segmentId)
        {
            string document_id = "document;" + resourceId + ";" + segmentId;
            if (documentContainer == null) return null;
            if (jsonDelete.action != "delete") return null;
            string CurrId = "";
            foreach (string id in jsonDelete.id)
            {
                CurrId = id;
                if (documentContainer.elements.ContainsKey(CurrId))
                {
                    KeyValuePair<string,JsonElement> je = documentContainer.elements.Where(p => p.Key == id).Select(p=>p).FirstOrDefault();
                    if (je.Value.name == "table")
                    {
                        je = documentContainer.elements.Where(p => p.Value.children.Where(c => c.id == je.Key).Count() != 0).FirstOrDefault();
                        if (je.Value.name == "div" && (je.Value.attributes.Where(p => p.Key == "data-class" && p.Value == "table-wrapper").Count() != 0))
                        {
                            CurrId = je.Key;
                        }
                    }
                    JsonElement e = documentContainer.elements.Values.Where(p => p.children.Where(c => c.id == CurrId).Count() > 0).FirstOrDefault();
                    if (e != null)
                    {
                        e.RemoveChild(CurrId);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            if (CurrId == "")
                return null;

            XElement document = documentContainer.GetDocumentContainerXML();
            TocJson tocJson = new TocJson(document,resourceId,segmentId);

            List<TocUpdate> tocUpdates = null;
            tocUpdates = (
                from t in documentContainer.toc
                join t1 in tocJson.toc
                on t.Key equals t1.Key
                where !t.Value.Equals(t1.Value)
                select new TocUpdate { oldToc = t, newToc = t1 }
            ).ToList();

            foreach (TocUpdate tu in tocUpdates)
            {
                if (documentContainer.toc.ContainsKey(tu.newToc.Key))
                {
                    documentContainer.toc[tu.newToc.Key] = tu.newToc.Value;
                }
                    
            }
            

            return new EditDocumentContainerResult
            {
                json = new JsonObject
                {
                    root = new List<JsonChild> { new JsonChild { id = CurrId } },
                    toc = tocUpdates == null ? null : (tocUpdates.Count() == 0 ? null : tocUpdates.Select(p => p).ToDictionary(p => p.newToc.Key, p => p.newToc.Value))
                },
                documentContainer = documentContainer
            };

        }
        public static EditDocumentContainerResult UpdateJsonElements(this DocumentContainer documentContainer, JsonUpdate jsonUpdate, string resourceId, string segmentId)
        {
            //string document_id = "document;" + resourceId + ";" + segmentId;
            if (jsonUpdate.action != "update") return null;
            bool updateToc = false;
            List<KeyValuePair<string, JsonElement>> updated = new List<KeyValuePair<string, JsonElement>>();
            
            foreach (KeyValuePair<string, JsonElement> pair in jsonUpdate.elements)
            {
                if (documentContainer.elements.ContainsKey(pair.Key))
                {
                    if (!documentContainer.elements[pair.Key].Equals(pair.Value))
                    {
                        if (Regex.IsMatch(pair.Value.name, @"^h\d$") || "section;document".Split(';').Contains(pair.Value.name) || pair.Key.StartsWith("document;")) updateToc = true;
                        documentContainer.elements[pair.Key] = pair.Value;
                        updated.Add(pair);
                    }
                }
                else
                {
                    documentContainer.elements.Add(pair.Key, pair.Value);
                    updated.Add(pair);
                }
            }
            List<TocUpdate> tocUpdates = null;
            if (updateToc)
            {
                tocUpdates = documentContainer.UpdateToc(resourceId, segmentId);

                //XElement document = documentContainer.GetDocumentContainerXML();
                

                //TocJson tocJson = new TocJson(document, resourceId, segmentId);

                //tocUpdates = (
                //    from t in documentContainer.toc
                //    join t1 in tocJson.toc
                //    on t.Key equals t1.Key
                //    where !t.Value.Equals(t1.Value)
                //    select new TocUpdate { oldToc = t, newToc = t1 }
                //).ToList();


                //foreach (TocUpdate tu in tocUpdates)
                //{
                //    if (documentContainer.toc.ContainsKey(tu.newToc.Key))
                //    {
                //        documentContainer.toc[tu.newToc.Key] = tu.newToc.Value;
                //    }
                //}
                //List<TocUpdate> newToc =
                //    tocJson
                //        .toc.Where(p => !documentContainer.toc.ContainsKey(p.Key))
                //        .Select(p => new TocUpdate { newToc = p })
                //        .ToList();

                //if (newToc.Count()>1)
                //{
                //    documentContainer.toc.AddRange(newToc.ToDictionary(p => p.newToc.Key, p => p.newToc.Value));
                //    tocUpdates.AddRange(newToc);
                //}
            }
            return new EditDocumentContainerResult
            {
                json = new JsonObject {
                    toc = tocUpdates == null ? null : (tocUpdates.Count() == 0 ? null : tocUpdates.Select(p => p).ToDictionary(p => p.newToc.Key, p => p.newToc.Value)),
                    elements = updated.Count()==0 ? null : updated.ToDictionary(p => p.Key, p => p.Value) 
                },
                documentContainer = documentContainer
            };
        }
        public static List<TocUpdate> UpdateToc(this DocumentContainer documentContainer, string resourceId, string segmentId)
        {
            List<TocUpdate> tocUpdates = null;
            XElement document = documentContainer.GetDocumentContainerXML();


            TocJson tocJson = new TocJson(document, resourceId, segmentId);

            
            tocUpdates = (
                from t in documentContainer.toc
                join t1 in tocJson.toc
                on t.Key equals t1.Key
                where !t.Value.Equals(t1.Value)
                select new TocUpdate { oldToc = t, newToc = t1 }
            ).ToList();


            foreach (TocUpdate tu in tocUpdates)
            {
                if (documentContainer.toc.ContainsKey(tu.newToc.Key))
                {
                    documentContainer.toc[tu.newToc.Key] = tu.newToc.Value;
                }
            }
            List<TocUpdate> newToc =
                tocJson
                    .toc.Where(p => !documentContainer.toc.ContainsKey(p.Key))
                    .Select(p => new TocUpdate { newToc = p })
                    .ToList();

            if (newToc.Count() > 0)
            {
                documentContainer.toc.AddRange(newToc.ToDictionary(p => p.newToc.Key, p => p.newToc.Value));
                tocUpdates.AddRange(newToc);
            }
            return tocUpdates;
        }
        //public static bool UpdateJsonElements(this Dictionary<string, JsonElement> elements, JsonUpdate jsonUpdate)
        //{
        //    if (jsonUpdate.root.action != "update") return false;

        //    foreach (KeyValuePair<string,JsonElement> pair in jsonUpdate.elements)
        //    {
        //        if (elements.ContainsKey(pair.Key))
        //        {
        //            if (pair.Value.name=="section")
        //            {
        //                (
        //                     from x in pair.Value.children.Where(p => p.id != null)
        //                     join e in elements.Where(p => Regex.IsMatch(p.Value.name, @"^h\d"))
        //                     on x.id equals e.Key
        //                     select e
        //                ).ToList();
        //            }
        //            elements[pair.Key] = pair.Value; 
        //        }
        //    }
            
        //    return true;
        //}
        public static XElement AddTableColRow(this XElement tableElemenmt,  int rows, int cols )
        {
            for (int i = 1; i <= rows; i++)
            {
                XElement tr = new XElement("tr", new XAttribute("id", Guid.NewGuid().ToString()));
                for (int j = 1; j <= cols; j++)
                {
                    tr.Add(new XElement(tableElemenmt.Name.LocalName == "tbody" ? "td" : "th", new XAttribute("id", Guid.NewGuid().ToString()), new XText("")));
                }
                tableElemenmt.Add(tr);
            }

            return tableElemenmt;
        }
        public static XElement CreateTable(this string id, int rows, int cols, int headers, int footers, string className)
        {
            XElement result = new XElement("table",
                new XAttribute("id", id),
                (className == null ? "" : className) == "" ? null : new XAttribute("class", className)
                );
            if (headers>0)
            {
                XElement thead = new XElement("thead", new XAttribute("id", Guid.NewGuid()));

                result.Add(thead.AddTableColRow(headers, cols));
            }
            XElement tbody = new XElement("tbody", new XAttribute("id", Guid.NewGuid().ToString()));
            result.Add(tbody.AddTableColRow(rows, cols));

            if (footers >0)
            {
                XElement tfoot = new XElement("tfoot", new XAttribute("id", Guid.NewGuid()));
                result.Add(tfoot.AddTableColRow(footers, cols));
            }
            
            return result;
        }
        public static void AddRange<T, S>(this Dictionary<T, S> source, Dictionary<T, S> collection)
        {
            if (collection == null)
            {
                return;
                //throw new ArgumentNullException("Collection is null");
            }

            foreach (var item in collection)
            {
                if (!source.ContainsKey(item.Key))
                {
                    source.Add(item.Key, item.Value);
                }
                else
                {
                    // handle duplicate key issue here
                }
            }
        }
        public static JsonObject Create_Table(this Dictionary<string, JsonElement> elements, JsonCreateElement jsonCreate)
        {
            JsonObject result = null;
            int rows = jsonCreate.attributes.Where(p => p.Key == "rows" && Regex.IsMatch(p.Value, @"^\d+$")).Select(p => Convert.ToInt32(p.Value)).FirstOrDefault();
            int cols = jsonCreate.attributes.Where(p => p.Key == "cols" && Regex.IsMatch(p.Value, @"^\d+$")).Select(p => Convert.ToInt32(p.Value)).FirstOrDefault();
            int headers = jsonCreate.attributes.Where(p => p.Key == "headers" && Regex.IsMatch(p.Value, @"^\d+$")).Select(p => Convert.ToInt32(p.Value)).FirstOrDefault();
            int footers = jsonCreate.attributes.Where(p => p.Key == "footers" && Regex.IsMatch(p.Value, @"^\d+$")).Select(p => Convert.ToInt32(p.Value)).FirstOrDefault();
            string className = jsonCreate.attributes.Where(p => p.Key == "class").Select(p => p.Value).FirstOrDefault();
            if (rows > 0 && cols > 0)
            {
                string newId = Guid.NewGuid().ToString();
                XElement table = newId.CreateTable(rows, cols, headers, footers, className);
                table = new XElement("div",
                    new XAttribute("class", "table-wrapper"),
                    new XAttribute("data-class", "table-wrapper"),
                    new XAttribute("id", Guid.NewGuid().ToString())
                    , table
                );
                if (table != null)
                {
                    //int i = 1;
                    //foreach (XElement tr in table.Descendants("tr").Where(p=>p.Ancestors("table").LastOrDefault()==table))
                    //{
                    //    char a = 'A';
                    //    string ix = ((char)(a + (i-1))).ToString();
                    //    int j = 1;
                    //    foreach (XElement td in tr.Elements())
                    //    {
                    //        td.Nodes().Remove();
                    //        td.Add(new XText(ix + j.ToString()));
                    //        j++;
                    //    }
                    //    i++;
                    //}
                    result = new JsonObject(table);
                   
                    return result;
                }
            }
            return result;
        }
       
        public static JsonPaste CreateChecklistItem(this JsonCreateElement jsonCreate)
        {
            JsonPaste result = null;
            XElement checklistitem = new ElementConstructor().CreateCheckItem();
            checklistitem
                .DescendantsAndSelf()
                .Where(p => (string)p.Attributes("class").FirstOrDefault() != null)
                .ToList()
                .ForEach(p => p.SetAttributeValueEx("data-class", (string)p.Attributes("class").FirstOrDefault()));
            result = checklistitem.NewElementToJson();
            return result;
        }
        public static JsonObject Create_ElementWithchildren(this Dictionary<string, JsonElement> elements, JsonCreateElement jsonCreate)
        {
            JsonObject result = null;
            string name = jsonCreate.name;
            string childName = jsonCreate.attributes.Where(p => p.Key == "childName" && Regex.IsMatch(p.Value, @"^[a-z]+([1-6])?$")).Select(p => p.Value).FirstOrDefault();
            int childCount = jsonCreate.attributes.Where(p => p.Key == "childCount" && Regex.IsMatch(p.Value, @"^\d+$")).Select(p => Convert.ToInt32(p.Value)).FirstOrDefault();
            string defaultText = jsonCreate.attributes.Where(p => p.Key == "defaultText").Select(p => p.Value).FirstOrDefault();
            if (childName == null)
                return null;

            string id = Guid.NewGuid().ToString();
            XElement returnElement = new XElement(name, new XAttribute("id", id));
            for (int i = 1; i<= childCount; i++)
            {
                returnElement.Add(new XElement(childName, new XAttribute("id", Guid.NewGuid().ToString()), new XText(""))); 
            }
            if (returnElement != null)
            {
                result = new JsonObject(returnElement);
                return result;
            }
            return result;
        }

        public static KeyValuePair<string, JsonElement> GetTable(this Dictionary<string, JsonElement> elements, string id)
        {
            KeyValuePair<string, JsonElement> result = elements.Where(p => p.Value.children.Where(c => c.id == id).FirstOrDefault() != null).FirstOrDefault();
            if (result.Key == null)
            {
                return new KeyValuePair<string, JsonElement>();
            }
            if (result.Value.name!="table")
            {
                result = GetTable(elements, result.Key);
            }

            return result;
        }
        public static RowInfo GetColRowInfo(this List<XElement> cols, RowInfo rowInfo, List<RowInfo> rowInfos)
        {
            int n = 1;
            foreach (XElement td in cols)
            {
                //Sjekk om det finnes rowspan foran
                List<CellSpan> ri = rowInfos
                                    .SelectMany(p => p.cellSpans)
                                    .Where(p => n.Between(p.colStart, p.colEnd, true))
                                    .ToList();
                //Hvis det finnes rowspan foran sett col teller til siste span +1
                if (ri != null)
                {
                    foreach (int spanEnd in ri.Select(p=>p.colEnd))
                    {
                        if (spanEnd > n)
                        {
                            n = spanEnd;
                            n++;
                        }
                    }
                }

                td.AddAnnotation(new CellInfo { cellNumber = n });
                XAttribute colspan = td.Attributes().Where(p => p.Name.LocalName.ToLower() == "colspan").FirstOrDefault();
                int colvalue = 1;
                if (colspan != null)
                {
                    colspan.SetValue(colspan.Value.Trim());
                    colvalue = Convert.ToInt32(Regex.IsMatch(colspan.Value, @"^\d+$") ? colspan.Value : "0");
                    if (colvalue > 1)
                    {
                        colspan.SetValue("1");
                        colvalue = 1;
                            
                    }
                }
                XAttribute rowspan = td.Attributes().Where(p => p.Name.LocalName.ToLower() == "rowspan").FirstOrDefault();
                int rowvalue = 1;
                if (rowspan != null)
                {
                    rowspan.SetValue(rowspan.Value.Trim());
                    rowvalue = Convert.ToInt32(Regex.IsMatch(rowspan.Value, @"^\d+$") ? colspan.Value : "0");
                    if (rowvalue > 1)
                    {
                        rowspan.SetValue("1");
                        rowvalue = 1;

                    }
                }
                if (rowvalue > 1 || colvalue > 1)
                {
                    rowInfo.cellSpans.Add(new CellSpan
                    {
                        rowNumber = rowInfo.rowNumber,
                        rowStart = rowInfo.rowNumber,
                        rowEnd = rowInfo.rowNumber + (rowvalue - 1),
                        colStart = n,
                        colEnd = n + (colvalue-1)
                    });
                }
                n++;
            }
            rowInfo.cols = n-1;
            return rowInfo;
        }
        public static void AddColumn(this XElement row, int colNumber, TableActions actions)
        {
            XElement col = row.Elements().Take(colNumber).FirstOrDefault();
            switch (actions)
            {
                case TableActions.InsertColumnAfter:
                    col.AddAfterSelf(new XElement(col.Name.LocalName, new XAttribute("id", Guid.NewGuid().ToString()), new XText(""))); break;
                case TableActions.InsertColumnBefore:
                    col.AddBeforeSelf(new XElement(col.Name.LocalName, new XAttribute("id", Guid.NewGuid().ToString()), new XText(""))); break;
            }
        }

        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
        public static EditResult PasteJsonElements(this DocumentContainer documentContainer, JsonCreateElements jsonCreate)
        {
            if (jsonCreate.action != "create" || (jsonCreate.html==null ? "" : jsonCreate.html)=="")  return null;

            XElement container = jsonCreate.ParseTextToHtml();

            if (container == null ? true : container.Nodes().Count() == 0) return null;


            JsonPaste jsonPaste = container.ParseHtmlToJsonElement();

            documentContainer.elements.AddRange(jsonPaste.elements);
            documentContainer.toc.AddRange(jsonPaste.toc);

            return new EditResult
            {
                json = new JsonObject
                {
                    resourceid = jsonCreate.resourceId,
                    root = jsonPaste.children,
                    elements = jsonPaste.elements,
                    toc = jsonPaste.toc
                },
                documentContainer = documentContainer
            };
        }
        public static IEnumerable<XElement> GetFirstHeadings(
            this IEnumerable<Heading> headingList,
            int Level
        )
        {
            return
                headingList
                    .Where(h => h.Level <= Level)
                    .Select(h =>
                        new XElement("section",
                            h.Element.Attributes("id"),
                            new XElement("h" + Level.ToString(), new XAttribute("id", Guid.NewGuid().ToString()), h.Element.Nodes()),
                           headingList.GetChildrenHeadings(h)
                        )
                    );

        }
        public static IEnumerable<XElement> GetChildrenHeadings(
           this IEnumerable<Heading> headingList,
           Heading parent
           )
        {
            int level = parent.Level + 1;
            for (int i = level; i < 7; i++)
            {
                int n = headingList
                        .SkipWhile(h => h.Element != parent.Element)
                        .Skip(1)
                        .TakeWhile(h => h.Level >= i)
                        .Where(h => h.Level == i)
                        .Count();

                if (n != 0)
                {
                    level = i;
                    return
                        headingList
                            .SkipWhile(h => h.Element != parent.Element)
                            .Skip(1)
                            .TakeWhile(h => h.Level >= level)
                            .Where(h => h.Level == level)
                            .Select(h =>
                                new XElement("section",
                                     h.Element.Attributes("id"),
                                    new XElement("h" + level.ToString(), new XAttribute("id", Guid.NewGuid().ToString()), h.Element.Nodes()),
                                    GetChildrenHeadings(headingList, h)
                                )
                            );

                }
            }
            return null;
        }
        public static XElement FlatToHierarcy(this XElement container)
        {
            IEnumerable<Heading> headingList = container
                .Descendants()
                .Where(p =>p.IsHeaderName())
                .Select(p => new Heading(p)
            );
            if (headingList.Count() == 0) return container; ;

            int Level = Convert.ToInt32(Regex.Match(headingList.First().Element.Name.LocalName, @"h(?<n>(\d+))").Groups["n"].Value);
            XElement headings = new XElement(container.Name.LocalName, GetFirstHeadings(headingList, Level));
            List<XElement> h = headings.DescendantsAndSelf("section").ToList();

            if (h.Count() == 0) return container;

            List<XNode> nodelist_before = new List<XNode>();
            XElement first = h.FirstOrDefault();
            if (first != null)
            {
                nodelist_before.AddRange(
                    container
                    .Nodes()
                    .OfType<XElement>()
                    .TakeWhile(p => ((string)p.Attributes("id").FirstOrDefault() ?? "") != (string)first.Attributes("id").FirstOrDefault())
                );
                nodelist_before.ForEach(p => p.Remove());
            }

            XElement currentHeader = null;
            foreach (XElement current in h)
            {
                List<XNode> nodelist = new List<XNode>();
                XElement next = h.SkipWhile(p => p != current).Skip(1).Take(1).FirstOrDefault();
                nodelist = new List<XNode>();
                if (next != null)
                {
                    nodelist.AddRange(
                        container
                        .Nodes()
                        .OfType<XElement>()
                        .SkipWhile(p => ((string)p.Attributes("id").FirstOrDefault() ?? "") != (string)current.Attributes("id").FirstOrDefault())
                        .TakeWhile(p => ((string)p.Attributes("id").FirstOrDefault() ?? "") != (string)next.Attributes("id").FirstOrDefault())
                    );
                    nodelist.ForEach(p => p.Remove());
                    currentHeader = current.Elements().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d")).FirstOrDefault();
                    currentHeader.AddAfterSelf(nodelist.Skip(1));
                }
                else
                {
                    nodelist.AddRange(
                        container
                        .Nodes()
                        .OfType<XElement>()
                        .SkipWhile(p => ((string)p.Attributes("id").FirstOrDefault() ?? "") != (string)current.Attributes("id").FirstOrDefault())

                    );
                    nodelist.ForEach(p => p.Remove());
                    currentHeader = current.Elements().Where(p => Regex.IsMatch(p.Name.LocalName, @"h\d")).FirstOrDefault();
                    currentHeader.AddAfterSelf(nodelist.Skip(1));
                }
            }

            if (nodelist_before.Count() != 0)
            {
                headings.AddFirst(nodelist_before);
            }
            return headings;
        }
        public static XElement GetAgilityXMLAll(string inputHtml)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            try
            {
                inputHtml = WebUtility.HtmlDecode(inputHtml);
                Stream stream = GenerateStreamFromString(inputHtml);
                htmlDoc.OptionOutputAsXml = true;
                htmlDoc.OptionAutoCloseOnEnd = true;

                htmlDoc.Load(stream, true);

                StringWriter writer = new StringWriter();
                htmlDoc.Save(writer);
                StringReader reader = new StringReader(writer.ToString());

                XDocument xDocument = XDocument.Load(reader);
                if (xDocument == null) return null;
                xDocument.RemoveNameSpace();
                xDocument.Root.DescendantNodesAndSelf().OfType<XComment>().Remove();
                XElement result = xDocument.Root;
                if (result == null) return null;

                string inline = "a;abbr;acronym;audio;b;bdi;bdo;big;br;button;canvas;cite;code;data;datalist;del;dfn;em;embed;i;iframe;img;input;ins;kbd;label;map;mark;meter;noscript;object;output;picture;progress;q;ruby;s;samp;script;select;slot;small;span;strong;sub;sup;svg;template;textarea;time;u;tt;var;video;wbr;";
                string block = "address;article;aside;blockquote;details;dialog;dd;div;dl;dt;fieldset;figcaption;figure;figcaption;footer;form;h1;h2;h3;h4;h5;h6;header;hgroup;hr;li;main;nav;ol;p;pre;section;table;ul";



                XElement container = new XElement("container", result);
                
                container.Descendants()
                    .Where(p=>!((string)p.Attributes("id").FirstOrDefault()??"").IsGuid())
                    .ToList()
                    .ForEach(p => p.SetAttributeValueEx("id", Guid.NewGuid().ToString()));
                

                container
                    .Descendants("table")
                    .Where(p => p.Parent.Name.LocalName + ((string)p.Parent.Attributes("class").FirstOrDefault() ?? "") != "divtable-wrapper")
                    .Reverse()
                    .ToList()
                    .ForEach(p => p.ReplaceWith(new XElement("div", new XAttribute("id", Guid.NewGuid().ToString()), new XAttribute("class", "table-wrapper"), p)));

                container.Descendants().Attributes().Where(p => p.Name.LocalName == "class").ToList().ForEach(p => p.Remove());
                container.Descendants().Attributes().Where(p => p.Name.LocalName == "data-class").ToList().ForEach(p => p.Parent.SetAttributeValueEx("class",p.Value));
                
                return container;
            }
            catch
            {
                return null;
            }
        }
        public static XElement GetAgilityXML(string inputHtml, string action)
        {

            HtmlDocument htmlDoc = new HtmlDocument();
            try
            {
                Stream stream = GenerateStreamFromString(inputHtml);
                htmlDoc.OptionOutputAsXml = true;
                htmlDoc.OptionAutoCloseOnEnd = true;

                htmlDoc.Load(stream, true);
                
                StringWriter writer = new StringWriter();
                htmlDoc.Save(writer);
                StringReader reader = new StringReader(writer.ToString());

                XDocument xDocument = XDocument.Load(reader);
                if (xDocument == null) return null;
                xDocument.RemoveNameSpace();
                xDocument.Root.DescendantNodesAndSelf().OfType<XComment>().Remove();
                XElement result = xDocument.Root;
                if (result == null) return null;

                if (result.Name.LocalName != "span")
                    result = new XElement("span", result);
                string inline = "a;abbr;acronym;audio;b;bdi;bdo;big;br;button;canvas;cite;code;data;datalist;del;dfn;em;embed;i;iframe;img;input;ins;kbd;label;map;mark;meter;noscript;object;output;picture;progress;q;ruby;s;samp;script;select;slot;small;span;strong;sub;sup;svg;template;textarea;time;u;tt;var;video;wbr;";
                string block = "address;article;aside;blockquote;details;dialog;dd;div;dl;dt;fieldset;figcaption;figure;figcaption;footer;form;h1;h2;h3;h4;h5;h6;header;hgroup;hr;li;main;nav;ol;p;pre;section;table;ul";

                result.DescendantsAndSelf().Attributes().ToList().ForEach(p => p.Parent.RenameAttribute(p.Name.LocalName, p.Name.LocalName.ToLower()));
                result.Descendants().Reverse().ToList().ForEach(p => p.ReplaceWith(new XElement(p.Name.LocalName.ToLower(), p.Attributes(), p.Nodes())));


                foreach (XElement element in result.Descendants().Reverse())
                {
                    string name = element.Name.LocalName.ToLower();
                    if (Regex.IsMatch(name, @"h\d"))
                    {
                        element.ReplaceWith(new XElement(name, element.DescendantNodes().OfType<XText>().Where(p => p.Ancestors("sup").Count() == 0).Select(p => p.Value).StringConcatenate()));
                    }
                    else if ("ol".Split(';').Contains(name))
                    {
                        element.ReplaceWith(new XElement(name, element.Attributes("type"), element.Nodes()));
                    }
                    else if ("ul;dl;li;table;tr;td;th;tbody;tfoot;thead".Split(';').Contains(name))
                    {
                        element.ReplaceWith(new XElement(name, element.Nodes()));
                    }
                    else if ("section;div".Split(';').Contains(name) && element.DescendantNodes().OfType<XText>().Select(p => p.Value).StringConcatenate().Trim() == "")
                    {
                        element.ReplaceWith(element.Nodes());
                    }
                    else if ("div".Split(';').Contains(name))
                    {
                        element.ReplaceWith(new XElement("section", element.Nodes()));
                    }
                    else if ("a".Split(';').Contains(name))
                    {
                        //element.ReplaceWith(element.Nodes());
                    }
                    else if ("span".Split(';').Contains(name) && element.DescendantNodes().OfType<XText>().Select(p=>p.Value).StringConcatenate().Trim() == "")
                    {
                        element.ReplaceWith(element.Nodes());
                    }
                    else if ("em;strong;p".Contains(name) && element.DescendantNodes().OfType<XText>().Select(p => p.Value).StringConcatenate().Trim() == "")
                    {
                        element.Remove();
                    }
                    else if ( !"br".Split(';').Contains(name) && element.DescendantNodes().OfType<XText>().Select(p => p.Value).StringConcatenate().Trim() == "" && element.Nodes().OfType<XElement>().Count()==0)
                    {
                        element.Remove();
                    }
                }

                XElement container = new XElement("container");
                while (result.Nodes().Count()>0)
                {
                    int nCount = 0;
                    List<XNode> n = result
                        .Nodes()
                        .TakeWhile(p =>!(p.NodeType == XmlNodeType.Text || p.NodeType == XmlNodeType.Element)
                        ).ToList();
                    nCount = nCount + n.Count();
                    n.ForEach(p => p.Remove());
                    

                    n = result
                        .Nodes()
                        .TakeWhile(p =>
                        p.NodeType == XmlNodeType.Text
                        || (p.NodeType == XmlNodeType.Element
                                ? inline.Split(';').Contains(((XElement)p).Name.LocalName)
                                : false
                            )
                        ).ToList();
                    nCount = nCount + n.Count();
                    n.ForEach(p => p.Remove());
                    if (n.Count()>0)
                        container.Add(new XElement("p", n));
                    n = result
                        .Nodes()
                        .TakeWhile(p => p.NodeType == XmlNodeType.Element ? block.Split(';').Contains(((XElement)p).Name.LocalName) : false)
                        .ToList();
                    nCount = nCount + n.Count();
                    n.ForEach(p => p.Remove());
                    if (n.Count() > 0)
                        container.Add(n);
                    if (nCount==0)
                    {
                        break;
                    }
                }
                if (action == "create")
                {
                    container.DescendantsAndSelf().ToList().ForEach(p => p.SetAttributeValueEx("id", Guid.NewGuid().ToString()));
                }
                else
                {
                    container.DescendantsAndSelf().ToList().ForEach(p => p.SetAttributeValueEx("id", Guid.NewGuid().ToString()));
                }
                

                //container.Descendants().Attributes("class").Where(p => p.Value.Trim().ToLower().StartsWith("sc-")).ToList().ForEach(p => p.Remove());
                container.Descendants().Attributes("title").Remove();
                container.Descendants().Attributes("class").SetClassValue();
                container.Descendants().Attributes("data-class-name").Where(p=>p.Parent.Attributes("class").FirstOrDefault()==null).SetDataClassValue();
                container.Descendants().Attributes("data-href").SetHrefValue();
                container.Descendants().Attributes("rel").Remove();
                container.Descendants().Attributes("target").Remove();

                container
                    .Descendants("table")
                    .Where(p => p.Parent.Name.LocalName + ((string)p.Parent.Attributes("class").FirstOrDefault() ?? "") != "divtable-wrapper")
                    .Reverse()
                    .ToList()
                    .ForEach(p => p.ReplaceWith(new XElement("div", new XAttribute("id", Guid.NewGuid().ToString()), new XAttribute("class", "table-wrapper"), p)));

                return container;
            }
            catch
            {
                return null;
            }
        }
        
        public static List<XElement> GetToc(this XElement xElement, List<XElement> sections)
        {
            List<XElement> result = new List<XElement>();
            foreach (XElement e in xElement.Elements())
            {
                XElement section = sections.Where(p => p == e).FirstOrDefault();
                if (section!=null)
                {
                    result.Add(
                        new XElement("item",
                            new XAttribute("id", (string)e.Attributes("id").FirstOrDefault()),
                            new XAttribute("title", ((XElement)e.FirstNode).DescendantNodes().OfType<XText>().Where(p=>p.Ancestors("sup").Count()==0).Select(p=>p.Value).StringConcatenate()),
                            e.GetToc(sections)
                        )
                    );
                }
                else
                {
                    result.AddRange(e.GetToc(sections));
                }
            }
            return result;
        }
       
        public static XElement ParseTextToHtml(this JsonCreateElements jsonCreate)
        {
            try
            {
                string inputHtml = WebUtility.HtmlDecode(jsonCreate.html);
                XElement container = GetAgilityXML(inputHtml, jsonCreate.action);
                if (container == null) return null;

                return container;
            }
            catch
            {
                return null;
            }
        }

        public static bool EvalHeaderSections(this XNode n)
        {
            if (n.NodeType == XmlNodeType.Text)
            {
                return true;
            }
            else if (n.NodeType==XmlNodeType.Element)
            {
                XElement test = (XElement)n;
                if (test.Name.LocalName.Trim().ToLower()=="section" && test.Descendants().Where(p=>p.IsHeaderName()).Count()>0)
                {
                    return false;
                }
            }
            return true;
        }
        public static bool IsHeaderName(this XElement e)
        {
            return Regex.IsMatch(e.Name.LocalName.Trim().ToLower(), @"h\d");
        }
        public static int ElementHeaderCount(this XElement s)
        {
            return s.Elements().Where(p => p.IsHeaderName()).Count();
        }
        public static JsonPaste NewElementToJson(this XElement element)
        {
            JsonPaste result = new JsonPaste();
            TocJson tocJson = new TocJson(element);
            if ((tocJson.toc == null ? 0 : tocJson.toc.Count()) > 0)
            {
                result.toc = tocJson.toc;
            }
            JsonObject json = new JsonObject(element);
            result.children.AddRange(json.root);
            result.elements.AddRange(json.elements);
            return result;
        }
        public static JsonPaste ParseHtmlToJsonElement(this XElement container)
        {
            JsonPaste result = new JsonPaste();
            if (
                !(container.Elements().Count() == 1 ? "h1;h2;h3;h4;h5;h6;p".Split(';').Contains(container.Elements().FirstOrDefault().Name.LocalName) : false)
            )
            {
                if (container.Elements().Where(p => "h1;h2;h3;h4;h5;h6".Split(';').Contains(p.Name.LocalName)).Count() > 1)
                {
                    container = container.FlatToHierarcy();
                }
            }
            TocJson tocJson = new TocJson(container);

            if ((tocJson.toc == null ? 0 : tocJson.toc.Count()) > 0)
            {
                result.toc = tocJson.toc;
            }

            foreach (XNode xNode in container.Nodes())
            {
                if (xNode.NodeType == XmlNodeType.Element)
                {
                    XElement xElement = (XElement)xNode;
                    JsonObject json = new JsonObject( xElement);
                    result.children.AddRange(json.root);
                    result.elements.AddRange(json.elements);
                }
                else if (xNode.NodeType == XmlNodeType.Text)
                {
                    result.children.Add(new JsonChild { text = ((XText)xNode).Value });
                }
            }
            return result;
        }
        public static Dictionary<string, JsonElement> GetJsonCreateElementChildren(this string id, JsonCreateElement element, Dictionary<string, JsonCreateElement> elements)
        {
            Dictionary<string, JsonElement> result = new Dictionary<string, JsonElement>();
            if (element.children.Where(p=>p.id!=null).Count()>0)
            {
                JsonElement json = new JsonElement();
                
                json.name = element.name;
                json.attributes.AddRange(element.attributes.Where(p => !"key;id".Split(';').Contains(p.Key)).ToDictionary(p => p.Key, p => p.Value));
                json.attributes.Add("id", id);
                json.children = new List<JsonChild>();
                foreach (JsonChild jc in element.children)
                {
                    if (jc.id!=null)
                    {
                        KeyValuePair<string, JsonCreateElement> el = elements.Where(p => p.Key.Trim().ToLower() == jc.id.Trim().ToLower()).FirstOrDefault();
                        if (el.Key!=null)
                        {
                            string newId = Guid.NewGuid().ToString();
                            json.children.Add(new JsonChild { id = newId });
                            result.AddRange(newId.GetJsonCreateElementChildren(el.Value, elements));
                        }
                        
                    }
                    else if (jc.text!=null)
                    {
                        json.children.Add(jc);
                    }
                }
                result.Add(id, json);
            }
            else
            {
                JsonElement json = new JsonElement();
                json.attributes.AddRange(element.attributes.Where(p => !"key;id".Split(';').Contains(p.Key)).ToDictionary(p => p.Key, p => p.Value));
                json.attributes.Add("id", id);
                json.name = element.name;
                json.children = element.children;
                result.Add(id, json);
            }
            return result;
        }
        public static EditResult CreateJsonElement(this DocumentContainer documentContainer , JsonCreateElements jsonCreate)
        {
            string newId = "";
            JsonObject result = null;
            jsonCreate.resourceId = jsonCreate.resourceId.ToLower();
            
            if (jsonCreate.action == "create" && jsonCreate.html != null)
            {
                XElement container = GetAgilityXMLAll(jsonCreate.html);
                JsonObject json = new JsonObject(container.Elements());
                bool updateToc = false;
                List<KeyValuePair<string, JsonElement>> updated = new List<KeyValuePair<string, JsonElement>>();
                bool variableAdded = false;
                foreach (KeyValuePair<string, JsonElement> pair in json.elements)
                {
                    if (documentContainer.elements.ContainsKey(pair.Key))
                    {
                        if (!documentContainer.elements[pair.Key].Equals(pair.Value))
                        { 
                            if (Regex.IsMatch(pair.Value.name, @"^h\d$") || "section;document".Split(';').Contains(pair.Value.name) || pair.Key.StartsWith("document;")) updateToc = true;
                            documentContainer.elements[pair.Key] = pair.Value;
                            updated.Add(pair);
                        }
                    }
                    else
                    {
                        documentContainer.elements.Add(pair.Key, pair.Value); 
                        updated.Add(pair);
                    }
                    if (pair.Value.name == "span" && pair.Value.attributes.ContainsKey("data-var-id") && pair.Value.attributes.ContainsKey("class") ? pair.Value.attributes["class"].Split(' ').Contains("dib-x-var") : false)
                    {
                        string varId = pair.Value.attributes["data-var-id"];
                        if (!documentContainer.variableroot.Contains(varId))
                        {
                            documentContainer.variableroot.Add(varId);
                            variableAdded = true;

                        }
                    }
                }
                List<TocUpdate> tocUpdates = null;
                if (updateToc)
                {
                    tocUpdates = documentContainer.UpdateToc(documentContainer.id, documentContainer.segmentId);
                }

                return new EditResult
                {
                    json = new JsonObject
                    {
                        resourceid = jsonCreate.resourceId,
                        root = json.root,
                        elements = updated.ToDictionary(p=>p.Key, p=>p.Value),
                        toc = tocUpdates == null ? null : (tocUpdates.Count() == 0 ? null : tocUpdates.Select(p => p).ToDictionary(p => p.newToc.Key, p => p.newToc.Value)),
                        variableroot = variableAdded ? documentContainer.variableroot : null
                    },
                    documentContainer = documentContainer
                };

            }
            else if (jsonCreate.action=="paste" && jsonCreate.html!=null)
            {
                return PasteJsonElements(documentContainer, jsonCreate);
            }
            else if (jsonCreate.action == "create" && (jsonCreate.element == null ? 0 : jsonCreate.element.Where(p => p.name != "").Count()) > 0)
            {
                result = new JsonObject
                {
                    resourceid = jsonCreate.resourceId,
                    root = new List<JsonChild>(),
                    elements = new Dictionary<string, JsonElement>()
                };
                Dictionary<string, string> newAttributes = null;
                foreach (JsonCreateElement jce in jsonCreate.element)
                {

                    switch (jce.name)
                    {

                        case "ol":
                        case "ul":
                            {
                                newId = Guid.NewGuid().ToString();
                                XElement list = new XElement(jce.name,
                                    new XAttribute("id", newId),
                                    new XAttribute("type", jce.name == "ul" ? "disc" : "1"),
                                    new XElement("li",
                                        new XAttribute("id", Guid.NewGuid().ToString()),
                                        new XText("")
                                    )
                                );

                                JsonObject json = new JsonObject(list);
                                result.root.AddRange(json.root);
                                result.elements.AddRange(json.elements);
                                break;
                            }
                        case "table":
                            {
                                JsonObject json = documentContainer.elements.Create_Table(jce);
                                result.root.AddRange(json.root);
                                result.elements.AddRange(json.elements);
                                break;
                            }

                        case "tr":
                            {
                                JsonObject json = documentContainer.elements.Create_ElementWithchildren(jce);
                                result.root.AddRange(json.root);
                                result.elements.AddRange(json.elements);
                                break;
                            }
                        case "p":
                            newId = Guid.NewGuid().ToString();
                            newAttributes = new Dictionary<string, string>();
                            newAttributes.Add("id", newId);
                            newAttributes.AddRange(jce.attributes.Where(p => p.Key != "id").ToDictionary(p => p.Key, p => p.Value));
                            result.root.Add(new JsonChild { id = newId });
                            result.elements.Add(newId, new JsonElement
                            {
                                name = jce.name,
                                attributes = newAttributes,
                                children = jce.children == null
                                    ? new List<JsonChild>() { new JsonChild { text = "" } }
                                    : (
                                        jce.children.Count() == 0
                                        ? new List<JsonChild>() { new JsonChild { text = "" } }
                                        : jce.children
                                       )
                            }
                            );
                            break;

                        default:

                            if (jce.name=="section" && jce.attributes.Where(p => "className;class".Split(';').Contains(p.Key) && p.Value == "check-item").Count() > 0)
                            {
                                JsonPaste json = jce.CreateChecklistItem();
                                result.root.AddRange(json.children);
                                result.elements.AddRange(json.elements);
                                result.toc.AddRange(json.toc);
                                documentContainer.elements.AddRange(json.elements);
                                documentContainer.toc.AddRange(json.toc);
                                return new EditResult
                                {
                                    json = new JsonObject
                                    {
                                        resourceid = jsonCreate.resourceId,
                                        root = json.children,
                                        elements = json.elements,
                                        toc = json.toc
                                    },
                                    documentContainer = documentContainer
                                };
                            }
                            else
                            {
                                newId = Guid.NewGuid().ToString();
                                newAttributes = new Dictionary<string, string>();
                                newAttributes.Add("id", newId);
                                newAttributes.AddRange(jce.attributes.Where(p => p.Key != "id").ToDictionary(p => p.Key, p => p.Value));
                                result.root.Add(new JsonChild { id = newId });
                                result.elements.Add(newId, new JsonElement
                                {
                                    name = jce.name,
                                    attributes = newAttributes,
                                    children = jce.children == null ? new List<JsonChild>() : jce.children
                                }
                                );

                            }
                            
                            break;
                    }
                }
                documentContainer.elements = documentContainer.elements
                    .Where(x => !result.elements.ContainsKey(x.Key))
                    .ToDictionary(x => x.Key, x => x.Value);
                documentContainer.elements.AddRange(result.elements);
                return new EditResult
                {
                    json = result,
                    documentContainer = documentContainer
                };

            }
            else if (jsonCreate.action == "create" && (jsonCreate.root == null ? 0 :  jsonCreate.root.Count()) >0 )
            {
                result = new JsonObject();
                result.root = new List<JsonChild>();

                result.elements = new Dictionary<string, JsonElement>();
                if (jsonCreate.elements!= null)
                {
                    jsonCreate.elements.SelectMany(p => p.Value.children).Where(p => p.text != null).ToList().ForEach(p => p.text = WebUtility.HtmlDecode(p.text));
                    foreach (JsonChild jc in jsonCreate.root)
                    {
                        if (jc.id!= null)
                        {
                            KeyValuePair<string, JsonCreateElement> el = jsonCreate.elements.Where(p => p.Key.Trim().ToLower() == jc.id.Trim().ToLower()).FirstOrDefault();
                            if (el.Key != null)
                            {
                                newId = Guid.NewGuid().ToString();
                                result.root.Add(new JsonChild { id = newId });
                                result.elements.AddRange(newId.GetJsonCreateElementChildren(el.Value, jsonCreate.elements));
                            }
                        }
                        else if (jc.text != null)
                        {
                            result.root.Add(jc);
                        }
                    }
                }
                
                if (result.elements.Where(p=>Regex.IsMatch(p.Value.name,@"h\d")).Count()>0 || result.elements.Where(p => Regex.IsMatch(p.Value.name, @"table")).Count() > 0)
                {
                    XElement container = new XElement("container", result.elements.GetXml(result.root.Select(p=>p.id).FirstOrDefault()));
                    if (container == null ? true : container.Nodes().Count() == 0) return null;

                    container
                    .Descendants("table")
                    .Where(p => p.Parent.Name.LocalName + ((string)p.Parent.Attributes("data-class").FirstOrDefault() ?? "") != "divtable-wrapper")
                    .Reverse()
                    .ToList()
                    .ForEach(p => p.ReplaceWith(new XElement("div", new XAttribute("id", Guid.NewGuid().ToString()), new XAttribute("class", "table-wrapper"), new XAttribute("data-class", "table-wrapper"), p)));


                    JsonPaste jsonPaste = container.ParseHtmlToJsonElement();
                    documentContainer.elements.AddRange(jsonPaste.elements);
                    documentContainer.toc.AddRange(jsonPaste.toc);
                    return new EditResult
                    {
                        json = new JsonObject
                        {
                            resourceid = jsonCreate.resourceId,
                            root = jsonPaste.children,
                            elements = jsonPaste.elements,
                            toc = jsonPaste.toc
                        },
                        documentContainer = documentContainer
                    };
                }
                else
                {
                    documentContainer.elements.AddRange(result.elements);
                    return new EditResult
                    {
                        json = result,
                        documentContainer = documentContainer
                    };

                }
            }
            else if (jsonCreate.action == "insertrow after"
                || jsonCreate.action == "insertrow before"
                || jsonCreate.action == "insertcolumn before"
                || jsonCreate.action == "insertcolumn after"
                || jsonCreate.action == "span"
                || jsonCreate.action == "unspan"
                || jsonCreate.action == "deleterow"
                || jsonCreate.action == "deletecolumn"
                )
            {
                TableActions actions;
                switch (jsonCreate.action)
                {
                    case "insertrow after": actions = TableActions.InsertRowAfter; break;
                    case "insertrow before": actions = TableActions.InsertRowBefore; break;
                    case "insertcolumn before": actions = TableActions.InsertColumnBefore; break;
                    case "insertcolumn after": actions = TableActions.InsertColumnAfter; break;
                    case "span": actions = TableActions.Span; break;
                    case "unspan": actions = TableActions.Unspan; break;
                    case "deleterow": actions = TableActions.DeleteRow; break;
                    case "deletecolumn": actions = TableActions.DeleteColumn; break;
                    default:
                        return null;
                }
                
                string id = jsonCreate.id;
                KeyValuePair<string, JsonElement> jsontableelement = documentContainer.elements.GetTable(id);
                if (jsontableelement.Key == null)
                {
                    return null;
                }
                XElement table = documentContainer.elements.GetXml(jsontableelement.Key);

                //TableElements tableElements = new TableElements(table);
                TableEditObject editObject = new TableEditObject
                {
                    start = id,
                    end = jsonCreate.toid,
                    actions = actions
                };
                TableProduction tableProduction = new TableProduction(table, editObject);
                table = tableProduction.Table;

                result = new JsonObject(table, jsonCreate.resourceId);

                documentContainer.elements = documentContainer.elements
                    .Where(x => !result.elements.ContainsKey(x.Key))
                    .ToDictionary(x => x.Key, x => x.Value);
                documentContainer.elements.AddRange(result.elements);

                return new EditResult
                {
                    json= result,
                    documentContainer = documentContainer
                }; 

            }

            else if (jsonCreate.action != "create")
            {
                return null;
            }

            //if (documentContainer.elements.ContainsKey(jsonCreate.id) && jsonCreate.html == null) return null;

            //if (jsonCreate.html != null && jsonCreate.id != null)
            //{
            //    string id = jsonCreate.id;
            //    string shtml = WebUtility.HtmlDecode(jsonCreate.html);
            //    XElement html = XElement.Parse(shtml);
            //    if (html == null) return null;
            //    List<string> elementIds = html.DescendantsAndSelf().Attributes("id").Select(p => p.Value.ToLower()).ToList();
            //    elements = elements
            //        .Where(x => !elementIds.Contains(x.Key.ToLower()))
            //        .ToDictionary(x => x.Key, x => x.Value);
            //    foreach (XElement element in  html.Descendants())
            //    {
            //        element.SetAttributeValueEx("id", Guid.NewGuid().ToString());
            //    }
            //    html.SetAttributeValueEx("id", id);
            //    result = new JsonObject(html, jsonCreate.resourceid);
            //    elements.AddRange(result.elements);


            //    return new EditResult
            //    {
            //        json = result,
            //        elements = elements
            //    };


            //}

            return null;
        }
        

        public static List<JsonChild> GetElementChildren(this XElement element)
        {
            List<JsonChild> result = new List<JsonChild>();
            foreach (XNode n in element.Nodes())
            {
                if (n.NodeType == XmlNodeType.Element)
                {
                    XElement elementnode = (XElement)n;
                    string id = ((string)elementnode.Attributes("id").FirstOrDefault() ?? "").Trim();
                    if (id!="")
                    {
                        result.Add(new JsonChild { id = id });
                    }
                    else
                    {

                    }
                }
                else if (n.NodeType == XmlNodeType.Text)
                {
                    result.Add(new JsonChild { text = ((XText)n).Value });
                }
            }
            return result;
        }
        public static IEnumerable<XNode> GetJsonChildrens(this List<JsonChild> jsonChildren, Dictionary<string, JsonElement> elements)
        {
            List<XNode> result = new List<XNode>();
            List<SubJsonElement> subelements = (from e in elements
                                            join c in jsonChildren.Where(p => p.id != null)
                                            on e.Key equals c.id
                                            select new SubJsonElement { id = e.Key, element = e.Value }).ToList();
            foreach (JsonChild jc in jsonChildren)
            {
                if (jc.text != null)
                    result.Add(new XText(jc.text));
                else if (jc.id != null)
                    result.AddRange(
                        subelements
                        .Where(p => p.id == jc.id)
                        .Select(p=>new XElement(p.element.name,
                            p.element.attributes.Select(a => new XAttribute(a.Key, a.Value)),
                            p.element.children.GetJsonChildrens(elements)
                            )
                        )
                    );

            }
            return result;
        }
        public static XNode GetJsonChildren(this JsonChild jc, Dictionary<string, JsonElement> elements)
        {
            if (jc.text != null)
            {
                return new XText(jc.text);
            }
            else if (jc.id != null)
            {
                KeyValuePair<string, JsonElement> e = elements.Where(p => p.Key.Trim().ToLower() == jc.id.Trim().ToLower()).FirstOrDefault();
                if (e.Key != null)
                {
                    return new XElement(e.Value.name,
                        e.Value.attributes.Select(p => new XAttribute(p.Key, p.Value)),
                        e.Value.children.Select(p => p.GetJsonChildren(elements))
                    );
                }
                else
                {
                    return new XElement("element", new XAttribute("id", jc.id));
                }

            }
            return null;
        }
    }

    public class JsonUpdateRoot
    {
        public string item { get; set; }
        public string action { get; set; }
    }
    public class JsonUpdate
    {
        public string resourceId { get; set; }
        public string segmentId { get; set; }
        //public string item { get; set; }
        public string action { get; set; }
        //public JsonUpdateRoot root { get; set; }
        public Dictionary<string, JsonElement> elements { get; set; }
    }

    public class JsonDelete
    {
        public string resourceId { get; set; }
        public string segmentId { get; set; }
        public string item { get; set; }
        public string action { get; set; }
        public List<string> id { get; set; }

        
    }
    public class JsonDocumentCreate
    {
        public string topicId { get; set; }
        public string resourceId { get; set; }
        public string segmentId { get; set; }
        public int? resourcetypeId { get; set; }
        public string op { get; set; }
        public string name { get; set; }

    }
    public class JsonPaste
    {
        public List<JsonChild> children = new List<JsonChild>();
        public Dictionary<string, JsonElement> elements = new Dictionary<string, JsonElement>();
        public Dictionary<string, JsonToc> toc = new Dictionary<string, JsonToc>();
    }
  

    public class JsonCreateElements
    {
        public string resourceId { get; set; }
        public string segmentId { get; set; }
        public string item { get; set; }
        public string action { get; set; }
        public string id { get; set; }
        public string toid { get; set; }
        public string html { get; set; }
        public List<JsonChild> root { get; set; }
        public List<JsonCreateElement> element { get; set; }
        public Dictionary<string, JsonCreateElement> elements { get; set; }
    }
    public class JsonCreateElement
    {
        public Dictionary<string, string> attributes { get; set; }
        public List<JsonChild> children { get; set; }
        public string name { get; set; }
    }
    public class JsonCreate
    {
        public JsonElement element { get; set; }
        public string resourceid { get; set; }
        public string segmentid { get; set; }
        public string item { get; set; }
        public string action { get; set; }
        public string id { get; set; }
        public string toid { get; set; }
        
        public JsonCreate()
        {
            element = new JsonElement
            {
                attributes = new Dictionary<string, string>()
            };

            element.children = new List<JsonChild>();

            if (element.attributes.ContainsKey("id"))
            {
                id = element.attributes.ContainsKey("id").ToString();
            }
            else
            {
                id = Guid.NewGuid().ToString();
                element.attributes.Add("id", id);
            }
        }
    }
    
    public class JsonPart
    {
        public string resourceid { get; set; }
        public string segmentid { get; set; }
        public List<JsonChild> root { get; set; }
        public Dictionary<string, JsonElement> elements { get; set; }
    }
    public class JsonObject
    {
        public string resourceid { get; set; }
        public List<JsonChild> root { get; set; }
        public Dictionary<string, JsonElement> elements { get; set; }

        public Dictionary<string, JsonToc> toc = new Dictionary<string, JsonToc>();
        public List<string> variableroot { get; set; }
        public JsonObject() { }
        public JsonObject(IEnumerable<XElement> xelements)
        {
            root = xelements.Select(p => new JsonChild { id = (string)p.Attributes("id").FirstOrDefault() }).ToList();
            elements = xelements
                .DescendantsAndSelf()
                .Select(p=>p)
                .ToDictionary(p => (string)p.Attributes("id").FirstOrDefault(), p => new JsonElement(p));
        }
        public JsonObject(XElement element)
        {
            root = new List<JsonChild> { new JsonChild { id = (string)element.Attributes("id").FirstOrDefault() } };
            elements = element
                .DescendantsAndSelf()
                .Where(p => ((string)p.Attributes("id").FirstOrDefault() ?? "").Trim() != "")
                .Select(p => p)
                .ToDictionary(p => (string)p.Attributes("id").FirstOrDefault(), p => new JsonElement(p));
        }
        public JsonObject(XElement element, string resourceId)
        {
            resourceid = resourceId;
            root = new List<JsonChild> { new JsonChild { id = (string)element.Attributes("id").FirstOrDefault() } };
            elements = element
                .DescendantsAndSelf()
                .Where(p => ((string)p.Attributes("id").FirstOrDefault() ?? "").Trim() != "")
                .Select(p => p)
                .ToDictionary(p => (string)p.Attributes("id").FirstOrDefault(), p => new JsonElement(p));
        }
    }

    public class JsonToc : IJsonToc, IEquatable<JsonToc>
    {
        public Dictionary<string, string> attributes { get; set; }
        public List<JsonChild> children { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public JsonToc() { }
        public JsonToc(XElement element)
        {
            name = "toc";
            children = element.GetElementChildren();
            attributes = element.Attributes().Select(p => p).ToDictionary(p => p.Name.LocalName, p => p.Value);
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(JsonToc other)
        {
            if (other == null) return false;
            if ((other.name ?? "null") != (name ?? "null")) return false;
            if ((other.type ?? "null") != (type ?? "null")) return false;

            if ((other.attributes == null ? "" : other.attributes.Select(p => p.Key + ":" + p.Value).StringConcatenate(";")) != (attributes == null ? "" : attributes.Select(p => p.Key + ":" + p.Value).StringConcatenate(";"))) return false;
            if ((other.children == null ? "" : other.children.Select(p => p.id).StringConcatenate()) != (children == null ? "" : children.Select(p => p.id).StringConcatenate())) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(attributes, children, name, type);
        }
    }
    public class JsonElement : IJsonElement, IEquatable<JsonElement>
    {

        public Dictionary<string, dynamic> otherprops { get; set; }
        public Dictionary<string, string> attributes { get; set; }
        public List<JsonChild> children { get; set; } = new List<JsonChild>();
        public string  name { get; set; }
        public string type { get; set; }
        

        public JsonElement(){}
        public JsonElement(XElement element)
        {
            name = element.Name.LocalName;
            children = element.GetElementChildren();
            attributes = element.Attributes().Where(p=>(p.Value==null ? "" : p.Value)!="").OrderBy(p=>p.Name.LocalName).Select(p => p).ToDictionary(p => p.Name.LocalName, p => p.Value);
        }
        public void RemoveChild(string id)
        {
            JsonChild child = children.Where(p => p.id == id).FirstOrDefault();
            children.Remove(child);
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(JsonElement other)
        {
            if (other == null) return false;
            if ((other.name ?? "null") != (name ?? "null")) return false;
            if ((other.type ?? "null") != (type ?? "null")) return false;

            if ((other.attributes == null ? "" : other.attributes.OrderBy(p=>p.Key).Select(p => p.Key + ":" + p.Value).StringConcatenate(";")) != (attributes == null ? "" : attributes.OrderBy(p => p.Key).Select(p => p.Key + ":" + p.Value).StringConcatenate(";"))) return false;
            if ((other.children == null ? "" : other.children.Select(p => (p.id == null ? p.text : p.id)).StringConcatenate(";")) != (children == null ? "" : children.Select(p => (p.id == null ? p.text : p.id)).StringConcatenate())) return false;
            return true;
        }

        
        public override int GetHashCode()
        {
            return HashCode.Combine(attributes, children, name, type);
        }
    }
    public class JsonChild
    {
        public string element { get; set; }
        public string text { get; set; }
        public string id { get; set; }
        public JsonElementHierarcy build { set; get; }
    }
    
    public class ViewElement
    {
        public string name { get; set; }
        public string ob { get; set; }
        public string id { get; set; }
        public List<string> children { get; set; }
    }
    
    public class TocJson
    {
        public List<JsonChild> tocroot { get; set; }

        public Dictionary<string, JsonToc> toc { get; set; }
        public XElement items { get; set; }
        public TocJson(XElement container)
        {
            items = new XElement("items");
            items.Add(container.Elements().Where(p => p.DescendantsAndSelf().Where(d => d.IsHeaderName()).Count() > 0).HeaderToToc());
            toc = new Dictionary<string, JsonToc>();
            toc.AddRange(items
                .Descendants()
                .Select(p => p)
                .ToDictionary(p => (string)p.Attributes("id").FirstOrDefault(), p => new JsonToc(p))
            );
        }
        public TocJson(XElement document, string resourceId, string segmentId)
        {
            items = new XElement("items");
            items.Add(
                document
                .Elements()
                .Where(p => p.Descendants().Where(d => d.IsHeaderName()).Count() > 0)
                .HeaderToToc());
            toc = new Dictionary<string, JsonToc>();
            if (segmentId =="")
            {
                string toc_id = ("toc_" + resourceId + ";" + segmentId).ToLower();
                toc.Add(toc_id, new JsonToc { type = "toc", children = items.Elements().Select(p => new JsonChild { id = (string)p.Attributes("id").FirstOrDefault() }).ToList() });
            }

            toc.AddRange(items
                .Descendants()
                .Select(p => p)
                .ToDictionary(p => (string)p.Attributes("id").FirstOrDefault(), p => new JsonToc(p))
            );
        }

        public TocJson(XElement map, XElement document, string resourc_id, string segment_id)
        {
            items = new XElement("items"); 
            segment_id = segment_id == null ? "" : segment_id;

            if (segment_id == "")
            {
                items.Add(document.Elements().Where(p => p.Descendants().Where(d => d.IsHeaderName()).Count() > 0).HeaderToToc());
            }
            else
            {
                items.Add(map.Elements().GetTocSegment(segment_id, document));
            }
            string toc_id = ("toc_" + resourc_id + ";" + segment_id).ToLower();
            tocroot = new List<JsonChild>();
            tocroot.Add(new JsonChild { id = toc_id });
            toc = new Dictionary<string, JsonToc>();
            toc.Add(toc_id, new JsonToc {type="toc", children = items.Elements().Select(p => new JsonChild { id = (string)p.Attributes("id").FirstOrDefault() }).ToList() });
            toc.AddRange(
                items
                .Descendants()
                .Select(p => p)
                .ToDictionary(p => (string)p.Attributes("id").FirstOrDefault(), p => new JsonToc(p))
            );
        }

    }
 
}
