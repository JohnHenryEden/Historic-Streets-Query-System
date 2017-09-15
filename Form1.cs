using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Net;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.ArcCatalogUI;
using ESRI.ArcGIS.Catalog;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Server;
using ESRI.ArcGIS.DisplayUI;
using ESRI.ArcGIS.Controls;


namespace OldCityBuild0
{
    public partial class Form1 : Form
    {
        
        public Form1()
        {
            InitializeComponent();
        }

        ISimpleFillSymbol pSFillSymbol = new SimpleFillSymbol();
        ISimpleLineSymbol pSLineSymbol = new SimpleLineSymbol();
        IGeometry pGeometry;
        object pSFS;
        object pSLS;
        int flag;
        HTMLPopupDisplayClass pHTMLDisplay = new HTMLPopupDisplayClass();
        PopupWindow pPopupWindow;

        private void Form1_Load(object sender, EventArgs e)
        {
            axTOCControl1.SetBuddyControl(axMapControl1);
            axToolbarControl1.SetBuddyControl(axMapControl1);
            axMapControl2.Map = axMapControl1.Map;
        }

        /// <summary>
        /// 显示关于信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("由 黄靖珩 于 华南理工大学亚热带建筑实验室GIS子实验室 制作\r\n\r\n按照GPL许可证发布    联系方式:1006917341@qq.com", "关于",MessageBoxButtons.OK,MessageBoxIcon.Information);
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dresult = MessageBox.Show("确定要退出吗？", "退出", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dresult == System.Windows.Forms.DialogResult.Yes) 
            {
                Application.Exit();
            }
        }

        private void 打开街巷数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string MapDocPath;
            this.openFileDialog1.InitialDirectory = Application.StartupPath;
            this.openFileDialog1.Filter = "地图文档|*.mxd";
            if (this.openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK) 
            {
                MapDocPath = this.openFileDialog1.FileName;
                this.axMapControl1.LoadMxFile(MapDocPath);
                this.axMapControl2.LoadMxFile(MapDocPath);
                this.axMapControl2.Extent = this.axMapControl1.FullExtent;
                axMapControl2.Map.Layer[0].Visible = false;
            }
        }

        private void axMapControl2_OnMouseDown(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseDownEvent e)
        {
            IPoint pPoint = new ESRI.ArcGIS.Geometry.Point();
            pPoint.PutCoords(e.mapX, e.mapY);
            this.axMapControl1.CenterAt(pPoint);
            this.axMapControl1.Extent.CenterAt(pPoint);
            IRgbColor pColor = new RgbColor();
            pColor.Transparency = 0;
            pSFillSymbol.Color = pColor;
            pSLineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
            pColor.Red = 255;
            pColor.Transparency = 255;
            pSLineSymbol.Color = pColor;
            pSFillSymbol.Outline = pSLineSymbol;
            pGeometry = this.axMapControl1.ActiveView.Extent;
            pSFS = pSFillSymbol as object;
            this.axMapControl2.DrawShape(pGeometry, ref pSFS);
        }

        private void axMapControl1_OnExtentUpdated(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnExtentUpdatedEvent e)
        {
            axMapControl2.Refresh();
        }

        private void axMapControl2_OnMouseUp(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseUpEvent e)
        {
            this.axMapControl2.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
        }

        private void 按照点击位置查询ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                IFeatureLayer pFeatureLayer = this.axMapControl1.Map.Layer[0] as FeatureLayer;
                IFeature pFeature = pFeatureLayer.FeatureClass as IFeature;
                IHTMLPopupInfo pHTMLPopupInfo = pFeatureLayer as IHTMLPopupInfo;
                pHTMLPopupInfo.HTMLPopupEnabled = true;
                pHTMLPopupInfo.HTMLPresentationStyle = esriHTMLPopupStyle.esriHTMLPopupStyleXSLStylesheet;
                pHTMLPopupInfo.HTMLXSLStylesheet = Application.StartupPath + @"\popup_0.xsl";
                this.axMapControl1.MousePointer = ESRI.ArcGIS.Controls.esriControlsMousePointer.esriPointerIdentify;
                flag = 1;
            }
            catch
            {
                MessageBox.Show("数据尚未加载！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 点选查询
        /// </summary>
        /// <param name="pPoint">空间点</param>
        /// <param name="pFeatureLayer">操作图层</param>
        /// <param name="pRadius">缓冲区半径</param>
        /// <returns>返回查找结果</returns>
        public IFeature GetPointSelect(IPoint pPoint, IFeatureLayer pFeatureLayer, double pRadius)
        {
            if (pPoint != null && pFeatureLayer != null)
            {
                ITopologicalOperator pTopologicalOperator = pPoint as ITopologicalOperator;
                IGeometry pGeometry = pTopologicalOperator.Buffer(pRadius);//建立缓冲区
                ISpatialFilter pSpatialFilter = new SpatialFilterClass();
                pSpatialFilter.Geometry = pGeometry;
                pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;//求交
                
                IFeatureCursor pFeatureCursor = pFeatureLayer.FeatureClass.Search(pSpatialFilter, false);//空间查询
                IFeature pFeature = pFeatureCursor.NextFeature();
                
                if (pFeature != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureCursor);//释放缓存
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(pSpatialFilter);//释放缓存
                    pGeometry = pFeature.Shape;
                    return pFeature;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 左键在flag=1时点选道路进行查询，右键将地图中心移至点击位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void axMapControl1_OnMouseDown(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseDownEvent e)
        {
            if (e.button == 1)
            {
                if (flag == 1)
                {
                    if (pPopupWindow != null)
                    {
                        if (pPopupWindow.Visible == true)
                        {
                            pPopupWindow.Dispose();
                        }
                    }

                    IPoint pPoint = new ESRI.ArcGIS.Geometry.Point();
                    pPoint.PutCoords(e.mapX, e.mapY);
                    IFeatureLayer pFeatureLayer = this.axMapControl1.Map.Layer[0] as FeatureLayer;
                    IFeature pFeature = GetPointSelect(pPoint, pFeatureLayer, 16) as IFeature;
                    IPropertySet pOptions = new PropertySet();

                    IHTMLPopupInfo pHTMLPopupInfo = pFeatureLayer as IHTMLPopupInfo;
                    IHTMLPopupInfo2 pHTMLPopupInfo2 = pFeatureLayer as IHTMLPopupInfo2;
                    pHTMLPopupInfo2.HTMLDownloadAttachmentData = true;
                    pHTMLPopupInfo.HTMLPopupEnabled = true;
                    pHTMLPopupInfo.HTMLPresentationStyle = esriHTMLPopupStyle.esriHTMLPopupStyleXSLStylesheet;
                    pHTMLPopupInfo.HTMLXSLStylesheet = Application.StartupPath + @"\popup_0.xsl";
                    this.axMapControl1.MousePointer = ESRI.ArcGIS.Controls.esriControlsMousePointer.esriPointerIdentify;
                    if (pFeature != null)
                    {
                        axMapControl1.ActiveView.Refresh();
                        IGeometry pAnnoGeometry = pFeature.Shape;
                        pSLS = pSLineSymbol as object;
                        pSLineSymbol.Style = esriSimpleLineStyle.esriSLSSolid;
                        IRgbColor pColor = new RgbColor();
                        pColor.Red = 255;
                        pColor.Transparency = 255;
                        pSLineSymbol.Color = pColor;
                        axMapControl1.DrawShape(pAnnoGeometry, ref pSLS);
                        displayXMLdocumentContent(pHTMLPopupInfo2.HTMLOutput(pFeature, pOptions));
                    }
                }
            }
            if (e.button == 2)
            {
                IPoint pPoint = new ESRI.ArcGIS.Geometry.Point();
                pPoint.PutCoords(e.mapX, e.mapY);
                this.axMapControl1.MousePointer = esriControlsMousePointer.esriPointerHand;
                axMapControl1.CenterAt(pPoint);
            }
        }

        /// <summary>
        /// 解析并显示XML表格
        /// </summary>
        /// <param name="XML">XML字符串</param>
        private void displayXMLdocumentContent(string XML) 
        {
            XmlDocument pXmlDoc = new XmlDocument();
            pXmlDoc.LoadXml(XML);
            //XmlDocument pData = new XmlDocument();
            XmlNodeList pXmlNodeList = pXmlDoc.SelectNodes("//Field");
            XmlNodeList pAttachList = pXmlDoc.SelectNodes("//Attachment");
            if (pXmlNodeList != null && pAttachList != null)
            {
                List<string> FieldNames = new List<string>();
                List<string> FieldValues = new List<string>();
                List<string> AttachmentsLinks = new List<string>(); 
                foreach (XmlNode pXmlNode in pXmlNodeList)
                {
                    FieldNames.Add(pXmlNode.SelectSingleNode("FieldName").InnerText);
                    FieldValues.Add(pXmlNode.SelectSingleNode("FieldValue").InnerText);
                    
                }
                foreach (XmlNode attachNode in pAttachList)
                {
                    AttachmentsLinks.Add(attachNode.SelectSingleNode("FilePath").InnerText);
                }
                pPopupWindow = new PopupWindow();
                pHTMLDisplay.FieldNames = FieldNames;
                pHTMLDisplay.FieldValues = FieldValues;
                pHTMLDisplay.AttachLinks = AttachmentsLinks;
                pPopupWindow.pHtmlPopupDisplay = pHTMLDisplay;
                pPopupWindow.Show();
                this.Activate();
                pPopupWindow.BringToFront();
            }
        }

        private void 停止点击查询ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            flag = 0;
            this.axMapControl1.MousePointer = ESRI.ArcGIS.Controls.esriControlsMousePointer.esriPointerArrow;
        }

        private void 帮助文档ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("帮助：\r\n点击“文件”选项可以选择打开地图文档（*.mxd）或者退出；\r\n点击“查询操作”选项可以开始查询；\r\n点击“帮助和关于”选项可以获得本文档和作者信息。\r\n展开内容列表中的矢量数据项，可以双击修改符号","帮助",MessageBoxButtons.OK,MessageBoxIcon.Information);
        }

        private void axTOCControl1_OnMouseDown(object sender, ESRI.ArcGIS.Controls.ITOCControlEvents_OnMouseDownEvent e)
        {

        }

        /// <summary>
        /// 双击TOC修改符号系统
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void axTOCControl1_OnDoubleClick(object sender, ESRI.ArcGIS.Controls.ITOCControlEvents_OnDoubleClickEvent e)
        {
            esriTOCControlItem pTocControlItem = esriTOCControlItem.esriTOCControlItemNone;
            ILayer pLayer = null;
            IBasicMap pBasicMap = null;
            object unk = null;
            object data = null;
            if (e.button == 1)
            {
                axTOCControl1.HitTest(e.x, e.y, ref pTocControlItem, ref pBasicMap, ref pLayer, ref unk, ref data);
                System.Drawing.Point pos = new System.Drawing.Point(e.x, e.y);
                if (pTocControlItem == esriTOCControlItem.esriTOCControlItemLegendClass)
                {
                    ILegendClass pLegendClass = new LegendClass();
                    ILegendGroup pLegendGroup = new LegendGroup();
                    if (unk is ILegendGroup)
                    {
                        pLegendGroup = unk as ILegendGroup;
                    }
                    pLegendClass = pLegendGroup.get_Class((int)data);
                    ISymbol pSymbol;
                    pSymbol = pLegendClass.Symbol;
                    ISymbolSelector pSymbolSelector = new SymbolSelector();
                    bool isOk = false;
                    pSymbolSelector.AddSymbol(pSymbol);
                    isOk = pSymbolSelector.SelectSymbol(0);
                    if (isOk)
                    {
                        pLegendClass.Symbol = pSymbolSelector.GetSymbolAt(0);
                    }
                    this.axMapControl1.ActiveView.Refresh();
                    this.axTOCControl1.Refresh();
                }
            }
        }
    }
}